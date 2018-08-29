using System;
using Xamariners.RestClient.Interfaces;
using Xamariners.RestClient.Providers;

namespace Xamariners.RestClient.Models
{
    public class RestConfiguration : IRestConfiguration
    {
        const double DEFAULT_TIMEOUT = 30000;

        public RestConfiguration(ICredentialsProvider credentialsProvider, IApiConfiguration apiConfiguration, IUrlRewriteProvider urlRewriteProvider = null)
        {
            CredentialsProvider = credentialsProvider;
            ApiConfiguration = apiConfiguration;
            UrlRewriteProvider = urlRewriteProvider;

            if (apiConfiguration.ApiTimeout <= 0) TimeOut = DEFAULT_TIMEOUT;
            else TimeOut = apiConfiguration.ApiTimeout;

            if (CredentialsProvider == null)
                throw new ArgumentNullException("CredentialsProvider should not be null");

            if (ApiConfiguration == null)
                throw new ArgumentNullException("ApiConfiguration should not be null");
        }

        /// <summary>
        /// Gets or sets the credentials provider.
        /// </summary>
        public ICredentialsProvider CredentialsProvider { get; set; }

        /// <summary>
        /// Gets or sets the URL rewrite provider.
        /// </summary>
        public IUrlRewriteProvider UrlRewriteProvider { get; set; }

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