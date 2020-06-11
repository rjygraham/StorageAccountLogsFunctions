using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Rgom.StorageAccountLogs.Functions.Services;
using System;
using System.Threading.Tasks;

namespace Rgom.StorageAccountLogs.Functions
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
			var entityId = new EntityId(nameof(StorageAccountOrchestratorEntity), "singleton");
			await client.SignalEntityAsync<IStorageAccountOrchestratorEntity>(entityId, async proxy => await proxy.ProcessLogsAsync());
		}
	}
}
