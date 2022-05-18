// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RestClientIDS.cs" company="">
//
// </copyright>
// <summary>
//   The rest client.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using IdentityModel.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xamariners.RestClient.Helpers.Infrastructure;
using Xamariners.RestClient.Helpers.Models;
using Xamariners.RestClient.Infrastructure;
using Xamariners.RestClient.Interfaces;
using Xamariners.RestClient.Models;

namespace Xamariners.RestClient.Services
{
    /// <summary>
    ///     The rest client.
    /// </summary>
    /// <typeparam name="T">
    /// </typeparam>
    public class RestClientIDS : RestClient, ITokenClient
    {
        private const string AuthSuffix = "iam";

        private readonly IIDSConfiguration _idsConfiguration;
        private readonly IRestConfiguration _restConfiguration;

        public static RestClientIDS Current { get; private set; }


        /// <summary>
        /// Initializes a new instance of the <see cref="RestClient" /> class.
        /// Initializes a new instance of the <see cref="RestClient{T}" /> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        public RestClientIDS(IApiConfiguration configuration, IIDSConfiguration idsConfiguration, IRestConfiguration restConfiguration)
            : base(configuration)
        {
            _idsConfiguration = idsConfiguration;
            _restConfiguration = restConfiguration;

            SerializationSettings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore,
                DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                DateFormatHandling = DateFormatHandling.IsoDateFormat
            };

            DeserializationSettings = SerializationSettings;
        }


        public static RestClientIDS GetInstance(string baseUrl, string clientId, string identityScope, string clientSecret, string identityRegistrationScope, int timeout = 30000)
        {
            var apiConfiguration = new ApiConfiguration(baseUrl, timeout, false);
            var idsConfiguration = new IDSConfiguration(null, clientId, null, clientSecret, null, identityScope, identityRegistrationScope);
            var restConfiguration = new RestConfiguration(apiConfiguration);
            Current = new RestClientIDS(apiConfiguration, idsConfiguration, restConfiguration);
            return Current;
        }

