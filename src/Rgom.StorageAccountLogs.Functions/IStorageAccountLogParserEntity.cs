using System.Threading.Tasks;

namespace Rgom.StorageAccountLogs.Functions
{
	public interface IStorageAccountLogParserEntity
	{
		void Configure(LogProcessingConfiguration logProcessingConfiguration);
		Task ProcessLogsAsync();
		void Delete();
	}
}
