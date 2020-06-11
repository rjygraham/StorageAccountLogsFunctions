using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Newtonsoft.Json.Linq;
using Rgom.StorageAccountLogs.Functions.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Rgom.StorageAccountLogs.Functions
{
	public static class HttpConfigurationFunctions
	{
		[FunctionName(nameof(AddStorageAccountAsync))]
		public static async Task<IActionResult> AddStorageAccountAsync(
			[HttpTrigger(AuthorizationLevel.Function, methods: "post", Route = "ochestrator/{storageAccountName}")] StorageAccountConfiguration model,
			[DurableClient] IDurableEntityClient client,
			string storageAccountName
		)
		{

			var config = ValidateConfiguration(model);
			
			if (config == null)
			{
				return new BadRequestResult();
			}

			var entityId = new EntityId(nameof(StorageAccountOrchestratorEntity), "singleton");
			await client.SignalEntityAsync<IStorageAccountOrchestratorEntity>(entityId, proxy => proxy.AddStorageAccount(new Tuple<string, LogProcessingConfiguration>(storageAccountName, config)));

			return new OkResult();
		}

		[FunctionName(nameof(RemoveStorageAccountAsync))]
		public static async Task RemoveStorageAccountAsync(
			[HttpTrigger(AuthorizationLevel.Function, methods: "delete", Route = "ochestrator/{storageAccountName}")] HttpRequestMessage req,
			[DurableClient] IDurableEntityClient client,
			string storageAccountName
		)
		{
			var entityId = new EntityId(nameof(StorageAccountOrchestratorEntity), "singleton");
			await client.SignalEntityAsync<IStorageAccountOrchestratorEntity>(entityId, proxy => proxy.RemoveStorageAccount(storageAccountName));
		}

		[FunctionName(nameof(ProcessLogsAsync))]
		public static async Task ProcessLogsAsync(
			[HttpTrigger(AuthorizationLevel.Function, methods: "post", Route = "ochestrator/actions/processlogs")] HttpRequestMessage req,
			[DurableClient] IDurableEntityClient client
		)
		{
			var entityId = new EntityId(nameof(StorageAccountOrchestratorEntity), "singleton");
			await client.SignalEntityAsync<IStorageAccountOrchestratorEntity>(entityId, async proxy => await proxy.ProcessLogsAsync());
		}

		[FunctionName(nameof(ListStorageAccountsAsync))]
		public static async Task<IActionResult> ListStorageAccountsAsync(
			[HttpTrigger(AuthorizationLevel.Function, methods: "get", Route = "ochestrator")] HttpRequestMessage req,
			[DurableClient] IDurableEntityClient client
		)
		{
			var entityId = new EntityId(nameof(StorageAccountOrchestratorEntity), "singleton");
			var state = await client.ReadEntityStateAsync<StorageAccountOrchestratorEntity>(entityId);
			return new OkObjectResult(state.EntityState.StorageAccounts);
		}

		[FunctionName(nameof(SetStorageAccountConfigurationAsync))]
		public static async Task<IActionResult> SetStorageAccountConfigurationAsync(
			[HttpTrigger(AuthorizationLevel.Function, methods: "post", Route = "storage/{storageAccountName}/configuration")] StorageAccountConfiguration model,
			[DurableClient] IDurableEntityClient client,
			string storageAccountName
		)
		{
			var config = ValidateConfiguration(model);

			if (config == null)
			{
				return new BadRequestResult();
			}

			var entityId = new EntityId(nameof(StorageAccountLogParserEntity), storageAccountName);
			await client.SignalEntityAsync<IStorageAccountLogParserEntity>(entityId, proxy => proxy.Configure(config));

			return new OkResult();
		}

		[FunctionName(nameof(GetStorageAccountConfigurationAsync))]
		public static async Task<IActionResult> GetStorageAccountConfigurationAsync(
			[HttpTrigger(AuthorizationLevel.Function, methods: "get", Route = "storage/{storageAccountName}/configuration")] HttpRequestMessage req,
			[DurableClient] IDurableEntityClient client,
			string storageAccountName
		)
		{
			var entityId = new EntityId(nameof(StorageAccountLogParserEntity), storageAccountName);
			var state = await client.ReadEntityStateAsync<StorageAccountLogParserEntity>(entityId);

			if (state.EntityState == null)
			{
				return new NotFoundResult();
			}

			var config = state.EntityState.LogProcessingConfiguration;

			var result = new StorageAccountConfiguration
			{
				IgnoredApplicationIds = config.IgnoredApplicationIds == null ? new string[] { } : config.IgnoredApplicationIds.ToArray(),
				IgnoredContainers = config.IgnoredContainers == null ? new string[] { } : config.IgnoredContainers.ToArray(),
				IgnoredIps = config.IgnoredIps == null ? new string[] { } : config.IgnoredIps.ToArray(),
				IgnoredOperationTypes = config.IgnoredOperationTypes == null ? new string[] { } : config.IgnoredOperationTypes.ToArray(),
				IgnoredPrincipals = config.IgnoredPrincipals == null ? new string[] { } : config.IgnoredPrincipals.ToArray(),
				LogAnalyticsKey = string.IsNullOrWhiteSpace(config.LogAnalyticsKey)
					? null
					: "***",
				LogAnalyticsTable = string.IsNullOrWhiteSpace(config.LogAnalyticsTable)
					? null
					: config.LogAnalyticsTable,
				LogAnalyticsWorkspaceId = string.IsNullOrWhiteSpace(config.LogAnalyticsWorkspaceId)
					? null
					: config.LogAnalyticsWorkspaceId,
				LogProcessingMode = config.LogProcessingMode,
				ShouldAlwaysLogAnonymousRequests = config.ShouldAlwaysLogAnonymousRequests
			};

			return new OkObjectResult(result);
		}

		private static LogProcessingConfiguration ValidateConfiguration(StorageAccountConfiguration model)
		{
			if (
				(string.IsNullOrWhiteSpace(model.LogAnalyticsWorkspaceId) && !string.IsNullOrWhiteSpace(model.LogAnalyticsKey))
				|| (!string.IsNullOrWhiteSpace(model.LogAnalyticsWorkspaceId) && string.IsNullOrWhiteSpace(model.LogAnalyticsKey))
			)
			{
				return null;
			}

			var config = new LogProcessingConfiguration();
			config.IgnoredApplicationIds = model.IgnoredApplicationIds == null ? new HashSet<string>() : new HashSet<string>(model.IgnoredApplicationIds);
			config.IgnoredApplicationIds = model.IgnoredApplicationIds == null ? new HashSet<string>() : new HashSet<string>(model.IgnoredApplicationIds);
			config.IgnoredContainers = model.IgnoredContainers == null ? new HashSet<string>() : new HashSet<string>(model.IgnoredContainers);
			config.IgnoredIps = model.IgnoredIps == null ? new HashSet<string>() : new HashSet<string>(model.IgnoredIps);
			config.IgnoredOperationTypes = model.IgnoredOperationTypes == null ? new HashSet<string>() : new HashSet<string>(model.IgnoredOperationTypes);
			config.IgnoredPrincipals = model.IgnoredPrincipals == null ? new HashSet<string>() : new HashSet<string>(model.IgnoredPrincipals);
			config.LogAnalyticsKey = string.IsNullOrWhiteSpace(model.LogAnalyticsKey) ? null : model.LogAnalyticsKey;
			config.LogAnalyticsTable = string.IsNullOrWhiteSpace(model.LogAnalyticsTable) ? null : model.LogAnalyticsTable;
			config.LogAnalyticsWorkspaceId = string.IsNullOrWhiteSpace(model.LogAnalyticsWorkspaceId) ? null : model.LogAnalyticsWorkspaceId;
			config.LogProcessingMode = model.LogProcessingMode;
			config.ShouldAlwaysLogAnonymousRequests = model.ShouldAlwaysLogAnonymousRequests;

			return config;
		}
	}
}