        /// <summary>
        /// Gets the Resource Owner Authentication Token.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <param name="authToken">The authentication token.</param>
        /// <param name="headers">The headers.</param>
        /// <returns>
        /// The <see cref="ServiceResponse{T}" />.
        /// </returns>
        /// <exception cref="NetworkException"></exception>
        /// <exception cref="UnauthorizedAccessException">Invalid username format</exception>
        public override async Task<ServiceResponse<AuthToken>> GetPasswordToken(
            string username, string password, AuthToken authToken, Dictionary<string, string> headers)
        {

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                throw new UnauthorizedAccessException("Invalid credential");

            TokenResponse response = null;

            string clientId = _idsConfiguration.IdentityClientId;
            string clientScope = _idsConfiguration.IdentityScope;
            string clientSecret = _idsConfiguration.IdentityClientSecret;

            if (string.IsNullOrEmpty(clientId)
                || string.IsNullOrEmpty(clientScope)
                || string.IsNullOrEmpty(clientSecret))
                throw new UnauthorizedAccessException("Invalid credential");

            //retry 3 times
            IsHostReachable();

            var sw = new Stopwatch();
            sw.Start();

            var tokenEndpoint = $"{AuthSuffix}/connect/token";

            var tokenEndpointUrl = new Uri(BaseUri, tokenEndpoint).ToString();

            using (var client = new HttpClient())
            {
                if (headers != null)
                    foreach (var header in headers)
                        client.DefaultRequestHeaders.Add(header.Key, header.Value);

                var parameters = new Parameters { { IdentityModel.OidcConstants.AuthorizeRequest.Scope, clientScope } };

                var ptr = new PasswordTokenRequest
                {
                    Address = tokenEndpointUrl,
                    ClientId = clientId,
                    GrantType = IdentityModel.OidcConstants.GrantTypes.Password,
                    ClientSecret = clientSecret,
                    UserName = username,
                    Password = password,
                    Parameters = parameters
                };

                try
                {
                    response = await client.RequestPasswordTokenAsync(ptr).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    return new ServiceResponse<AuthToken>(ServiceStatus.Error, errorMessage: ex.Message, exception: ex);
                }
                finally
                {
                    ptr.Dispose();
                    client.Dispose();
                }
            }

            switch (response?.HttpStatusCode)
            {
                case HttpStatusCode.BadGateway:
                case HttpStatusCode.BadRequest:
                case HttpStatusCode.Forbidden:
                case HttpStatusCode.GatewayTimeout:
                case HttpStatusCode.InternalServerError:
                case HttpStatusCode.NoContent:
                case HttpStatusCode.NotAcceptable:
                case HttpStatusCode.NotFound:
                case HttpStatusCode.NotImplemented:
                case HttpStatusCode.RequestTimeout:
                case HttpStatusCode.Unauthorized:

                    var error = response.TryGet(IdentityModel.OidcConstants.TokenResponse.Error);
                    var errorDescription = response.TryGet(IdentityModel.OidcConstants.TokenResponse.ErrorDescription);

                    sw.Stop();

                    if (string.IsNullOrEmpty(error) && string.IsNullOrEmpty(errorDescription))
                    {
                        return new ServiceResponse<AuthToken>(ServiceStatus.Error,
                            errorMessage: $"Authentication Failed", errorType: ServiceErrorType.Authentication);
                    }
                    else
                    {
                        return new ServiceResponse<AuthToken>(ServiceStatus.Error,
                            message: error, errorMessage: errorDescription, errorType: ServiceErrorType.Authentication);
                    }

                default:
                    break;
            }

            var issuedAt = DateTime.UtcNow;

            if (response != null)
            {
                var authTokenNew = new AuthToken
                {
                    IssuedAt = issuedAt,
                    AccessToken = response.AccessToken,
                    RefreshToken = response.RefreshToken,
                    TokenType = response.TokenType,
                    ExpiresIn = response.ExpiresIn,
                    ExpiresAt = issuedAt.AddSeconds(response.ExpiresIn),
                    ApiVersion = response.TryGet(AuthToken.ApiVersionKey),
                    MinMobileAppVersion = response.TryGet(AuthToken.MinMobileAppVersionKey)
                };

                SetCurrentAuthToken(authTokenNew);
                SetStatusCode(response.HttpStatusCode);

                var tokenResponse = new ServiceResponse<AuthToken>(ServiceStatus.Success, data: authTokenNew)
                {
                    ElapsedTime = TimeSpan.FromMilliseconds(sw.ElapsedMilliseconds)
                };

                sw.Stop();

                return tokenResponse;
            }

            return null;
        }

        private ServiceResponse<AuthToken> GetAuthTokenResponseError(TokenResponse response, ScopeType scopeType)
        {
            bool isError = false;

            switch (response?.HttpStatusCode)
            {
                case HttpStatusCode.BadGateway:
                case HttpStatusCode.BadRequest:
                case HttpStatusCode.Forbidden:
                case HttpStatusCode.GatewayTimeout:
                case HttpStatusCode.InternalServerError:
                case HttpStatusCode.NoContent:
                case HttpStatusCode.NotAcceptable:
                case HttpStatusCode.NotFound:
                case HttpStatusCode.NotImplemented:
                case HttpStatusCode.RequestTimeout:
                case HttpStatusCode.Unauthorized:
                    isError = true;
                    break;

                default:
                    if (response?.ErrorType == ResponseErrorType.Exception)
                        isError = true;
                    break;
            }

            if (!isError) return null;

            var error = response.TryGet(IdentityModel.OidcConstants.TokenResponse.Error);
            var errorDescription = response.TryGet(IdentityModel.OidcConstants.TokenResponse.ErrorDescription);

            if (string.IsNullOrEmpty(error) && string.IsNullOrEmpty(errorDescription))
            {
                return new ServiceResponse<AuthToken>(ServiceStatus.Error,
                    errorMessage: $"Failed executing for {scopeType}", errorType: ServiceErrorType.Authentication);
            }
            else
            {
                return new ServiceResponse<AuthToken>(ServiceStatus.Error,
                    error, errorDescription, errorType: ServiceErrorType.Authentication);
            }
        }

