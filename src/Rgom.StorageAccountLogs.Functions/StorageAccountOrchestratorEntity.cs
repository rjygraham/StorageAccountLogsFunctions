using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rgom.StorageAccountLogs.Functions
{
	[JsonObject(MemberSerialization.OptIn)]
	public class StorageAccountOrchestratorEntity : IStorageAccountOrchestratorEntity
	{
		[JsonProperty("storageAccounts")]
		public HashSet<string> StorageAccounts { get; set; }

		public void AddStorageAccount(Tuple<string, LogProcessingConfiguration> storageAccountConfig)
		{
			if (!StorageAccounts.Contains(storageAccountConfig.Item1))
			{
				var entityId = new EntityId(nameof(StorageAccountLogParserEntity), storageAccountConfig.Item1);
				Entity.Current.SignalEntity<IStorageAccountLogParserEntity>(entityId, proxy => proxy.Configure(storageAccountConfig.Item2));
				StorageAccounts.Add(storageAccountConfig.Item1);
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

		public async Task ProcessLogsAsync()
		{
			if (StorageAccounts == null)
			{
				return;
			}

			foreach (var storageAccountName in StorageAccounts)
			{
				var entityId = new EntityId(nameof(StorageAccountLogParserEntity), storageAccountName);
				Entity.Current.SignalEntity<IStorageAccountLogParserEntity>(entityId, async proxy => await proxy.ProcessLogsAsync());
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
