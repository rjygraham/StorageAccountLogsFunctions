using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Azure.Storage.Auth;
using System;
using System.Threading.Tasks;

namespace StorageEventFunctions.Services
{
	internal class CredentialService : ICredentialService
	{
		private readonly string tenantId;
		private readonly AzureServiceTokenProvider azureServiceTokenProvider = new AzureServiceTokenProvider();

		public CredentialService(string tenantId)
		{
			this.tenantId = tenantId;
		}

		public async Task<TokenCredential> GetTokenCredentialsAsync()
		{
			try
			{
				var authResult = await azureServiceTokenProvider.GetAuthenticationResultAsync("https://storage.azure.com/", tenantId).ConfigureAwait(false);
				return new TokenCredential(authResult.AccessToken);
			}
			catch (Exception)
			{
				throw;
			}
		}

	}
}