        /// <inheritdoc />
        public async Task<ServiceResponse<AuthToken>> GetClientCredentialToken(ScopeType scopeType,
            Dictionary<string, string> extraRequest, Dictionary<string, string> headers)
        {
            TokenResponse response = null;

            string clientScope = string.Empty;

            var clientId = _idsConfiguration.IdentityClientId;
            var clientSecret = _idsConfiguration.IdentityClientSecret;

            switch (scopeType)
            {
                case ScopeType.Registration:
                    clientScope = _idsConfiguration.IdentityRegistrationScope;
                    break;
            }

            if (string.IsNullOrEmpty(clientId)
                || string.IsNullOrEmpty(clientScope)
                || string.IsNullOrEmpty(clientSecret))
                throw new UnauthorizedAccessException("Invalid credential");

            //retry 3 times
            IsHostReachable();

            var sw = new Stopwatch();
            sw.Start();

            var tokenEndpoint = $"{AuthSuffix}/connect/token";

            var tokenEndpointUrl = new Uri(BaseUri, tokenEndpoint).ToString();

            using (var client = new HttpClient())
            {
                if (headers != null)
                    foreach (var header in headers)
                        client.DefaultRequestHeaders.Add(header.Key, header.Value);

                var parameters = new Parameters { { IdentityModel.OidcConstants.AuthorizeRequest.Scope, clientScope } };

                if (extraRequest != null)
                    foreach (var item in extraRequest)
                        parameters.Add(item.Key, item.Value);

                var tr = new TokenRequest
                {
                    Address = tokenEndpointUrl,
                    ClientId = clientId,
                    GrantType = IdentityModel.OidcConstants.GrantTypes.ClientCredentials,
                    ClientSecret = clientSecret,
                    Parameters = parameters
                };

                try
                {
                    response = await client.RequestTokenAsync(tr).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    return new ServiceResponse<AuthToken>(ServiceStatus.Error, errorMessage: ex.Message, exception: ex);
                }
                finally
                {
                    tr.Dispose();
                    client.Dispose();
                }
            }

            var responseError = GetAuthTokenResponseError(response, scopeType);

            sw.Stop();

            if (responseError != null)
                return responseError;

            var issuedAt = DateTime.UtcNow;

            var authTokenNew = new AuthToken
            {
                IssuedAt = issuedAt,
                AccessToken = response.AccessToken,
                RefreshToken = response.RefreshToken,
                TokenType = response.TokenType,
                ExpiresIn = response.ExpiresIn,
                ExpiresAt = issuedAt.AddSeconds(response.ExpiresIn),
                ApiVersion = response.TryGet(AuthToken.ApiVersionKey),
                MinMobileAppVersion = response.TryGet(AuthToken.MinMobileAppVersionKey)
            };

            SetStatusCode(response.HttpStatusCode);

            var tokenResponse = new ServiceResponse<AuthToken>(ServiceStatus.Success, data: authTokenNew)
            {
                ElapsedTime = TimeSpan.FromMilliseconds(sw.ElapsedMilliseconds)
            };

            return tokenResponse;
        }

