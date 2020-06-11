using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using StorageEventFunctions.Models;
using System;
using System.Threading.Tasks;

namespace StorageEventFunctions
{
	public static class TimerTriggerFunctions
	{
		[FunctionName(nameof(HandleLogTimeAsync))]
		public static async Task HandleLogTimeAsync(
			[TimerTrigger("0 * * * * *")] TimerInfo timer,
			[DurableClient] IDurableEntityClient client,
			ILogger log
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
