using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Rgom.StorageAccountLogs.Functions.Services;
using System;

[assembly: FunctionsStartup(typeof(Rgom.StorageAccountLogs.Functions.Startup))]

namespace Rgom.StorageAccountLogs.Functions
{
	public class Startup : FunctionsStartup
	{
		public override void Configure(IFunctionsHostBuilder builder)
		{
			builder.Services.AddSingleton<ICredentialService, CredentialService>(sp => new CredentialService(Environment.GetEnvironmentVariable("TenantId")));
			builder.Services.AddSingleton<ILogAnalyticsConfigurationService, LogAnalyticsConfigurationService>(sp => new LogAnalyticsConfigurationService(
				Environment.GetEnvironmentVariable("DefaultLogAnalyticsWorkspaceId"),
				Environment.GetEnvironmentVariable("DefaultLogAnalyticsKey"),
				Environment.GetEnvironmentVariable("DefaultLogAnalyticsTableName")
			));
		}
	}
}
