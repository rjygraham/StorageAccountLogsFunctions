using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Auth;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Newtonsoft.Json;
using StorageEventFunctions.Models;
using StorageEventFunctions.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StorageEventFunctions
{
	[JsonObject(MemberSerialization.OptIn)]
	public class StorageAccountLogParserEntity : IStorageAccountLogParserEntity
	{
		[JsonIgnore]
		private readonly ICredentialService credentialService;

		[JsonProperty("lastLogEntryProcessedTime")]
		public DateTime LastLogEntryProcessedTime { get; set; }

		[JsonProperty("logProcessingMode")]
		public LogProcessingMode LogProcessingMode { get; set; }

		[JsonProperty("safeIps")]
		public HashSet<string> safeIps { get; set; }

		[JsonProperty("safePrincipals")]
		public HashSet<string> safePrincipals { get; set; }

		[JsonProperty("Locked")]
		public bool Locked { get; set; }

		public StorageAccountLogParserEntity(ICredentialService credentialService)
		{
			this.credentialService = credentialService;
		}

		public void Configure()
		{
			// TODO: Add configuration to control how this Storage Account is processed.
		}

		public async Task ProcessLogsAsync(LogAnalyticsModel model)
		{
			if (!Locked)
			{
				Locked = true;
				Entity.Current.SetState(this);

				var tokenCredentials = await credentialService.GetTokenCredentialsAsync();
				var storageCredentials = new StorageCredentials(tokenCredentials);
				var storageAccount = new CloudStorageAccount(storageCredentials, Entity.Current.EntityKey, "core.windows.net", true);

				var processor = new LogProcessor(storageAccount, model.WorkspaceId, model.Key, model.TableName);
				LastLogEntryProcessedTime = await processor.ProcessLogsAsync(LastLogEntryProcessedTime, LogProcessingMode);

				Locked = false;
				Entity.Current.SetState(this);
			}
		}

		public void Delete()
		{
			Entity.Current.DeleteState();
		}

		[FunctionName(nameof(StorageAccountLogParserEntity))]
		public static Task Run([EntityTrigger] IDurableEntityContext ctx)
		{
			if (!ctx.HasState)
			{
				ctx.SetState(new StorageAccountLogParserEntity(null)
				{
					LastLogEntryProcessedTime = DateTime.MinValue.ToUniversalTime(),
					LogProcessingMode = LogProcessingMode.Complete
				});
			}

			return ctx.DispatchAsync<StorageAccountLogParserEntity>();
		}
	}
}
