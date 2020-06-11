using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Newtonsoft.Json;
using StorageEventFunctions.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StorageEventFunctions
{
	[JsonObject(MemberSerialization.OptIn)]
	public class StorageAccountOrchestratorEntity : IStorageAccountOrchestratorEntity
	{
		[JsonProperty("storageAccounts")]
		public HashSet<string> StorageAccounts { get; set; }

		public void AddStorageAccount(string storageAccountName)
		{
			if (!StorageAccounts.Contains(storageAccountName))
			{
				var entityId = new EntityId(nameof(StorageAccountLogParserEntity), storageAccountName);
				Entity.Current.SignalEntity<IStorageAccountLogParserEntity>(entityId, proxy => proxy.Configure());
				StorageAccounts.Add(storageAccountName);
			}
		}

		public void RemoveStorageAccount(string storageAccountName)
		{
			if (StorageAccounts.Contains(storageAccountName))
			{
				var entityId = new EntityId(nameof(StorageAccountLogParserEntity), storageAccountName);
				Entity.Current.SignalEntity<IStorageAccountLogParserEntity>(entityId, proxy => proxy.Delete());
				StorageAccounts.Remove(storageAccountName);
			}
		}

		public async Task ProcessLogsAsync(LogAnalyticsModel model)
		{
			if (StorageAccounts == null)
			{
				return;
			}

			foreach (var storageAccountName in StorageAccounts)
			{
				var entityId = new EntityId(nameof(StorageAccountLogParserEntity), storageAccountName);
				Entity.Current.SignalEntity<IStorageAccountLogParserEntity>(entityId, async proxy => await proxy.ProcessLogsAsync(model));
			}
		}

		[FunctionName(nameof(StorageAccountOrchestratorEntity))]
		public static Task Run([EntityTrigger] IDurableEntityContext ctx)
		{
			if (!ctx.HasState)
			{
				ctx.SetState(new StorageAccountOrchestratorEntity { StorageAccounts = new HashSet<string>() });
			}

			return ctx.DispatchAsync<StorageAccountOrchestratorEntity>();
		}
	}
}
