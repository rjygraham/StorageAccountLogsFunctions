using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Auth;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Newtonsoft.Json;
using Rgom.StorageAccountLogs.Functions.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rgom.StorageAccountLogs.Functions
{
	[JsonObject(MemberSerialization.OptIn)]
	public class StorageAccountLogParserEntity : IStorageAccountLogParserEntity
	{
		[JsonIgnore]
		private readonly ICredentialService credentialService;

		[JsonIgnore]
		private readonly ILogAnalyticsConfigurationService logAnalyticsConfigurationService;

		[JsonProperty("lastLogEntryProcessedTime")]
		public DateTime LastLogEntryProcessedTime { get; set; }

		[JsonProperty("logProcessingConfiguration")]
		public LogProcessingConfiguration LogProcessingConfiguration { get; set; }

		[JsonProperty("Locked")]
		public bool Locked { get; set; }

		public StorageAccountLogParserEntity(ICredentialService credentialService, ILogAnalyticsConfigurationService logAnalyticsConfigurationService)
		{
			this.credentialService = credentialService;
			this.logAnalyticsConfigurationService = logAnalyticsConfigurationService;
		}

		public void Configure(LogProcessingConfiguration logProcessingConfiguration)
		{
			LogProcessingConfiguration = logProcessingConfiguration;
		}

		public async Task ProcessLogsAsync()
		{
			if (!Locked)
			{
				Locked = true;
				Entity.Current.SetState(this);

				var tokenCredentials = await credentialService.GetTokenCredentialsAsync();
				var storageCredentials = new StorageCredentials(tokenCredentials);
				var storageAccount = new CloudStorageAccount(storageCredentials, Entity.Current.EntityKey, "core.windows.net", true);

				// Map 
				var config = new LogProcessingConfiguration
				{
					IgnoredApplicationIds = LogProcessingConfiguration.IgnoredApplicationIds,
					IgnoredContainers = LogProcessingConfiguration.IgnoredContainers,
					IgnoredIps = LogProcessingConfiguration.IgnoredIps,
					IgnoredOperationTypes = LogProcessingConfiguration.IgnoredOperationTypes,
					IgnoredPrincipals = LogProcessingConfiguration.IgnoredPrincipals,

					LogAnalyticsKey = string.IsNullOrWhiteSpace(LogProcessingConfiguration.LogAnalyticsKey)
						? logAnalyticsConfigurationService.Key
						: LogProcessingConfiguration.LogAnalyticsKey,

					LogAnalyticsTable = string.IsNullOrWhiteSpace(LogProcessingConfiguration.LogAnalyticsTable)
						? logAnalyticsConfigurationService.TableName
						: LogProcessingConfiguration.LogAnalyticsTable,

					LogAnalyticsWorkspaceId = string.IsNullOrWhiteSpace(LogProcessingConfiguration.LogAnalyticsWorkspaceId)
						? logAnalyticsConfigurationService.WorkspaceId
						: LogProcessingConfiguration.LogAnalyticsWorkspaceId,

					LogProcessingMode = LogProcessingConfiguration.LogProcessingMode,
					ShouldAlwaysLogAnonymousRequests = LogProcessingConfiguration.ShouldAlwaysLogAnonymousRequests
				};

				var processor = new LogProcessor(storageAccount, config);
				LastLogEntryProcessedTime = await processor.ProcessLogsAsync(LastLogEntryProcessedTime);

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
				// Setup the logging with some commonsense defaults.
				ctx.SetState(new StorageAccountLogParserEntity(null, null)
				{
					LastLogEntryProcessedTime = new DateTime(1970, 1, 1).ToUniversalTime()
					//LogProcessingConfiguration = new LogProcessingConfiguration
					//{
					//	LogProcessingMode = LogProcessingMode.Complete,
					//	ShouldAlwaysLogAnonymousRequests = true,
					//	IgnoredContainers = new HashSet<string>(new string[] { "$logs" }),
					//	IgnoredOperationTypes = new HashSet<string>(new string[] { "GetContainerServiceMetadata", "SetContainerMetadata", "GetContainerProperties", "BlobPreflightRequest", "GetBlobMetadata", "GetBlobProperties" }),
					//	IgnoredApplicationIds = new HashSet<string>(),
					//	IgnoredIps = new HashSet<string>(),
					//	IgnoredPrincipals = new HashSet<string>()
					//}
				});
			}

			return ctx.DispatchAsync<StorageAccountLogParserEntity>();
		}
	}
}
