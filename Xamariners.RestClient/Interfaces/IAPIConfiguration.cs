using Xamariners.RestClient.Providers;

namespace Xamariners.RestClient.Interfaces
{
    public interface IApiConfiguration
    {
        string BaseUrl { get;  }
        double ApiTimeout { get; }
        string LocalBaseUrl { get; }
        string IdentityBaseUrl { get; }
        string IdentityClientId { get; }
        string IdentityClientSecret { get; }
        string IdentityScope { get; }
        bool EnableUrlRewrite { get; }
    }
}