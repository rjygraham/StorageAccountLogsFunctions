namespace Rgom.StorageAccountLogs.Functions.Services
{
	public class LogAnalyticsConfigurationService : ILogAnalyticsConfigurationService
	{
		public string WorkspaceId { get; private set; }
		public string Key { get; private set; }

		public string TableName { get; private set; }

		public LogAnalyticsConfigurationService(string workspaceId, string key, string tableName)
		{
			this.WorkspaceId = workspaceId;
			this.Key = key;
			this.TableName = tableName;
		}
	}
}
