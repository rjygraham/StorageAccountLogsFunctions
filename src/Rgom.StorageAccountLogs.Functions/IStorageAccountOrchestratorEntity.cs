using System;
using System.Threading.Tasks;

namespace Rgom.StorageAccountLogs.Functions
{
	public interface IStorageAccountOrchestratorEntity
	{
		void AddStorageAccount(Tuple<string, LogProcessingConfiguration> storageAccountConfig);
		void RemoveStorageAccount(string storageAccountName);
		Task ProcessLogsAsync();
	}
}
