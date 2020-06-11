#if DEBUG

using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using StorageEventFunctions.Models;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace StorageEventFunctions
{
	public static class LocalTestFunctions
	{
		[FunctionName(nameof(AddStorageAccount))]
		public static async Task AddStorageAccount(
			[HttpTrigger(AuthorizationLevel.Function, methods: "post", Route = "{storageAccountName}")] HttpRequestMessage req,
			[DurableClient] IDurableEntityClient client,
			string storageAccountName
		)
		{
			var entityId = new EntityId(nameof(StorageAccountOrchestratorEntity), "singleton");
			await client.SignalEntityAsync<IStorageAccountOrchestratorEntity>(entityId, proxy => proxy.AddStorageAccount(storageAccountName));
		}

		[FunctionName(nameof(RemoveStorageAccount))]
		public static async Task RemoveStorageAccount(
			[HttpTrigger(AuthorizationLevel.Function, methods: "delete", Route = "{storageAccountName}")] HttpRequestMessage req,
			[DurableClient] IDurableEntityClient client,
			string storageAccountName
		)
		{
			var entityId = new EntityId(nameof(StorageAccountOrchestratorEntity), "singleton");
			await client.SignalEntityAsync<IStorageAccountOrchestratorEntity>(entityId, proxy => proxy.RemoveStorageAccount(storageAccountName));
		}

		[FunctionName(nameof(GetStorageAccounts))]
		public static async Task<IActionResult> GetStorageAccounts(
			[HttpTrigger(AuthorizationLevel.Function, methods: "get")] HttpRequestMessage req,
			[DurableClient] IDurableEntityClient client
		)
		{
			var entityId = new EntityId(nameof(StorageAccountOrchestratorEntity), "singleton");
			var state = await client.ReadEntityStateAsync<StorageAccountOrchestratorEntity>(entityId);
			return new OkObjectResult(state.EntityState.StorageAccounts);
		}


		[FunctionName(nameof(ProcessLogsAsync))]
		public static async Task ProcessLogsAsync(
			[HttpTrigger(AuthorizationLevel.Function, methods: "post", Route = "actions/processlogs")] HttpRequestMessage req,
			[DurableClient] IDurableEntityClient client
		)
		{
			var model = new LogAnalyticsModel(
				Environment.GetEnvironmentVariable("LogAnalyticsWorkspaceId"),
				Environment.GetEnvironmentVariable("LogAnalyticsKey"),
				Environment.GetEnvironmentVariable("LogAnalyticsTableName")
			);

			var entityId = new EntityId(nameof(StorageAccountOrchestratorEntity), "singleton");
			await client.SignalEntityAsync<IStorageAccountOrchestratorEntity>(entityId, async proxy => await proxy.ProcessLogsAsync(model));
		}
	}
}

#endif
