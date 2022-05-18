// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RestClientBasicAuth.cs" company="">
//
// </copyright>
// <summary>
//   The rest client.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xamariners.RestClient.Helpers;
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
    public class RestClientBasicAuth : RestClient
    {
        private readonly IBasicAuthConfiguration _basicAuthConfiguration;

        public static RestClientBasicAuth Current { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RestClient" /> class.
        /// Initializes a new instance of the <see cref="RestClient{T}" /> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="basicAuthConfiguration">The configuration specific to basic auth.</param>
        public RestClientBasicAuth(IApiConfiguration configuration, IBasicAuthConfiguration basicAuthConfiguration) : base(configuration)
        {
            _basicAuthConfiguration = basicAuthConfiguration;
            SerializationSettings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                NullValueHandling = NullValueHandling.Ignore,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = new ReadOnlyJsonContractResolver(),
                Converters = new List<JsonConverter>
                {
                    new Iso8601TimeSpanConverter()
                }
            };

            DeserializationSettings = new JsonSerializerSettings
            {
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                NullValueHandling = NullValueHandling.Ignore,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = new ReadOnlyJsonContractResolver(),
                Converters = new List<JsonConverter>
                {
                    new Iso8601TimeSpanConverter()
                }
            };
        }

        public static RestClientBasicAuth GetInstance(string baseUrl, string basicAuthUrl, string clientId, string identityScope, string clientSecret, string identityRegistrationScope, int timeout = 30000)
        {
            var apiConfiguration = new ApiConfiguration(baseUrl, timeout, false);
            var basicAuthConfiguration = new BasicAuthConfiguration(basicAuthUrl);
            Current = new RestClientBasicAuth(apiConfiguration, basicAuthConfiguration);
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

            string tokenReponse;
            HttpResponseMessage response = null;

            IsHostReachable();

            var sw = new Stopwatch();
            sw.Start();

            //user_auth_tokens
            var tokenEndpointUrl = new Uri(_basicAuthConfiguration.BasicAuthUrl).ToString();

            using (var client = new HttpClient())
            {
                if (headers != null)
                    foreach (var header in headers)
                        client.DefaultRequestHeaders.Add(header.Key, header.Value);

                try
                {
                    var data = $"{{\"email\":\"{username}\",\"password\":\"{password}\"}}";

                    using (var requestContent = new StringContent(data, Encoding.UTF8, MIME_JSON))
                    {
                        response = await client.PostAsync(tokenEndpointUrl, requestContent).ConfigureAwait(false);
                        tokenReponse = await response.Content.ReadAsStringAsync().ConfigureAwait(false); ;
                    }
                }
                catch (Exception ex)
                {
                    sw.Stop();
                    return new ServiceResponse<AuthToken>(ServiceStatus.Error, errorMessage: ex.Message, exception: ex);
                }
            }

            switch (response?.StatusCode)
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

                    sw.Stop();

                    var errObj = JObject.Parse(tokenReponse);
                    var errorDesc = errObj["message"]?.ToString();

                    // {"message":"The username or password you have entered is invalid. Please try again.","status":404}
                    return new ServiceResponse<AuthToken>(ServiceStatus.Error,
                        errorMessage: $"Authentication Failed: {errorDesc}", errorType: ServiceErrorType.Authentication);
                default:
                    break;
            }

            var tokenObj = JObject.Parse(tokenReponse);

            //Seller auth token does not expire. setting it to one year expiry as default
            long ttl = 31536000;

            var authTokenNew = _basicAuthConfiguration.BasicAuthUrl.Contains("user_auth_tokens")
                ? AuthTokenFactory.CreateBasicAuthToken(tokenObj["token"]?.ToString(), null, Convert.ToInt64(tokenObj["ttl"]))
                : AuthTokenFactory.CreateBasicAuthToken(tokenObj["email"]?.ToString(), tokenObj["api_key"]?.ToString(), ttl);

            SetCurrentAuthToken(authTokenNew);
            SetStatusCode(response.StatusCode);

            var tokenResponse = new ServiceResponse<AuthToken>(ServiceStatus.Success, data: authTokenNew)
            {
                ElapsedTime = TimeSpan.FromMilliseconds(sw.ElapsedMilliseconds)
            };

            sw.Stop();

            return tokenResponse;
        }
    }

}
