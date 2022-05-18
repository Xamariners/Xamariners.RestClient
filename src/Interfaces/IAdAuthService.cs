using System;
using System.Security;
using System.Threading.Tasks;
using Xamariners.RestClient.Helpers.Models;
using Xamariners.RestClient.Models;

namespace Xamariners.RestClient.Interfaces
{
    public interface IAdAuthService
    {
        Task<ServiceResponse<(AuthToken authToken, Guid UniqueId, string Username)>> Authenticate(object parent);

        Task<ServiceResponse<bool>> Unauthenticate();

        Task<AuthToken> GetTokenFromCache();

        Task<ServiceResponse<(AuthToken authToken, Guid UniqueId, string Username)>> AuthenticateUsernamePassword(string username, SecureString password);
        
    }
}
