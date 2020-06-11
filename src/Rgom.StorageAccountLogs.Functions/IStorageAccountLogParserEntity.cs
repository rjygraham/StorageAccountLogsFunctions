using StorageEventFunctions.Models;
using System.Threading.Tasks;

namespace StorageEventFunctions
{
	public interface IStorageAccountLogParserEntity
	{
		void Configure();
		Task ProcessLogsAsync(LogAnalyticsModel model);
		void Delete();
	}
}
