using System;
using System.Collections.Generic;
using System.Text;

namespace Rgom.StorageAccountLogs.Functions.Services
{
	public interface ILogAnalyticsConfigurationService
	{
		string WorkspaceId { get; }
		string Key { get; }
		string TableName { get; }
	}
}
