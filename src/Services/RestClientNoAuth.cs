// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RestClientBasicAuth.cs" company="">
//
// </copyright>
// <summary>
//   The rest client.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamariners.RestClient.Helpers.Models;
using Xamariners.RestClient.Interfaces;
using Xamariners.RestClient.Models;

namespace Xamariners.RestClient.Services
{
    /// <summary>
    ///     The rest client.
    /// </summary>
    /// <typeparam name="T">
    /// </typeparam>
    public class RestClientNoAuth : RestClient
    {
        public static RestClientNoAuth Current { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RestClient" /> class.
        /// Initializes a new instance of the <see cref="RestClient{T}" /> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        public RestClientNoAuth(IApiConfiguration configuration) : base(configuration)
        {
            SerializationSettings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore,
                DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                DateFormatHandling = DateFormatHandling.IsoDateFormat
            };
        }

        public async override Task<ServiceResponse<AuthToken>> GetPasswordToken(string username, string password, AuthToken authToken, Dictionary<string, string> headers)
        {
            return new ServiceResponse<AuthToken>();
        }

        public static RestClientNoAuth GetInstance(string baseUrl, int timeout = 30000)
        {
            var apiConfiguration = new ApiConfiguration(baseUrl, timeout, false);
            Current = new RestClientNoAuth(apiConfiguration);
            return Current;
        }
    }
}
