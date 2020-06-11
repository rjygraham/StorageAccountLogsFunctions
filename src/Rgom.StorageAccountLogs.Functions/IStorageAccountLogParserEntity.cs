using System.Threading.Tasks;

namespace Rgom.StorageAccountLogs.Functions
{
	public interface IStorageAccountLogParserEntity
	{
		void Configure();
		Task ProcessLogsAsync();
		void Delete();
	}
}
