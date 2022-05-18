namespace Xamariners.RestClient.Interfaces
{
    public interface IApiConfiguration
    {
        string BaseUrl { get; }
        double ApiTimeout { get; }
        bool EnableUrlRewrite { get; }
    }
}