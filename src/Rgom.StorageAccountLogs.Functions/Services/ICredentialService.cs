using Microsoft.Azure.Storage.Auth;
using System.Threading.Tasks;

namespace StorageEventFunctions.Services
{
	public interface ICredentialService
	{
		Task<TokenCredential> GetTokenCredentialsAsync();
	}
}
