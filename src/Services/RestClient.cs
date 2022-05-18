// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RestClient.cs" company="">
//
// </copyright>
// <summary>
//   The rest client.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamariners.RestClient.Helpers;
using Xamariners.RestClient.Helpers.Extensions;
using Xamariners.RestClient.Helpers.Infrastructure;
using Xamariners.RestClient.Helpers.Models;
using Xamariners.RestClient.Infrastructure;
using Xamariners.RestClient.Interfaces;

namespace Xamariners.RestClient.Services
{
    /// <summary>
    ///     The rest client.
    /// </summary>
    /// <typeparam name="T">
    /// </typeparam>
    public abstract class RestClient : IRestClient
    {
        #region Constants

        /// <summary>
        /// The MIME JSON.
        /// </summary>
        internal const string MIME_JSON = "application/json";
        internal const string MIME_BSON = "application/bson";
        internal const string MIME_OCTET_STREAM = "application/octet-stream";
        internal const string MIME_XFORM = "application/x-www-form-urlencoded";
        internal const int MAX_NETWORK_FAILURE = 5;
        #endregion Constants

        #region Fields
        protected HttpClient _restClient;

        #endregion Fields

        #region Public Properties

        /// <summary>
        /// Gets the configuration.
        /// </summary>
        public IApiConfiguration Configuration { get; }

        /// <summary>
        /// The base URI of the service.
        /// </summary>
        public Uri BaseUri { get; }

        /// <summary>
        /// Gets the API timeout.
        /// </summary>
        public double ApiTimeout { get; }

        /// <summary>
        /// Gets or sets json serialization settings.
        /// </summary>
        public JsonSerializerSettings SerializationSettings { get; internal set; }

        /// <summary>
        /// Gets or sets json deserialization settings.
        /// </summary>
        public JsonSerializerSettings DeserializationSettings { get; internal set; }

        /// <summary>
        /// Gets or sets the status code.
        /// </summary>
        public int StatusCode { get; private set; }

        /// <summary>
        /// Gets the current AuthToken value
        /// </summary>
        public AuthToken CurrentAuthToken { get; private set; }

        #endregion Public Properties

        #region Constructors and Destructor

        /// <summary>
        /// Initializes a new instance of the <see cref="RestClient" /> class.
        /// Initializes a new instance of the <see cref="RestClient{T}" /> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        protected RestClient(IApiConfiguration configuration)
        {
            //Configuration = configuration;

            if (configuration == null) return;

            BaseUri = new Uri(configuration?.BaseUrl);
            ApiTimeout = configuration.ApiTimeout;
        }

        #endregion Constructors and Destructor

        #region Public Methods and Operators

        private HttpClient GetRestClient(string mime = null)
        {
            var client = new HttpClient()
            {
                BaseAddress = mime != null ? BaseUri : null,
                Timeout = TimeSpan.FromMilliseconds(ApiTimeout)
            };

            client.DefaultRequestHeaders.Accept.Clear();

            if(mime != null)
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(mime)); // MIME_JSON || MIME_OCTET_STREAM

            return client;
        }

