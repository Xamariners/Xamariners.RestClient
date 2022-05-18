using System;
using Xamariners.RestClient.Interfaces;
using Xamariners.RestClient.Providers;

namespace Xamariners.RestClient.Models
{
    public class RestConfiguration : IRestConfiguration
    {
        const double DEFAULT_TIMEOUT = 30000;

        public RestConfiguration( IApiConfiguration apiConfiguration)
        {
            ApiConfiguration = apiConfiguration;

            if (apiConfiguration.ApiTimeout <= 0) TimeOut = DEFAULT_TIMEOUT;
            else TimeOut = apiConfiguration.ApiTimeout;
            
            if (ApiConfiguration == null)
                throw new ArgumentNullException("ApiConfiguration should not be null");
        }
        
        /// <summary>
        /// Gets or sets the API configuration.
        /// </summary>
        public IApiConfiguration ApiConfiguration { get; set; }

        /// <summary>
        /// Gets or sets the time out.
        /// </summary>
        public double TimeOut { get; }
    }
}