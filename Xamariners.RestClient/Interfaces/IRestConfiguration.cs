using Xamariners.RestClient.Providers;

namespace Xamariners.RestClient.Interfaces
{
    public interface IRestConfiguration
    {
        ICredentialsProvider CredentialsProvider { get; set; }

        // Others setup properties can be added here
        IUrlRewriteProvider UrlRewriteProvider { get; set; }

        IApiConfiguration ApiConfiguration { get; set; }

        double TimeOut { get; }
    }
}
