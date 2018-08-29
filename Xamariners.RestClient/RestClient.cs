// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RestClient.cs" company="">
//
// </copyright>
// <summary>
//   The rest client.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Plugin.Connectivity;
using Xamariners.RestClient.Helpers;
using Xamariners.RestClient.Infrastructure;
using Xamariners.RestClient.Interfaces;
using Xamariners.RestClient.Models;
using Xamariners.RestClient.Providers;

namespace Xamariners.RestClient
{
    using System.Diagnostics;
    using System.IO;

    /// <summary>
    ///     The rest client.
    /// </summary>
    /// <typeparam name="T">
    /// </typeparam>
    public class RestClient : IRestClient
    {
        #region Constants

        /// <summary>
        /// The mim e_ json.
        /// </summary>
        private const string MIME_JSON = "application/json";
        private const string MIME_BSON = "application/bson";
        private const string MIME_OCTET_STREAM = "application/octet-stream";
        private const string MIME_XFORM = "application/x-www-form-urlencoded";
        private const int MAX_NETWORK_FAILURE = 5;

        #endregion Constants

        #region Fields
        /// <summary>
        /// The credentials.
        /// </summary>
        private readonly ICredentialsProvider _credentials;

        /// <summary>
        /// The rest configuration
        /// </summary>
        private readonly IRestConfiguration _restConfiguration;

        /// <summary>
        ///     The json setting.
        /// </summary>
        private readonly JsonSerializerSettings _jsonSettings;

        private readonly HttpClient _apiClient;

        private readonly HttpClient _streamClient;

        private readonly HttpClient _authClient;

        private readonly HttpClient _miscClient;

        private readonly string _baseUrl;

        private readonly string _baseAuthUrl;

        #endregion Fields

        #region Public Properties

        /// <summary>
        ///     Gets or sets the status code.
        /// </summary>
        public int StatusCode { get; set; }

        #endregion Public Properties

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="RestClient" /> class.
        /// Initializes a new instance of the <see cref="RestClient{T}" /> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        public RestClient(IRestConfiguration configuration)
        {
            _restConfiguration = configuration;
            _credentials = configuration.CredentialsProvider;

            // TODO: later we can take this out and put it in IRestConfiguration instead
            _baseUrl = configuration.ApiConfiguration.BaseUrl;
            _baseAuthUrl = configuration.ApiConfiguration.IdentityBaseUrl;
            var timeout = configuration.TimeOut;

            // main client
            _apiClient = new HttpClient()
            {
                BaseAddress = new Uri(_baseUrl),
                Timeout = TimeSpan.FromMilliseconds(timeout)
            };
            _apiClient.DefaultRequestHeaders.Accept.Clear();
            _apiClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(MIME_JSON));

            // auth client
            _authClient = new HttpClient()
            {
                BaseAddress = new Uri(_baseAuthUrl), //StringHelpers.GetHost()
                Timeout = TimeSpan.FromMilliseconds(timeout)
            };

            _authClient.DefaultRequestHeaders.Accept.Clear();
            _authClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(MIME_XFORM));

