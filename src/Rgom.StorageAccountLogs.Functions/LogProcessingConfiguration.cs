using System.Collections.Generic;

namespace Rgom.StorageAccountLogs.Functions
{
	public class LogProcessingConfiguration
	{
		public LogProcessingMode LogProcessingMode { get; set; }
		public bool ShouldAlwaysLogAnonymousRequests { get; set; }
		public HashSet<string> IgnoredOperationTypes { get; set; }
		public HashSet<string> IgnoredContainers { get; set; }
		public HashSet<string> IgnoredIps { get; set; }
		public HashSet<string> IgnoredPrincipals { get; set; }
		public HashSet<string> IgnoredApplicationIds { get; set; }
		public string LogAnalyticsWorkspaceId { get; set; }
		public string LogAnalyticsKey { get; set; }
		public string LogAnalyticsTable { get; set; }
	}
}