        /// <summary>
        /// Refreshes the Resource Owner Authentication Token.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="refreshToken">The refresh token.</param>
        /// <param name="headers">The headers.</param>
        /// <returns>
        /// The <see cref="ServiceResponse{AuthToken}" />.
        /// </returns>
        /// <exception cref="NetworkException"></exception>
        /// <exception cref="UnauthorizedAccessException">Invalid refresh token</exception>
        public async Task<ServiceResponse<AuthToken>> RefreshPasswordToken(Dictionary<string, string> headers)
        {
            if (CurrentAuthToken is null)
                throw new UnauthorizedAccessException("Authentication failed!");

            var refreshToken = CurrentAuthToken.RefreshToken;

            if (string.IsNullOrEmpty(refreshToken))
                throw new UnauthorizedAccessException("Invalid refresh token");

            string clientId = _idsConfiguration.IdentityClientId;
            string clientScope = _idsConfiguration.IdentityScope;
            string clientSecret = _idsConfiguration.IdentityClientSecret;

            if (string.IsNullOrEmpty(clientId)
                || string.IsNullOrEmpty(clientScope)
                || string.IsNullOrEmpty(clientSecret))
                throw new UnauthorizedAccessException("Invalid credential");

            //retry 3 times
            IsHostReachable();

            var sw = new Stopwatch();
            sw.Start();

            var tokenEndpoint = $"{AuthSuffix}/connect/token";

            var tokenEndpointUrl = new Uri(BaseUri, tokenEndpoint).ToString();

            var client = new HttpClient();

            foreach (var header in headers)
                client.DefaultRequestHeaders.Add(header.Key, header.Value);

            var parameters = new Parameters { { IdentityModel.OidcConstants.AuthorizeRequest.Scope, clientScope } };

            var tr = new RefreshTokenRequest
            {
                Address = tokenEndpointUrl,
                ClientId = clientId,
                GrantType = IdentityModel.OidcConstants.GrantTypes.RefreshToken,
                ClientSecret = clientSecret,
                RefreshToken = refreshToken,
                Parameters = parameters
            };

            var response = await client.RequestRefreshTokenAsync(tr).ConfigureAwait(false);

            tr.Dispose();
            client.Dispose();

            switch (response?.HttpStatusCode)
            {
                case HttpStatusCode.BadGateway:
                case HttpStatusCode.BadRequest:
                case HttpStatusCode.Forbidden:
                case HttpStatusCode.GatewayTimeout:
                case HttpStatusCode.InternalServerError:
                case HttpStatusCode.NoContent:
                case HttpStatusCode.NotAcceptable:
                case HttpStatusCode.NotFound:
                case HttpStatusCode.NotImplemented:
                case HttpStatusCode.RequestTimeout:
                case HttpStatusCode.Unauthorized:

                    var error = response.TryGet(IdentityModel.OidcConstants.TokenResponse.Error);
                    var errorDescription = response.TryGet(IdentityModel.OidcConstants.TokenResponse.ErrorDescription);

                    sw.Stop();

                    if (string.IsNullOrEmpty(error) && string.IsNullOrEmpty(errorDescription))
                    {
                        return new ServiceResponse<AuthToken>(ServiceStatus.Error,
                            errorMessage: $"Authentication failed!", errorType: ServiceErrorType.Authentication);
                    }
                    else
                    {
                        return new ServiceResponse<AuthToken>(ServiceStatus.Error,
                            message: error, errorMessage: errorDescription, errorType: ServiceErrorType.Authentication);
                    }

                default:
                    break;
            }

            var issuedAt = DateTime.UtcNow;

            var authTokenNew = new AuthToken
            {
                IssuedAt = issuedAt,
                AccessToken = response.AccessToken,
                RefreshToken = response.RefreshToken,
                TokenType = response.TokenType,
                ExpiresIn = response.ExpiresIn,
                ExpiresAt = issuedAt.AddSeconds(response.ExpiresIn),
                ApiVersion = response.TryGet(AuthToken.ApiVersionKey),
                MinMobileAppVersion = response.TryGet(AuthToken.MinMobileAppVersionKey)
            };

            SetCurrentAuthToken(authTokenNew);
            SetStatusCode(response.HttpStatusCode);

            var tokenRefreshResponse = new ServiceResponse<AuthToken>(ServiceStatus.Success, data: authTokenNew)
            {
                ElapsedTime = TimeSpan.FromMilliseconds(sw.ElapsedMilliseconds)
            };

            sw.Stop();

            return tokenRefreshResponse;
        }
    }
}
