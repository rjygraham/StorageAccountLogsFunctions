using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using StorageEventFunctions.Services;
using System;

[assembly: FunctionsStartup(typeof(StorageEventFunctions.Startup))]

namespace StorageEventFunctions
{
	public class Startup : FunctionsStartup
	{
		public override void Configure(IFunctionsHostBuilder builder)
		{
			builder.Services.AddSingleton<ICredentialService, CredentialService>(sp => new CredentialService(Environment.GetEnvironmentVariable("TenantId")));
		}
	}
}
