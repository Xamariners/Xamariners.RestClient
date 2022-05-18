using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Security.Authentication;
using System.Threading.Tasks;
using Microsoft.Identity.Client;
using Xamariners.RestClient.Helpers.Extensions;
using Xamariners.RestClient.Helpers.Models;
using Xamariners.RestClient.Interfaces;

namespace Xamariners.RestClient.Services
{
    public class AdAuthService : IAdAuthService
    {
        private IPublicClientApplication _pca = null;

        private readonly string[] _scopes = { "User.Read" };

        private IAccount _account;

        public AdAuthService(IIDSConfiguration idsConfiguration)
        {
            _pca = PublicClientApplicationBuilder.CreateWithApplicationOptions(
                    new PublicClientApplicationOptions()
                    {
                        ClientId = idsConfiguration.IdentityClientId,
                        TenantId = idsConfiguration.IdentityTenantId,
                        RedirectUri = $"msal{idsConfiguration.IdentityClientId}://auth"
                    })
                .Build();

            if (!string.IsNullOrEmpty(idsConfiguration.IdentityScope))
            {
                _scopes = idsConfiguration.IdentityScope.Split(" ");
            }
        }

        public async Task<ServiceResponse<(AuthToken authToken, Guid UniqueId, string Username)>> Authenticate(object parent)
        {
            AuthenticationResult authResult = null;
            IEnumerable<IAccount> accounts = await _pca.GetAccountsAsync().ConfigureAwait(false);

            try
            {
                _account = accounts.FirstOrDefault();
                authResult = await _pca.AcquireTokenSilent(_scopes, _account)
                    .ExecuteAsync().ConfigureAwait(false);
            }
            catch (MsalUiRequiredException msalUiRequiredException)
            {
                try
                {
                    // new user or logged out user // needs re login
                    authResult = await _pca.AcquireTokenInteractive(_scopes)
                        .WithUseEmbeddedWebView(true)
                        .WithParentActivityOrWindow(parent)
                        .ExecuteAsync().ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    // ignored
                }
            }

            if (authResult == null)
                throw new AuthenticationException();

            var issuedAt = DateTime.UtcNow;

            var authToken = new AuthToken
            {
                IssuedAt = issuedAt,
                AccessToken = authResult.AccessToken,
                RefreshToken = "",
                TokenType = "Bearer",
                ExpiresAt = authResult.ExpiresOn.UtcDateTime,
                ApiVersion = "",
                MinMobileAppVersion = ""
            };

            return (authToken, new Guid(authResult.UniqueId), authResult.Account.Username).AsServiceResponse();
        }

        public async Task<ServiceResponse<(AuthToken authToken, Guid UniqueId, string Username)>> AuthenticateUsernamePassword(string username, SecureString password)
        {
            AuthenticationResult authResult = null;
            IEnumerable<IAccount> accounts = await _pca.GetAccountsAsync().ConfigureAwait(false);

            try
            {
                _account = accounts.FirstOrDefault();
                authResult = await _pca.AcquireTokenSilent(_scopes, _account)
                    .ExecuteAsync().ConfigureAwait(false);
            }
            catch (MsalUiRequiredException msalUiRequiredException)
            {
                try
                {
                    // new user or logged out user // needs re login
                    authResult = await _pca.AcquireTokenByUsernamePassword(_scopes, username, password)
                        .ExecuteAsync().ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    // ignored
                }
            }

            if (authResult == null)
                throw new AuthenticationException();

            var issuedAt = DateTime.UtcNow;

            var authToken = new AuthToken
            {
                IssuedAt = issuedAt,
                AccessToken = authResult.AccessToken,
                RefreshToken = "",
                TokenType = "Bearer",
                ExpiresAt = authResult.ExpiresOn.UtcDateTime,
                ApiVersion = "",
                MinMobileAppVersion = ""
            };

            return (authToken, new Guid(authResult.UniqueId), authResult.Account.Username).AsServiceResponse();
        }

        public async Task<AuthToken> GetTokenFromCache()
        {
            var accounts = await _pca.GetAccountsAsync().ConfigureAwait(false);

            // check if we got an account here
            if (accounts == null || !accounts.Any())
                throw new AuthenticationException("No account found");

            _account = accounts.FirstOrDefault();

            var token = await _pca.AcquireTokenSilent(_scopes, _account).ExecuteAsync().ConfigureAwait(false);

            var issuedAt = DateTime.UtcNow;

            var authToken = new AuthToken
            {
                IssuedAt = issuedAt,
                AccessToken = token.AccessToken,
                RefreshToken = "",
                TokenType = "Bearer",
                ExpiresAt = token.ExpiresOn.UtcDateTime,
                ApiVersion = "",
                MinMobileAppVersion = ""
            };

            return authToken;
        }

        public async Task<ServiceResponse<bool>> Unauthenticate()
        {
            var accounts = await _pca.GetAccountsAsync().ConfigureAwait(false);

            if (accounts != null)
            {
                while (accounts.Any())
                {
                    await _pca.RemoveAsync(accounts.FirstOrDefault()).ConfigureAwait(false);
                    accounts = await _pca.GetAccountsAsync().ConfigureAwait(false);
                }

                return true.AsServiceResponse();
            }
            else
            {
                return false.AsServiceResponse();
            }
        }
    }
}
