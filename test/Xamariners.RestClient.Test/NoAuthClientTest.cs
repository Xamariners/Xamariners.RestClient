using System.Threading.Tasks;
using Shouldly;
using Xamariners.RestClient.Infrastructure;
using Xamariners.RestClient.Services;
using Xunit;

namespace Xamariners.RestClient.Test
{
    public class NoAuthClientTest
    {
        public NoAuthClientTest()
        {
            Startup.Init();

            RestClientNoAuth.GetInstance(Startup.Config.BaseUrlNoAuth);
        }

        [Fact]
        public async Task GetStringUsingNoAuth()
        {
            var response = await RestClientNoAuth.Current.ExecuteAsync<string>(
                requestVerb: HttpVerb.GET,
                action: "2-hour-weather-forecast",
                apiRoutePrefix: "environment");

            response.IsOK().ShouldBeTrue();

            response.StringData.ShouldNotBeNull();
        }

        [Fact]
        public async Task GetPocoUsingNoAuth()
        {
            var response = await RestClientNoAuth.Current.ExecuteAsync<WeatherResponse>(
                requestVerb: HttpVerb.GET,
                action: "2-hour-weather-forecast",
                apiRoutePrefix: "environment",
                isServiceResponse: false);

            response.IsOK().ShouldBeTrue();

            response.Data.ShouldNotBeNull();

            response.Data.api_info.status.ShouldBe("healthy");
        }
    }
}