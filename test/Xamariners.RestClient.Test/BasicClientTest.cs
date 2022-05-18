using System;
using System.Threading.Tasks;
using Shouldly;
using Xamariners.RestClient.Helpers.Models;
using Xamariners.RestClient.Infrastructure;
using Xamariners.RestClient.Services;
using Xunit;

namespace Xamariners.RestClient.Test
{
    public class BasicClientTest
    {
        public BasicClientTest()
        {
            Startup.Init();

            // Postman mock setup
            string baseUrl = "https://postman-echo.com/basic-auth";

            RestClientBasicAuth.GetInstance(baseUrl, baseUrl, null, null, null, null);
        }

        public async Task<AuthToken> GetPasswordToken()
        {
            // username
            string username = "postman";
            string password = "password";

            var response = await RestClientBasicAuth.Current.GetPasswordToken(
                username, password, RestClientBasicAuth.Current.CurrentAuthToken, null);

            response.IsOK().ShouldBeTrue();

            response.Data.ShouldNotBeNull();

            string.IsNullOrEmpty(response.Data.AccessToken).ShouldBeFalse();

            response.Data.ExpiresAt.ShouldBeGreaterThan(DateTime.UtcNow);

            return response.Data;
        }

        public async Task GetPocoUsingBasicToken()
        {
            // TODO: replace with relevant
            var response = await RestClientBasicAuth.Current.ExecuteAsync<WeatherResponse>(
                requestVerb: HttpVerb.GET,
                action: "2-hour-weather-forecast",
                apiRoutePrefix: "environment",
                isServiceResponse: false);

            response.IsOK().ShouldBeTrue();

            response.Data.ShouldNotBeNull();

            response.Data.area_metadata.Length.ShouldBeGreaterThan(0);
        }

        [Fact(Skip = "Not done")]
        public async void TestAllBasicFlows()
        {
            // Could not find a suitable API to test against
            var token = await GetPasswordToken();
            await GetPocoUsingBasicToken();
        }
    }
}