using System.Threading.Tasks;

namespace Rgom.StorageAccountLogs.Functions
{
	public interface IStorageAccountOrchestratorEntity
	{
		void AddStorageAccount(string storageAccountName);
		void RemoveStorageAccount(string storageAccountName);
		Task ProcessLogsAsync();
	}
}
