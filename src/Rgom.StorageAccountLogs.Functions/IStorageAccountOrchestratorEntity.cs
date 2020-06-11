using StorageEventFunctions.Models;
using System.Threading.Tasks;

namespace StorageEventFunctions
{
	public interface IStorageAccountOrchestratorEntity
	{
		void AddStorageAccount(string storageAccountName);
		void RemoveStorageAccount(string storageAccountName);
		Task ProcessLogsAsync(LogAnalyticsModel model);
	}
}
