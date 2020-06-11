namespace StorageEventFunctions.Models
{
	public class LogAnalyticsModel
	{
		public string WorkspaceId { get; private set; }
		public string Key { get; private set; }

		public string TableName { get; private set; }

		public LogAnalyticsModel(string workspaceId, string key, string tableName)
		{
			this.WorkspaceId = workspaceId;
			this.Key = key;
			this.TableName = tableName;
		}
	}
}
