using Xamariners.RestClient.Interfaces;

namespace Xamariners.RestClient.Models
{
    public class BasicAuthConfiguration : IBasicAuthConfiguration
    {
        public BasicAuthConfiguration(string basicAuthUrl)
        {
            BasicAuthUrl = basicAuthUrl;
        }

        public string BasicAuthUrl { get; }
    }
}
