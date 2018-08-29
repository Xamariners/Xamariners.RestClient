using System;
using Xamariners.RestClient.Infrastructure;
using Xamariners.RestClient.Interfaces;
using Xamariners.RestClient.Models;
using Xunit;

namespace Xamariners.RestClient.Test
{
    public class RestClientTests
    {
        [Fact]
        public void Test1()
        {
            var creds = new CredentialsProvider
            {
                Username = "Test",
                Password = "Password"
            };

            var apiConfig = new ApiConfiguration();

            var restClientConfig = new RestConfiguration(creds, apiConfig);
            var client = new RestClient(restClientConfig);

            var result = client.Execute<string>(HttpVerb.GET, "action");


        }
    }
}
