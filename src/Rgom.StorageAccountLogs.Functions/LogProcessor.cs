using CsvHelper;
using CsvHelper.Configuration;
using HTTPDataCollectorAPI;
using Microsoft.Azure.Storage;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace StorageEventFunctions.Models
{
	internal class LogProcessor
	{
		private static CsvConfiguration config;

		static LogProcessor()
		{
			config = new CsvConfiguration(CultureInfo.InvariantCulture)
			{
				Delimiter = ";",
				Escape = '"',
				IgnoreQuotes = false,
				HasHeaderRecord = false,
			};

			config.RegisterClassMap<StorageAccountLogMap>();
		}

		private readonly ConcurrentQueue<string> rawLogMessages = new ConcurrentQueue<string>();
		private readonly ConcurrentQueue<LogMessage> evaluatedLogMessages = new ConcurrentQueue<LogMessage>();
		private readonly ConcurrentBag<string> sentinelIgnoredIps = new ConcurrentBag<string>();

		private readonly CloudStorageAccount storageAccount;
		private readonly Collector collector;
		private readonly string logAnalyticsTableName;

		private Task getLogsTask;
		private Task<DateTime> evaluateLogsTask;

		public LogProcessor(CloudStorageAccount storageAccount, string logAnalyticsWorkspaceId, string logAnalyticsKey, string logAnalyticsTableName)
		{
			this.storageAccount = storageAccount;
			this.collector = new Collector(logAnalyticsWorkspaceId, logAnalyticsKey);
			this.logAnalyticsTableName = logAnalyticsTableName;
		}

		public async Task<DateTime> ProcessLogsAsync(DateTime lastLogEntryProcessedTime, LogProcessingMode logProcessingMode)
		{
			getLogsTask = Task.Run(() => GetLogs(lastLogEntryProcessedTime));
			evaluateLogsTask = Task.Run(() => EvaluateLogs(lastLogEntryProcessedTime, logProcessingMode));

			using (var exitEvent = new ManualResetEvent(false))
			{
				var collectLogsTask = Task.Run(async () =>
				{
					while (!evaluateLogsTask.IsCompleted && evaluatedLogMessages.Count >= 0)
					{
						exitEvent.WaitOne(30 * 1000);

						var messages = new List<LogMessage>();
						while (evaluatedLogMessages.TryDequeue(out LogMessage message))
						{
							messages.Add(message);
						}

						if (logProcessingMode == LogProcessingMode.Complete)
						{
							await CollectCompleteLogsAsync(messages);
						}
						else
						{
							await CollectSentinelLogsAsync(messages);
						}
					}
				});

				getLogsTask.Wait();
				evaluateLogsTask.Wait();
				exitEvent.Set();
				await collectLogsTask;

				return evaluateLogsTask.Result;
			}
		}

		private void GetLogs(DateTime lastLogEntryProcessedTime)
		{
			// By storing some state we can massively reduce the amount of log processing we do, here
			// we only retrieve logs that are recent
			var from = lastLogEntryProcessedTime == DateTime.MinValue.ToUniversalTime()
				? DateTime.MinValue.ToUniversalTime()
				: lastLogEntryProcessedTime.AddMinutes(-15);

			var to = DateTime.UtcNow.AddHours(1);

			var blobs = LogDownloader.DownloadStorageLogs(storageAccount, "blob", from, to);

			foreach (var blob in blobs)
			{
				// There is no point downloading more logs if the output queue is still full of data to process
				// we need to back off here and wait for the queue to reduce.
				while (rawLogMessages.Count >= 1000)
				{
					var rand = new Random();
					// back off a random time in ms.
					Task.Delay(rand.Next(0, 1000)).Wait();
				}

				try
				{
					using (var reader = new StreamReader(blob.OpenRead())) // underlying stream is closed by StreamReader
					{
						var text = reader.ReadToEnd();
						rawLogMessages.Enqueue(text);
					}
				}
				catch (Exception ex)
				{
					// TODO: managed exceptions
				}
			}
		}

		private DateTime EvaluateLogs(DateTime lastLogEntryProcessedTime, LogProcessingMode logProcessingMode)
		{
			DateTime newLastLogEntryProcessedTime = lastLogEntryProcessedTime;

			while (!getLogsTask.IsCompleted)
			{
				while (rawLogMessages.TryDequeue(out string item))
				{
					using (var sr = new StringReader(item))
					{
						using (var csv = new CsvReader(sr, config))
						{
							var logReference = new StorageAccountLog();
							var logs = csv.EnumerateRecords(logReference);
							foreach (var log in logs)
							{
								// first check to ensure we are not processing any old log entries
								// if we are we can bail out here
								if (log.RequestStartTime <= lastLogEntryProcessedTime)
								{
									continue;
								}

								newLastLogEntryProcessedTime = log.RequestStartTime;

								if (logProcessingMode == LogProcessingMode.Complete)
								{
									EvaluatedCompleteLog(log);
								}
								else
								{
									EvaluatedSentinelLog(log);
								}
							}
						}
					}
				}
			}

			return newLastLogEntryProcessedTime;
		}

		private void EvaluatedCompleteLog(StorageAccountLog log)
		{
			// TODO: Exclude logs from known IPs
			// TODO: Exclude logs from  known UPNs
			// TODO: Exclude logs with specific Operation Types

			evaluatedLogMessages.Enqueue(new LogMessage
			{
				RequestTime = log.RequestStartTime.ToString(),
				Url = log.RequestUrl,
				OriginIp = log.RequesterIpAddress.Substring(0, log.RequesterIpAddress.IndexOf(':')),
				RequestType = log.OperationType,
				UserAgent = log.UserAgentHeader,
				AuthenticationType = log.AuthenticationType,
				ApplicationId = log.ApplicationId,
				UserPrincipalName = log.UserPrincipalName
			});
		}

		private void EvaluatedSentinelLog(StorageAccountLog log)
		{
			// now we have a full logentry to process
			switch (log.OperationType)
			{
				case "PutBlob":
					var ip = log.RequesterIpAddress.Substring(0, log.RequesterIpAddress.IndexOf(':'));
					sentinelIgnoredIps.Add(ip);
					break;
				case "ListBlobs":
				case "ListContainers":
				case "GetBlob":
				case "GetBlobProperties":
				case "GetContainerProperties":
				case "GetContainerACL":
					var msg = new LogMessage
					{
						RequestTime = log.RequestStartTime.ToString(),
						Url = log.RequestUrl,
						OriginIp = log.RequesterIpAddress.Substring(0, log.RequesterIpAddress.IndexOf(':')),
						RequestType = log.OperationType,
						AuthenticationType = log.AuthenticationType,
						UserAgent = log.UserAgentHeader
					};

					if (sentinelIgnoredIps.Contains(msg.OriginIp))
					{
						// already ignored
						break;
					}

					//authentication-type
					var authtype = log.AuthenticationType;

					// Microsoft Azure Storage Explorer actively enumerates all the files in the
					// bucket & generates a lot of log entries. We want to ignore this because if someone has my sub in their
					// list and opens up this tool then I get a load of FPs
					if (msg.UserAgent.Contains("Microsoft Azure Storage Explorer") && authtype == "authenticated")
					{
						sentinelIgnoredIps.Add(msg.OriginIp);
					}
					// ignore any queries for $log or with null user agents. This is internal
					else if (msg.Url.Contains("$log") || string.IsNullOrWhiteSpace(msg.UserAgent))
					{
						sentinelIgnoredIps.Add(msg.OriginIp);
					}
					else
					{
						evaluatedLogMessages.Enqueue(msg);
					}
					break;
				default:
					break;
			}
		}

		private async Task CollectCompleteLogsAsync(List<LogMessage> messages)
		{
			if (messages.Count > 0)
			{
				await collector.Collect(logAnalyticsTableName, messages);
			}
		}

		private async Task CollectSentinelLogsAsync(List<LogMessage> messages)
		{
			var listOfIps = messages.Where(x => !sentinelIgnoredIps.Contains(x.OriginIp)).Distinct().ToList();

			if (listOfIps.Count() != 0)
			{
				try
				{
					await collector.Collect(logAnalyticsTableName, listOfIps);
				}
				catch (Exception ex)
				{
					return;
				}
			}
		}

	}
}