            // stream client
            _streamClient = new HttpClient()
            {
                BaseAddress = new Uri(_baseUrl),
                Timeout = TimeSpan.FromMilliseconds(timeout)
            };
            _streamClient.DefaultRequestHeaders.Accept.Clear();
            _streamClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(MIME_OCTET_STREAM));

            // misc client 
            _miscClient = new HttpClient()
            {
                Timeout = TimeSpan.FromMilliseconds(timeout)
            };
            _miscClient.DefaultRequestHeaders.Accept.Clear();

            _jsonSettings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore,
                DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                DateFormatHandling = DateFormatHandling.IsoDateFormat
            };
        }

        #endregion Constructors and Destructors

        #region Public Methods and Operators

        /// <summary>
        /// The execute.
        /// </summary>
        /// <param name="requestVerb">
        /// The request verb.
        /// </param>
        /// <param name="action">
        /// The action.
        /// </param>
        /// <param name="requestBody">
        /// The request body.
        /// </param>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <param name="amount">
        /// The amount.
        /// </param>
        /// <param name="start">
        /// The start.
        /// </param>
        /// <param name="order">
        /// The order.
        /// </param>
        /// <param name="includes">
        /// The includes.
        /// </param>
        /// <param name="parameters">
        /// The parameters.
        /// </param>
        /// <typeparam name="T">
        /// </typeparam>
        /// <returns>
        /// The <see cref="ServiceResponse{T}"/>.
        /// </returns>
        public ServiceResponse<T> Execute<T>(
            HttpVerb requestVerb,
            [CallerMemberName] string action = null,
            T requestBody = default(T),
            string id = null,
            int amount = -1,
            int start = -1,
            string order = "",
            string[] includes = null,
            Dictionary<string, object> parameters = null,
            HttpParamMode paramMode = HttpParamMode.REST,
            string apiRoutePrefix = null,
            Dictionary<string, string> headers = null,
            bool isServiceResponse = false,
            bool isAnonymous = false,
            bool skipHostCheck = false)
        {
            return Execute<T, T>(
                requestVerb,
                action,
                requestBody,
                id,
                amount,
                start,
                order,
                includes,
                parameters,
                paramMode,
                apiRoutePrefix,
                headers,
                isServiceResponse,
                isAnonymous,
                skipHostCheck);
        }

        /// <summary>
        /// The execute.
        /// </summary>
        /// <param name="requestVerb">
        ///     The request verb.
        /// </param>
        /// <param name="action">
        ///     The action.
        /// </param>
        /// <param name="requestBody">
        ///     The request body.
        /// </param>
        /// <param name="id">
        ///     The id.
        /// </param>
        /// <param name="amount">
        ///     The amount.
        /// </param>
        /// <param name="start">
        ///     The start.
        /// </param>
        /// <param name="order">
        ///     The order.
        /// </param>
        /// <param name="includes">
        ///     The includes.
        /// </param>
        /// <param name="parameters">
        ///     The parameters.
        /// </param>
        /// <typeparam name="TResult">
        /// </typeparam>
        /// <typeparam name="TBody">
        /// </typeparam>
        /// <returns>
        /// The <see cref="ServiceResponse"/>.
        /// </returns>
        public ServiceResponse<TResult> Execute<TResult, TBody>(HttpVerb requestVerb, [CallerMemberName] string action = null,
            TBody requestBody = default(TBody), string id = null, int amount = -1, int start = -1,
            string order = "", string[] includes = null, Dictionary<string, object> parameters = null,
            HttpParamMode paramMode = HttpParamMode.REST, string apiRoutePrefix = null, Dictionary<string, string> headers = null,
            bool isServiceResponse = false,
            bool isAnonymous = false,
            bool skipHostCheck = false)
        {
            return GetServiceResponseAsync<TResult, TBody>(
                    requestVerb,
                    action,
                    requestBody,
                    id,
                    amount,
                    start,
                    order,
                    includes,
                    parameters,
                    paramMode,
                    apiRoutePrefix,
                    headers,
                    isServiceResponse,
                    isAnonymous,
                    skipHostCheck).Result;
        }

        /// <summary>
        /// The execute async.
        /// </summary>
        /// <param name="requestVerb">
        ///     The request verb.
        /// </param>
        /// <param name="action">
        ///     The action.
        /// </param>
        /// <param name="requestBody">
        ///     The request body.
        /// </param>
        /// <param name="id">
        ///     The id.
        /// </param>
        /// <param name="amount">
        ///     The amount.
        /// </param>
        /// <param name="start">
        ///     The start.
        /// </param>
        /// <param name="order">
        ///     The order.
        /// </param>
        /// <param name="includes">
        ///     The includes.
        /// </param>
        /// <param name="parameters">
        ///     The parameters.
        /// </param>
        /// <param name="apiRoutePrefix"></param>
        /// <param name="headers"></param>
        /// <typeparam name="T">
        /// </typeparam>
        /// <returns>
        /// The <see cref="async Task"/>.
        /// </returns>
        public async Task<ServiceResponse<T>> ExecuteAsync<T>(HttpVerb requestVerb, [CallerMemberName] string action = null,
            T requestBody = default(T), string id = null, int amount = -1, int start = -1, string order = "", string[] includes = null,
            Dictionary<string, object> parameters = null, HttpParamMode paramMode = HttpParamMode.REST, string apiRoutePrefix = null,
            Dictionary<string, string> headers = null, bool isServiceResponse = false,
            bool isAnonymous = false,
            bool skipHostCheck = false)
        {
            return await GetServiceResponseAsync<T, T>(
                    requestVerb,
                    action,
                    requestBody,
                    id,
                    amount,
                    start,
                    order,
                    includes,
                    parameters,
                    paramMode,
                    apiRoutePrefix,
                    headers,
                    isServiceResponse,
                    isAnonymous,
                    skipHostCheck);
        }

        /// <summary>
        /// The execute async.
        /// </summary>
        /// <param name="requestVerb">
        /// The request verb.
        /// </param>
        /// <param name="action">
        /// The action.
        /// </param>
        /// <param name="requestBody">
        /// The request body.
        /// </param>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <param name="amount">
        /// The amount.
        /// </param>
        /// <param name="start">
        /// The start.
        /// </param>
        /// <param name="order">
        /// The order.
        /// </param>
        /// <param name="includes">
        /// The includes.
        /// </param>
        /// <param name="parameters">
        /// The parameters.
        /// </param>
        /// <typeparam name="TResult">
        /// </typeparam>
        /// <typeparam name="TBody">
        /// </typeparam>
        /// <returns>
        /// The <see cref="async Task"/>.
        /// </returns>
        public async Task<ServiceResponse<TResult>> ExecuteAsync<TResult, TBody>(
            HttpVerb requestVerb,
            [CallerMemberName] string action = null,
            TBody requestBody = default(TBody),
            string id = null,
            int amount = -1,
            int start = -1,
            string order = "",
            string[] includes = null,
            Dictionary<string, object> parameters = null,
            HttpParamMode paramMode = HttpParamMode.REST,
            string apiRoutePrefix = null,
            Dictionary<string, string> headers = null,
            bool isServiceResponse = false,
            bool isAnonymous = false,
            bool skipHostCheck = false)
        {
            return await
                GetServiceResponseAsync<TResult, TBody>(
                    requestVerb,
                    action,
                    requestBody,
                    id,
                    amount,
                    start,
                    order,
                    includes,
                    parameters,
                    paramMode,
                    apiRoutePrefix,
                    headers,
                    isServiceResponse,
                    isAnonymous,
                    skipHostCheck);
        }

        #endregion Public Methods and Operators

        #region Methods

        /// <summary>
        /// The http verb 2 method.
        /// </summary>
        /// <param name="verb">
        /// The verb.
        /// </param>
        /// <returns>
        /// The <see cref="Method"/>.
        /// </returns>
        private static HttpMethod HttpVerb2Method(HttpVerb verb)
        {
            switch (verb)
            {
                case HttpVerb.DELETE:
                    return HttpMethod.Delete;

                default:
                case HttpVerb.GET:
                    return HttpMethod.Get;

                case HttpVerb.HEAD:
                    return HttpMethod.Head;

                case HttpVerb.OPTIONS:
                    return HttpMethod.Options;

                // case HttpVerb.TRACE:
                // return HttpMethod.Trace;
                case HttpVerb.POST:
                    return HttpMethod.Post;

                case HttpVerb.PUT:
                    return HttpMethod.Put;
            }
        }


        /// <summary>
        /// The get auth token.
        /// </summary>
        /// <param name="baseUrl">
        /// The base url.
        /// </param>
        /// <param name="username">
        /// The username.
        /// </param>
        /// <param name="password">
        /// The password.
        /// </param>
        /// <param name="authToken">
        /// The auth token.
        /// </param>
        /// <param name="timeoutInMilliseconds">
        /// The timeout in milliseconds.
        /// </param>
        /// <returns>
        /// The <see cref="AuthToken"/>.
        /// </returns>
        /// <exception cref="AuthenticationException">
        /// </exception>
        private async Task<ServiceResponse<AuthToken>> GetAuthToken(
            string username,
            string password,
            AuthToken authToken)
        {
            if (authToken != null && authToken.ExpiresAt.ToUniversalTime() > DateTime.UtcNow &&
                string.Equals(authToken.Username, username, StringComparison.CurrentCultureIgnoreCase))
                return authToken.AsServiceResponse();

            if (string.IsNullOrEmpty(_credentials.Username) || string.IsNullOrEmpty(_credentials.Password))
                return new ServiceResponse<AuthToken>(ServiceStatus.Error, "Credentials are not set", "Credentials are not set");

            if (authToken != null)
            {
                //Debug.WriteLine("Token expires at {0} - Now is {1}", authToken.ExpiresAt, DateTime.UtcNow);
                //Debug.WriteLine("Username {0} - authToken.Username {1}", username, authToken.Username);
                //Debug.WriteLine("Token expires at {0} - Now is {1}", authToken.ExpiresAt, DateTime.UtcNow);
            }

            //retry 3 times
            var success = await RetryHelpers.Retry(async () => await IsHostReachable(null), 1000, 3);

            if (!success)
                throw new NetworkException();

            //Debug.WriteLine("Getting Token for {0} at {1}", username, _authClient.BaseAddress);

            DateTime issuedAt = DateTime.UtcNow;

            //var contactType = await MiscHelpers.GuessContactType(username);
            //var authIdentifier = await  MiscHelpers.GetUsernameFromAuthIdentifier(username, contactType);

            var authIdentifier = username;

            if (string.IsNullOrEmpty(authIdentifier))
                throw new UnauthorizedAccessException("Invalid username format");

            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("client_id", _restConfiguration.ApiConfiguration.IdentityClientId),
                new KeyValuePair<string, string>("client_secret", _restConfiguration.ApiConfiguration.IdentityClientSecret),
                new KeyValuePair<string, string>("scope", _restConfiguration.ApiConfiguration.IdentityScope),
                new KeyValuePair<string, string>("grant_type", "password"),
                new KeyValuePair<string, string>("username", authIdentifier),
                new KeyValuePair<string, string>("password", password)
            });

            DateTime startTime = DateTime.UtcNow;
            HttpResponseMessage response = null;
            Exception exception = null;

            try
            {
                var uri =_restConfiguration.UrlRewriteProvider != null ?
                    _restConfiguration.UrlRewriteProvider.Rewrite("auth/connect/token")
                    : "auth/connect/token";

                response = await _authClient.PostAsync(uri, content);
            }
            catch (WebException wex)
            {
                // connectivity issue
                throw new NetworkException();
            }
            catch (Exception e)
            {
                exception = e;
            }

            if (response?.StatusCode == HttpStatusCode.BadRequest)
            {
                // invalid credentials
                return new ServiceResponse<AuthToken>(ServiceStatus.Error, errorType: ServiceErrorType.Authentication);
            }

            var responseInfo = new HttpResponseInfo
            {
                StartTime = startTime,
                Response = response,
                Exception = exception,
            };

            var tokenResponse = await ProcessHttpResponse<AuthToken>(responseInfo, false);

            if (tokenResponse.IsOK())
            {
                tokenResponse.Data.IssuedAt = issuedAt;
                tokenResponse.Data.ExpiresAt = issuedAt.AddSeconds(tokenResponse.Data.ExpiresIn);
                tokenResponse.Data.Username = username;
                //Debug.WriteLine("Got Token for {0} at {1}", username, _authClient.BaseAddress);
            }
            else
            {
                //Debug.WriteLine("Did not Get Token for {0} at {1}: {2}", username, _authClient.BaseAddress, tokenResponse.ErrorMessage ?? tokenResponse.Message);
            }

            return tokenResponse;
        }

        /// <summary>
        /// The get formatted error message.
        /// </summary>
        /// <param name="error">
        /// The error.
        /// </param>
        /// <param name="requestUri">
        /// The request uri.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        private string GetFormattedErrorMessage(string error, string requestUri, object value)
        {
            // do not process stream
            if (value is StreamContent)
                value = "";

            return $"Error '{error}' getting JSON with status {this.StatusCode} for resource '{requestUri}' : {value}";
        }

        /// <summary>
        /// The get service response async.
        /// </summary>
        /// <param name="requestVerb">
        /// The request verb.
        /// </param>
        /// <param name="action">
        /// The action.
        /// </param>
        /// <param name="requestBody">
        /// The request body.
        /// </param>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <param name="amount">
        /// The amount.
        /// </param>
        /// <param name="start">
        /// The start.
        /// </param>
        /// <param name="order">
        /// The order.
        /// </param>
        /// <param name="includes">
        /// The includes.
        /// </param>
        /// <param name="parameters">
        /// The parameters.
        /// </param>
        /// <typeparam name="TResult">
        /// </typeparam>
        /// <typeparam name="TBody">
        /// </typeparam>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        private async Task<ServiceResponse<TResult>> GetServiceResponseAsync<TResult, TBody>(
            HttpVerb requestVerb,
            string action,
            TBody requestBody,
            string id = null,
            int amount = -1,
            int start = -1,
            string order = "",
            string[] includes = null,
            Dictionary<string, object> parameters = null,
            HttpParamMode paramMode = HttpParamMode.REST,
            string apiRoutePrefix = null,
            Dictionary<string, string> headers = null,
            bool isServiceResponse = false,
            bool isAnonymous = false,
            bool skipHostCheck = false)
        {
            if (!skipHostCheck)
            {
                //retry 3 times
                var success = await RetryHelpers.Retry(async () => await IsHostReachable(apiRoutePrefix), 1000, 3);

                if (!success)
                {
                    var result = CrossConnectivity.Current.IsConnected;
                    if (!result)
                    {
                        throw new NetworkException("Network not available");
                    }
                    
                    throw new NetworkException("Server not responding");
                }
                    
            }

            var rc = await PrepareRequest<TResult, TBody>(
            requestVerb,
            action,
            id,
            amount,
            start,
            order,
            includes,
            parameters,
            paramMode,
            requestBody,
            apiRoutePrefix,
            headers,
            isAnonymous);

            if (!rc.Item3.IsOK())
            {
                var result = rc.Item3.ToServiceResponse<AuthToken, TResult>();
                return result;
            }

            return await SendRequest<TResult>(rc.Item2, rc.Item1, isServiceResponse);
        }

        private async Task<bool> IsHostReachable(string apiRoutePrefix)
        {
            //return true;
            string host;

            if (!string.IsNullOrEmpty(apiRoutePrefix) && apiRoutePrefix.ToLower().Contains("http"))
                host = new Uri(apiRoutePrefix).Host;
            else
                host = new Uri(_baseUrl).Host;

          
            var result = await CrossConnectivity.Current.IsRemoteReachable(host, 80, 2000);

            return result;
        }

        /// <summary>
        /// The prepare request.
        /// </summary>
        /// <param name="requestVerb">
        ///     The request Verb.
        /// </param>
        /// <param name="action">
        ///     The action.
        /// </param>
        /// <param name="id">
        ///     The id.
        /// </param>
        /// <param name="amount">
        ///     The amount.
        /// </param>
        /// <param name="start">
        ///     The start.
        /// </param>
        /// <param name="order">
        ///     The order.
        /// </param>
        /// <param name="includes">
        /// </param>
        /// <param name="parameters">
        ///     The parameters.
        /// </param>
        /// <param name="requestBody">
        ///     The request Body.
        /// </param>
        /// <param name="apiRoutePrefix"></param>
        /// <param name="headers"></param>
        /// <param name="timeoutInMilliseconds">
        /// The timeout In Milliseconds.
        /// </param>
        /// <param name="client">
        /// The client.
        /// </param>
        /// <returns>
        /// The <see cref="RestRequest"/>.
        /// </returns>
        private async Task<Tuple<HttpRequestMessage, HttpClient, ServiceResponse<AuthToken>>> PrepareRequest<TResult, TBody>(HttpVerb requestVerb,
            string action, string id, int amount, int start, string order, string[] includes, Dictionary<string, object> parameters,
            HttpParamMode paramMode, TBody requestBody, string apiRoutePrefix, Dictionary<string, string> headers = null, bool isAnonymous = false)
        {
            HttpClient client = null;

            var resource = new StringBuilder();
            Type bodyType = typeof(TBody);
            Type resultType = typeof(TResult);

            bool isMiscClient = false;
            if (!string.IsNullOrEmpty(apiRoutePrefix))
            {
                if (apiRoutePrefix.ToLower().Contains("http"))
                {
                    client = _miscClient;
                    client.BaseAddress = new Uri(apiRoutePrefix);
                    isMiscClient = true;
                }
                else
                {
                    client = resultType != typeof(Stream) ? _apiClient : this._streamClient;
                    if (!string.IsNullOrEmpty(apiRoutePrefix))
                        resource.Append(apiRoutePrefix);
                }
            }
            else
            {
                client = resultType != typeof(Stream) ? _apiClient : this._streamClient;

                if (bodyType.GenericTypeArguments != null && bodyType.GenericTypeArguments.Length == 1)
                    resource.Append(bodyType.GenericTypeArguments[0].Name);
                else
                    resource.Append(bodyType.Name);
            }

            // construct uri
            if (!string.IsNullOrEmpty(action))
            {
                resource.Append("/" + action);
            }

            if (!string.IsNullOrEmpty(id))
            {
                resource.Append("/" + id);
            }

            if (parameters != null && parameters.Any())
            {
                string p = null;
                switch (paramMode)
                {
                    case HttpParamMode.QUERYSTRING:
                        p = $"?{string.Join("&", parameters.Select(kvp => $"{kvp.Key}={WebUtility.UrlEncode(kvp.Value.ToString()).Replace("+", " ")}"))}";
                        break;

                    case HttpParamMode.REST:
                        p = $"/{string.Join("/", parameters.Select(kvp => $"{kvp.Key}/{WebUtility.UrlEncode(kvp.Value.ToString()).Replace("+", " ")}"))}";
                        break;

                    case HttpParamMode.RESTVALUE:
                        p = $"/{string.Join("/", parameters.Select(kvp => $"{WebUtility.UrlEncode(kvp.Value.ToString()).Replace("+", " ")}"))}";
                        break;
                }

                if (p != null)
                    resource.Append(p);
            }

            if (amount != -1)
            {
                resource.Append("/amount/" + WebUtility.UrlEncode(amount.ToString()));
            }

            if (start != -1)
            {
                resource.Append("/start/" + start);
            }

            if (!string.IsNullOrEmpty(order))
            {
                resource.Append("/order/" + order);
            }

            if (includes != null)
            {
                resource.Append("?includes=" + String.Join(",", includes));
            }

            var tokenResponse = new ServiceResponse<AuthToken>();

            if (!isMiscClient && !isAnonymous)
            {
                try
                {
                    tokenResponse = await GetAuthToken(
                        _credentials.Username,
                        _credentials.Password,
                        _credentials.AuthToken);

                    if (tokenResponse.IsOK())
                    {
                        _credentials.AuthToken = tokenResponse.GetData();
                        _credentials.Password = string.Empty;
                    }
                    else
                        return new Tuple<HttpRequestMessage, HttpClient, ServiceResponse<AuthToken>>(null, null,
                            tokenResponse);
                }
                catch (Exception ex)
                {
                    if (ex.InnerException is WebException)
                        throw new NetworkException();

                    if (ex is UnauthorizedAccessException || ex is NetworkException)
                        throw;
                }
            }

            // clean resource if we already have a base url with trailing '/'
            if (client.BaseAddress.AbsolutePath.EndsWith("/") && resource.ToString().StartsWith("/"))
                resource.Remove(0, 1);

            var requestUrl = new Uri(client.BaseAddress, resource.ToString());
            var request = new HttpRequestMessage(HttpVerb2Method(requestVerb), requestUrl);

            // clear and add request headers
            request.Headers.Clear();
            try
            {
                if (headers != null)
                {
                    foreach (var header in headers)
                    {
                        request.Headers.Remove(header.Key);
                        request.Headers.Add(header.Key, header.Value);
                    }

                    if (paramMode == HttpParamMode.XFORM)
                    {
                        if (!request.Headers.Accept.Contains(new MediaTypeWithQualityHeaderValue(MIME_XFORM)))
                            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(MIME_XFORM));
                    }
                    else
                    {
                        if (!request.Headers.Accept.Contains(new MediaTypeWithQualityHeaderValue(MIME_JSON)))
                            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(MIME_JSON));
                    }
                }
                else
                {
                    request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(MIME_JSON));
                }
            }
            catch (Exception ex)
            {
                ; // ignore
            }

            if (!isMiscClient && !isAnonymous)
                request.Headers.Authorization = new AuthenticationHeaderValue(
                _credentials?.AuthToken?.TokenType,
                _credentials?.AuthToken?.AccessToken);

            // Add Body
            if (parameters != null && parameters.Any() && paramMode == HttpParamMode.BODY)
            {
                // from params - body
                string data = JsonConvert.SerializeObject(parameters, _jsonSettings);
                request.Content = new StringContent(data, Encoding.UTF8, MIME_JSON);
            }
            if (parameters != null && parameters.Any() && paramMode == HttpParamMode.XFORM)
            {
                // from params - xform
                var data = parameters.ToDictionary(p => p.Key, p => p.Value?.ToString());
                request.Content = new FormUrlEncodedContent(data);
            }
            else if ((typeof(TBody).GetTypeInfo().IsValueType && !Equals(requestBody, default(TBody)))
                || (!typeof(TBody).GetTypeInfo().IsValueType && requestBody != null))
            {
                // from body object
                string data = JsonConvert.SerializeObject(requestBody, _jsonSettings);
                request.Content = new StringContent(data, Encoding.UTF8, MIME_JSON);
            }

            request = _restConfiguration.UrlRewriteProvider != null 
                ? _restConfiguration.UrlRewriteProvider.Rewrite(request)
                : request;

            return new Tuple<HttpRequestMessage, HttpClient, ServiceResponse<AuthToken>>(request, client, tokenResponse);
        }

        /// <summary>
        /// The process http response.
        /// </summary>
        /// <param name="task">
        /// The task.
        /// </param>
        /// <param name="isServiceResponse"></param>
        /// <typeparam name="TResult">
        /// </typeparam>
        /// <returns>
        /// The <see cref="ServiceResponse"/>.
        /// </returns>
        private async Task<ServiceResponse<TResult>> ProcessHttpResponse<TResult>(HttpResponseInfo info, bool isServiceResponse)
        {
            ServiceResponse<TResult> data;
            TimeSpan elapsedTime = DateTime.UtcNow - info.StartTime;

            Debug.WriteLine("[{0}ms] rest roundtrip", elapsedTime.TotalMilliseconds);
          
            HttpResponseMessage response = info.Response;

            if (info.Exception != null && response != null)
            {
                Debug.WriteLine(info.Exception.GetInnermostExceptionMessage());
                StatusCode = (int)response.StatusCode;
                string message = GetFormattedErrorMessage(
                    response.ReasonPhrase,
                    response.RequestMessage.RequestUri.ToString(),
                    response.Content);
                return new ServiceResponse<TResult>(ServiceStatus.Error, "Error processing request", message) { ElapsedTime = elapsedTime };
            }

            if (info.Exception != null)
            {
                string message = info.Exception.GetInnermostExceptionMessage();
                return new ServiceResponse<TResult>(ServiceStatus.Error, "Error processing request", message) { ElapsedTime = elapsedTime };
            }

            if (response == null)
            {
                string message = "No response received from server";
                return new ServiceResponse<TResult>(ServiceStatus.Error, message) { ElapsedTime = elapsedTime };
            }

            if (response.RequestMessage.Method == HttpMethod.Get && response.StatusCode != HttpStatusCode.OK
                || new[] { HttpMethod.Put, HttpMethod.Post }.Contains(response.RequestMessage.Method)
                && new[] { HttpStatusCode.OK, HttpStatusCode.NoContent }.Contains(response.StatusCode) == false)
            {
                StatusCode = (int)response.StatusCode;
                string message = GetFormattedErrorMessage(
                    response?.ReasonPhrase,
                    response?.RequestMessage?.RequestUri.ToString(),
                    response?.Content);

                return new ServiceResponse<TResult>(ServiceStatus.Error, "Bad response received from server", message);
            }

            HttpContent content = response.Content;

            string json = string.Empty;

            try
            {
                if (typeof(TResult) != typeof(Stream))
                {
                    json = await content.ReadAsStringAsync();

                    var errorContentCheck = CheckForErrorContent<TResult>(json);
                    if (errorContentCheck != null)
                        return errorContentCheck;

                    if (!isServiceResponse)
                    {
                        try
                        {
                            data = JsonConvert.DeserializeObject<TResult>(json, _jsonSettings).AsServiceResponse();
                        }
                        catch
                        {
                            if (typeof(TResult) == typeof(string))
                                data = new ServiceResponse<TResult>(ServiceStatus.Success, stringData: json);
                            else
                                throw;
                        }
                    }
                    else
                        data = JsonConvert.DeserializeObject<ServiceResponse<TResult>>(json, _jsonSettings);
                }
                else
                {
                    var stream = await content.ReadAsStreamAsync();
                    data = new ServiceResponse<TResult>(true) { Stream = stream };
                }

                data.ElapsedTime = elapsedTime;
                data.RequestDateTime = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                string message = GetFormattedErrorMessage(
                    ex.GetInnermostExceptionMessage(),
                    response.RequestMessage.RequestUri.ToString(),
                    json);

                return new ServiceResponse<TResult>(ServiceStatus.Error, "Error deserializing response data", message);
            }

            StatusCode = (int)response.StatusCode;
            return data;
        }

        private ServiceResponse<TResult> CheckForErrorContent<TResult>(string json)
        {
            var errors = new[] { "Bad Request" };
            return errors.Any(json.Contains) ? new ServiceResponse<TResult>(ServiceStatus.Error, "Bad response received from server", json) : null;
        }

        /// <summary>
        /// The send request.
        /// </summary>
        /// <param name="client">
        /// The client.
        /// </param>
        /// <param name="request">
        /// The request.
        /// </param>
        /// <typeparam name="TResult">
        /// </typeparam>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        private async Task<ServiceResponse<TResult>> SendRequest<TResult>(HttpClient client, HttpRequestMessage request, bool isServiceResponse)
        {
            Debug.WriteLine($"RestClient: Sending request '{client.BaseAddress} - {request.RequestUri}'");

            DateTime startTime = DateTime.UtcNow;
            HttpResponseMessage response = null;
            Exception exception = null;

            try
            {
#if DEBUG
                response = client.SendAsync(request, HttpCompletionOption.ResponseContentRead).Result;
#else
                response = await client.SendAsync(request, HttpCompletionOption.ResponseContentRead);
#endif

                Debug.WriteLine($"RestClient: Success");

            }
            catch (WebException wex)
            {
                // connectivity issue
                throw new NetworkException();
            }
            catch (Exception e)
            {
                Debug.WriteLine("RestClient error:" + e.Message);
                exception = e;
            }

            var responseInfo = new HttpResponseInfo
            {
                StartTime = startTime,
                Response = response,
                Exception = exception,
                Request = request,
            };

#if DEBUG
            return ProcessHttpResponse<TResult>(responseInfo, isServiceResponse).Result;
#else
            return await ProcessHttpResponse<TResult>(responseInfo, isServiceResponse);
# endif
        }

        //// TODO: this not pcl
        ///// <summary>
        ///// The validator.
        ///// </summary>
        ///// <param name="sender">
        ///// The sender.
        ///// </param>
        ///// <param name="certificate">
        ///// The certificate.
        ///// </param>
        ///// <param name="chain">
        ///// The chain.
        ///// </param>
        ///// <param name="sslPolicyErrors">
        ///// The ssl policy errors.
        ///// </param>
        ///// <returns>
        ///// The <see cref="bool"/>.
        ///// </returns>
        //private static bool Validator(
        //    object sender,
        //    X509Certificate certificate,
        //    X509Chain chain,
        //    SslPolicyErrors sslPolicyErrors)
        //{
        //    return true;
        //}

        #endregion Methods

    }


    /// <summary>
    /// The http response info.
    /// </summary>
    internal struct HttpResponseInfo
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the exception.
        /// </summary>
        public Exception Exception { get; set; }

        /// <summary>
        /// Gets or sets the request.
        /// </summary>
        public HttpRequestMessage Request { get; set; }

        /// <summary>
        /// Gets or sets the response.
        /// </summary>
        public HttpResponseMessage Response { get; set; }

        /// <summary>
        /// Gets or sets the start time.
        /// </summary>
        public DateTime StartTime { get; set; }

        #endregion Public Properties
    }
}