using Xamariners.RestClient.Interfaces;

namespace Xamariners.RestClient.Models
{
    public class ApiConfiguration : IApiConfiguration
    {
        public ApiConfiguration(string baseUrl, double apiTimeout, bool enableUrlRewrite)
        {
            BaseUrl = baseUrl;
            ApiTimeout = apiTimeout;
            EnableUrlRewrite = enableUrlRewrite;
        }

        public string BaseUrl { get; }

        public double ApiTimeout { get; }
        public bool EnableUrlRewrite { get; }
    }
}
