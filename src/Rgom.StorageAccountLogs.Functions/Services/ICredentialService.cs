using Microsoft.Azure.Storage.Auth;
using System.Threading.Tasks;

namespace Rgom.StorageAccountLogs.Functions.Services
{
	public interface ICredentialService
	{
		Task<TokenCredential> GetTokenCredentialsAsync();
	}
}
