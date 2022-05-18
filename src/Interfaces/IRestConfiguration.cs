namespace Xamariners.RestClient.Interfaces
{
    public interface IRestConfiguration
    {
        IApiConfiguration ApiConfiguration { get; set; }

        double TimeOut { get; }
    }
}