        /// <summary>
        /// The execute async.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="requestVerb">The request verb.</param>
        /// <param name="action">The action.</param>
        /// <param name="requestBody">The request body.</param>
        /// <param name="id">The id.</param>
        /// <param name="amount">The amount.</param>
        /// <param name="start">The start.</param>
        /// <param name="order">The order.</param>
        /// <param name="includes">The includes.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="paramMode"></param>
        /// <param name="apiRoutePrefix"></param>
        /// <param name="headers"></param>
        /// <param name="isServiceResponse"></param>
        /// <param name="isAnonymous"></param>
        /// <param name="skipHostCheck"></param>
        /// <returns>
        /// The <see cref="async Task" />.
        /// </returns>
        public async Task<ServiceResponse<T>> ExecuteAsync<T>(HttpVerb requestVerb, [CallerMemberName] string action = null,
            T requestBody = default(T), string id = null, int amount = -1, int start = -1, string order = "", string[] includes = null,
            Dictionary<string, object> parameters = null, HttpParamMode paramMode = HttpParamMode.REST, string apiRoutePrefix = null,
            Dictionary<string, string> headers = null, AuthToken authToken = null, bool isServiceResponse = false,
            bool isAnonymous = false, bool skipHostCheck = false, Dictionary<MetaData, string> metaData = null)
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
                authToken,
                isServiceResponse,
                isAnonymous,
                skipHostCheck,
                metaData).ConfigureAwait(false);
        }

        /// <summary>
        /// The execute async.
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <typeparam name="TBody"></typeparam>
        /// <param name="requestVerb">The request verb.</param>
        /// <param name="action">The action.</param>
        /// <param name="requestBody">The request body.</param>
        /// <param name="id">The id.</param>
        /// <param name="amount">The amount.</param>
        /// <param name="start">The start.</param>
        /// <param name="order">The order.</param>
        /// <param name="includes">The includes.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="paramMode"></param>
        /// <param name="apiRoutePrefix"></param>
        /// <param name="headers"></param>
        /// <param name="isServiceResponse"></param>
        /// <param name="isAnonymous"></param>
        /// <param name="skipHostCheck"></param>
        /// <returns>
        /// The <see cref="async Task" />.
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
            AuthToken authToken = null,
            bool isServiceResponse = false,
            bool isAnonymous = false,
            bool skipHostCheck = false,
            Dictionary<MetaData, string> metaData = null)
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
                    authToken,
                    isServiceResponse,
                    isAnonymous,
                    skipHostCheck,
                    metaData).ConfigureAwait(false);
        }

        #endregion Public Methods and Operators

        #region Methods

        /// <summary>
        /// The http verb 2 method.
        /// </summary>
        /// <param name="verb">The verb.</param>
        /// <returns>
        /// The <see cref="HttpMethod" />.
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
        /// Gets the Resource Owner Authentication Token.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <param name="authToken">The authentication token.</param>
        /// <param name="headers">The headers.</param>
        /// <returns>
        /// The <see cref="ServiceResponse{AuthToken}" />.
        /// </returns>
        /// <exception cref="NetworkException"></exception>
        /// <exception cref="UnauthorizedAccessException">Invalid username format</exception>
        public abstract Task<ServiceResponse<AuthToken>> GetPasswordToken(
            string username,
            string password,
            AuthToken authToken,
            Dictionary<string, string> headers);

        /// <summary>
        /// The get formatted error message.
        /// </summary>
        /// <param name="error">The error.</param>
        /// <param name="requestUri">The request uri.</param>
        /// <param name="value">The value.</param>
        private string GetFormattedErrorMessage(string error, string requestUri, object value)
        {
            // do not process stream
            if (value is StreamContent)
                value = "";

            return $"Error '{error}' getting JSON with status {StatusCode} for resource '{requestUri}' : {value}";
        }

        /// <summary>
        /// The get service response async.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <typeparam name="TBody">The type of the body.</typeparam>
        /// <param name="requestVerb">The request verb.</param>
        /// <param name="action">The action.</param>
        /// <param name="requestBody">The request body.</param>
        /// <param name="id">The id.</param>
        /// <param name="amount">The amount.</param>
        /// <param name="start">The start.</param>
        /// <param name="order">The order.</param>
        /// <param name="includes">The includes.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="paramMode">The parameter mode.</param>
        /// <param name="apiRoutePrefix">The API route prefix.</param>
        /// <param name="headers">The headers.</param>
        /// <param name="isServiceResponse">if set to <c>true</c> [is service response].</param>
        /// <param name="isAnonymous">if set to <c>true</c> [is anonymous].</param>
        /// <param name="skipHostCheck">if set to <c>true</c> [skip host check].</param>
        /// <returns>
        /// The <see cref="ServiceResponse{TResult}" />.
        /// </returns>
        /// <exception cref="NetworkException">Network not available or Server not responding</exception>
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
            AuthToken authToken = null,
            bool isServiceResponse = false,
            bool isAnonymous = false,
            bool skipHostCheck = false,
            Dictionary<MetaData, string> metaData = null)
        {
            if (!skipHostCheck)
                IsHostReachable();

            var (httpRequestMessage, httpClient) = await PrepareRequest<TResult, TBody>(
                requestVerb, action, id, amount, start, order, includes, parameters, paramMode, requestBody, apiRoutePrefix, headers, authToken, isAnonymous, metaData);

            return await SendRequest<TResult>(httpClient, httpRequestMessage, isServiceResponse);
        }

        protected bool IsHostReachable()
        {

            try
            {
                var current = Connectivity.NetworkAccess;


                if (current == NetworkAccess.None)
                    throw new NetworkException("Network not available");

                return true;
            }
            catch (Xamarin.Essentials.NotImplementedInReferenceAssemblyException ex)
            {
                return true;
            }
        }

        /// <summary>
        /// The prepare request.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <typeparam name="TBody">The type of the body.</typeparam>
        /// <param name="requestVerb">The request Verb.</param>
        /// <param name="action">The action.</param>
        /// <param name="id">The id.</param>
        /// <param name="amount">The amount.</param>
        /// <param name="start">The start.</param>
        /// <param name="order">The order.</param>
        /// <param name="includes">The includes.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="paramMode">The parameter mode.</param>
        /// <param name="requestBody">The request Body.</param>
        /// <param name="apiRoutePrefix">The API route prefix.</param>
        /// <param name="headers">The headers.</param>
        /// <param name="authToken"></param>
        /// <param name="isAnonymous">if set to <c>true</c> [is anonymous].</param>
        /// <param name="metaData"></param>
        /// <returns>
        /// The <see cref="ServiceResponse{TResult}" />.
        /// </returns>
        /// <exception cref="NetworkException"></exception>
        protected async virtual Task<(HttpRequestMessage httpRequestMessage, HttpClient httpClient)>
            PrepareRequest<TResult, TBody>(HttpVerb requestVerb, string action, string id, int amount, int start,
                string order, string[] includes,
                Dictionary<string, object> parameters, HttpParamMode paramMode, TBody requestBody,
                string apiRoutePrefix,
                Dictionary<string, string> headers = null, AuthToken authToken = null, bool isAnonymous = false,
                Dictionary<MetaData, string> metaData = null)
        {
            // This is to fix a bug when uploading a photo (self reg) paramMode would reset to the default value (Rest instead of MultiPart)
            // Couldn't work it out how this was happening (iOS only, and device only, not Simulator) so put in this as a workaround
            if (metaData != null && metaData.ContainsKey(MetaData.ParamMode))
                Enum.TryParse(metaData[MetaData.ParamMode], out paramMode);

            var resource = new StringBuilder();
            Type bodyType = typeof(TBody);
            Type resultType = typeof(TResult);

            bool isMiscClient = false;
            if (!string.IsNullOrEmpty(apiRoutePrefix))
            {
                if (apiRoutePrefix.ToLower().Contains("http"))
                {
                    _restClient = GetRestClient(); // Misc Client
                    _restClient.BaseAddress = new Uri(apiRoutePrefix);
                    isMiscClient = true;
                }
                else
                {
                    _restClient = resultType != typeof(Stream) ? GetRestClient(MIME_JSON) : GetRestClient(MIME_OCTET_STREAM); // Media Client
                    if (!string.IsNullOrEmpty(apiRoutePrefix))
                        resource.Append(apiRoutePrefix);
                }
            }
            else
            {
                _restClient = resultType != typeof(Stream) ? GetRestClient(MIME_JSON) : GetRestClient(MIME_OCTET_STREAM); // API Client || Media Client

                if (bodyType.GenericTypeArguments != null && bodyType.GenericTypeArguments.Length == 1)
                    resource.Append(bodyType.GenericTypeArguments[0].Name);
                else
                    resource.Append(bodyType.Name);
            }

            // construct uri
            if (!string.IsNullOrEmpty(action)) resource.Append("/" + action);
            if (!string.IsNullOrEmpty(id)) resource.Append("/" + id);

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

                if (p != null) resource.Append(p);
            }

            if (amount != -1) resource.Append("/amount/" + WebUtility.UrlEncode(amount.ToString()));
            if (start != -1) resource.Append("/start/" + start);
            if (!string.IsNullOrEmpty(order)) resource.Append("/order/" + order);
            if (includes != null) resource.Append("?includes=" + string.Join(",", includes));

            // clean resource if we already have a base url with trailing '/'
            if (_restClient.BaseAddress.AbsolutePath.EndsWith("/") && resource.ToString().StartsWith("/"))
                resource.Remove(0, 1);

            var requestUrl = new Uri(_restClient.BaseAddress, resource.ToString());
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

                    if (paramMode == HttpParamMode.XFORM || paramMode == HttpParamMode.MULTIPART)
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
            catch (Exception)
            {
                ; // ignore
            }

            if (authToken is null)
            {
                if (!isMiscClient && CurrentAuthToken != null )
                {
                    // Use Resource Owner AuthToken
                    request.Headers.Authorization =
                        new AuthenticationHeaderValue(CurrentAuthToken.TokenType, CurrentAuthToken.AccessToken);
                }
            }
            else
            {
                if (!isMiscClient)
                {
                    // Use Client Credential AuthToken
                    request.Headers.Authorization =
                            new AuthenticationHeaderValue(authToken.TokenType, authToken.AccessToken);
                }
            }

            // Add Body
            if (parameters != null && parameters.Any() && paramMode == HttpParamMode.BODY)
            {
                // from params - body
                string data = JsonConvert.SerializeObject(parameters, SerializationSettings);
                request.Content = new StringContent(data, Encoding.UTF8, MIME_JSON);
            }

            // MULTIPART
            if (parameters != null && parameters.Any() && paramMode == HttpParamMode.MULTIPART)
            {
                // from params - multipart
                var multipartData = parameters.Values.Single(x => x.GetType() == typeof(byte[]));

                if (multipartData != null)
                {
                    var fileStreamContent = new ByteArrayContent(multipartData as byte[]);

                    // if no MIME type specified, use default image/jpeg
                    string mimeType = "image/jpeg";

                    // else get the mimeType value
                    if (metaData != null && metaData.ContainsKey(MetaData.MimeType))
                        mimeType = metaData[MetaData.MimeType];

                    // if no filename specified, we will use random Guid file name
                    string fileName = Guid.NewGuid().ToString().Trim('{', '}');

                    // else get the fileName value
                    if (metaData != null && metaData.ContainsKey(MetaData.FileName))
                        fileName = metaData[MetaData.FileName];

                    fileStreamContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data") { Name = "file", FileName = fileName };
                    fileStreamContent.Headers.ContentType = new MediaTypeHeaderValue(mimeType);

                    var formData = new MultipartFormDataContent { { fileStreamContent, "file" } };

                    foreach (var parameter in parameters)
                    {
                        // do not include content data
                        if (parameter.Value.GetType() == typeof(byte[]))
                            continue;

                        // check for primitive
                        if (!parameter.Value.GetType().IsPrimitive)
                            formData.Add(new StringContent(JsonConvert.SerializeObject(parameter.Value)), parameter.Key);
                        else
                            formData.Add(new StringContent(parameter.Value.ToString()), parameter.Key);
                    }

                    request.Content = formData;
                }
                else
                {
                    throw new Exception("There is no media content in the parameter");
                }
            }

            if (parameters != null && parameters.Any() && paramMode == HttpParamMode.XFORM)
            {
                // from params - xform
                var data1 = parameters.ToDictionary(p => p.Key, p => p.Value?.ToString());
                request.Content = new FormUrlEncodedContent(data1);
            }
            else if ((typeof(TBody).GetTypeInfo().IsValueType && !Equals(requestBody, default(TBody)))
                || (!typeof(TBody).GetTypeInfo().IsValueType && requestBody != null))
            {
                // from body object
                string data2 = JsonConvert.SerializeObject(requestBody, SerializationSettings);
                request.Content = new StringContent(data2, Encoding.UTF8, MIME_JSON);
            }

            // TODO : UNCOMMENT / REFACTOR FOR IDS FLOW
            //request = _apiConfiguration.UrlRewriteProvider.Rewrite(request);

            return (request, _restClient);
        }

        /// <summary>
        /// Process HTTP response.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="info">The information.</param>
        /// <param name="isServiceResponse">if set to <c>true</c> [is service response].</param>
        /// <returns>
        /// The <see cref="ServiceResponse{TResult}" />.
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
                return new ServiceResponse<TResult>(ServiceStatus.Error, "Error processing request", message)
                { ElapsedTime = elapsedTime, StatusCode = StatusCode };
            }

            if (info.Exception != null && response == null)
            {
                string message = info.Exception.GetInnermostExceptionMessage();
                return new ServiceResponse<TResult>(ServiceStatus.Error, "Error processing request", message) { ElapsedTime = elapsedTime };
            }

            if (info.Exception == null && response == null)
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


                string stringData = null;
                try
                {
                    var s = await response.Content?.ReadAsStreamAsync();
                    stringData = s.StreamToString();
                }
                catch
                {
                    // do nothing , jon snow
                }

                return new ServiceResponse<TResult>(ServiceStatus.Error, "Bad response received from server", message, stringData: stringData) { StatusCode = StatusCode };
            }

            HttpContent content = response.Content;

            string json = string.Empty;

            try
            {
                // only allow attachment
                if (typeof(TResult) == typeof(Stream) && content.Headers.ContentDisposition.Equals(new ContentDispositionHeaderValue("attachment")))
                {
                    var stream = await content.ReadAsStreamAsync().ConfigureAwait(false);
                    data = new ServiceResponse<TResult>(true) { Stream = stream };
                }
                else if (typeof(TResult) == typeof(byte[]))
                {
                    var bytes = await content.ReadAsByteArrayAsync().ConfigureAwait(false);
                    data = new ServiceResponse<TResult>(true) { Bytes = bytes };
                }
                else
                {
                    try
                    {
                        // await content.ReadAsStringAsync(); <- currently causes hard crash
                        var s = await content.ReadAsStreamAsync().ConfigureAwait(false);
                        json = s.StreamToString();
                    }
                    catch { throw; }

                    var errorContentCheck = CheckForErrorContent<TResult>(json);
                    if (errorContentCheck != null)
                        return errorContentCheck;

                    if (!isServiceResponse)
                    {
                        if (typeof(TResult) == typeof(string))
                        {
                            try
                            {
                                data = new ServiceResponse<TResult>(ServiceStatus.Success, stringData: json);
                            }
                            catch { throw; }
                        }
                        else
                        {
                            data = JsonConvert.DeserializeObject<TResult>(json, DeserializationSettings)
                                .AsServiceResponse();
                        }
                    }
                    else
                    {
                        try
                        {
                            data = JsonConvert.DeserializeObject<ServiceResponse<TResult>>(json, DeserializationSettings);
                        }
                        catch { throw; }
                    }
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
        /// Send request.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="client">The client.</param>
        /// <param name="request">The request.</param>
        /// <param name="isServiceResponse">if set to <c>true</c> [is service response].</param>
        /// <returns>
        /// The <see cref="ServiceResponse{TResult}" />.
        /// </returns>
        /// <exception cref="NetworkException"></exception>
        private async Task<ServiceResponse<TResult>> SendRequest<TResult>(HttpClient client, HttpRequestMessage request, bool isServiceResponse)
        {
            var cts = new CancellationTokenSource();

            Debug.WriteLine($"RestClient: Sending request '{client.BaseAddress} - {request.RequestUri}'");

            DateTime startTime = DateTime.UtcNow;
            HttpResponseMessage response = null;
            Exception exception = null;

            try
            {
#if DEBUG
                response = client.SendAsync(request, HttpCompletionOption.ResponseContentRead, cts.Token).Result;
#else
                response =
 await client.SendAsync(request, HttpCompletionOption.ResponseContentRead, cts.Token).ConfigureAwait(false);
#endif

                Debug.WriteLine($"RestClient: Success");

            }
            catch (WebException)
            {
                // connectivity issue
                throw new NetworkException();
            }
            catch (Exception e)
            {
                Debug.WriteLine("RestClient error:" + e.Message);
                exception = e;
            }
            finally
            {
                cts.Dispose();
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

        public void SetCurrentAuthToken(AuthToken authToken)
        {
            CurrentAuthToken = authToken;
            // can add more functions/methods here, such as logging, etc.
        }

        public void SetStatusCode(HttpStatusCode statusCode)
        {
            StatusCode = (int)statusCode;
            // can add more functions/methods here, such as logging, etc.
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
