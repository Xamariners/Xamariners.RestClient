// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RestClientIDS.cs" company="">
//
// </copyright>
// <summary>
//   The rest client.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Xamariners.RestClient.Helpers.Extensions;
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
    public class RestClientMSAL : RestClient, ITokenClient
    {
        private const string AuthSuffix = "ouch";

        private readonly IIDSConfiguration _idsConfiguration;
        private readonly IRestConfiguration _restConfiguration;
        public IAdAuthService AdAuthService { get; }

        public static RestClientMSAL Current { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RestClient" /> class.
        /// Initializes a new instance of the <see cref="RestClient{T}" /> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        public RestClientMSAL(IApiConfiguration configuration, IIDSConfiguration idsConfiguration, IRestConfiguration restConfiguration, IAdAuthService adAuthService)
            : base(configuration)
        {
            _idsConfiguration = idsConfiguration;
            _restConfiguration = restConfiguration;
            AdAuthService = adAuthService;

            SerializationSettings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore,
                DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                DateFormatHandling = DateFormatHandling.IsoDateFormat
            };

            DeserializationSettings = SerializationSettings;
        }


        public static RestClientMSAL GetInstance(string baseUrl, string clientId, string identityScope, string clientSecret, string identityRegistrationScope, string tenantId, int timeout = 30000)
        {
            var apiConfiguration = new ApiConfiguration(baseUrl, timeout, false);
            var idsConfiguration = new IDSConfiguration(null, clientId, tenantId, clientSecret, null, identityScope, identityRegistrationScope);
            var restConfiguration = new RestConfiguration(apiConfiguration);
            var adAuthService = new AdAuthService(idsConfiguration);
            Current = new RestClientMSAL(apiConfiguration, idsConfiguration, restConfiguration, adAuthService);
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
            return CurrentAuthToken.AsServiceResponse();
        }

        public async Task<ServiceResponse<AuthToken>> GetClientCredentialToken(ScopeType scopeType, Dictionary<string, string> extraRequest, Dictionary<string, string> headers)
        {
            return CurrentAuthToken.AsServiceResponse();
        }

        protected override async Task<(HttpRequestMessage httpRequestMessage, HttpClient httpClient)> PrepareRequest<TResult, TBody>(HttpVerb requestVerb, string action, string id, int amount, int start, string order,
            string[] includes, Dictionary<string, object> parameters, HttpParamMode paramMode, TBody requestBody, string apiRoutePrefix,
            Dictionary<string, string> headers = null, AuthToken authToken = null, bool isAnonymous = false, Dictionary<MetaData, string> metaData = null)
        {
            if (CurrentAuthToken == null || CurrentAuthToken.ExpiresAt < DateTime.UtcNow)
            {
                var tokenResponse = await RefreshPasswordToken();
                SetCurrentAuthToken(tokenResponse.Data);
            }

            if (CurrentAuthToken.ExpiresAt < DateTime.UtcNow)
                throw new Exception(
                    $"Error Getting Auth Token with UTC expiry {CurrentAuthToken.ExpiresAt} and current {DateTime.UtcNow}");
                
            return await base.PrepareRequest<TResult, TBody>(requestVerb, action, id, amount, start, order, includes, parameters, paramMode, requestBody, apiRoutePrefix, headers, authToken, isAnonymous, metaData);
        }

        /// <summary>
        /// Refreshes the Resource Owner Authentication Token.
        /// </summary>
        /// <param name="headers">The headers.</param>
        /// <returns>
        /// The <see cref="ServiceResponse{AuthToken}" />.
        /// </returns>
        /// <exception cref="NetworkException"></exception>
        /// <exception cref="UnauthorizedAccessException">Invalid refresh token</exception>
        public async Task<ServiceResponse<AuthToken>> RefreshPasswordToken(Dictionary<string, string> headers = null)
        {
            var token = await AdAuthService.GetTokenFromCache();
            return token.AsServiceResponse();
        }
    }
}
