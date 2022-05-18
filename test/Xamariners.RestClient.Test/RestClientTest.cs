using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Shouldly;
using Xamariners.RestClient.Helpers.Models;
using Xamariners.RestClient.Infrastructure;
using Xamariners.RestClient.Services;
using Xunit;

namespace Xamariners.RestClient.Test
{
    public class RestClientTest
    {
        public RestClientTest()
        {
            Startup.Init();

            RestClientNoAuth.GetInstance(Startup.Config.BaseUrlHttpBin);
        }

        [Fact]
        public async Task DefaultHeaders()
        {
            var response = await RestClientNoAuth.Current.ExecuteAsync<string>(
                HttpVerb.GET,
                action: "headers",
                apiRoutePrefix: "/",
                isServiceResponse: false);

            response.IsOK().ShouldBeTrue();
            response.StringData.ShouldNotBeNull();

            var body = JObject.Parse(response.StringData);
            body["headers"]["Accept"].ToString().ShouldBe("application/json");
        }

        [Fact]
        public async Task AddHeader()
        {
            var headers = new Dictionary<string, string> { { "environment", "test" } };

            var response = await RestClientNoAuth.Current.ExecuteAsync<string>(
                HttpVerb.GET,
                action: "headers",
                apiRoutePrefix: "/",
                headers: headers,
                isServiceResponse: false);

            response.IsOK().ShouldBeTrue();
            response.StringData.ShouldNotBeNull();

            var body = JObject.Parse(response.StringData);
            body["headers"]["Accept"].ToString().ShouldBe("application/json");
            body["headers"]["Environment"].ToString().ShouldBe("test");
        }

        [Fact]
        public async Task WhenXFORMAddsHeader()
        {
            var headers = new Dictionary<string, string> { { "environment", "test" } };

            var response = await RestClientNoAuth.Current.ExecuteAsync<string>(
                HttpVerb.GET,
                action: "headers",
                apiRoutePrefix: "/",
                headers: headers,
                isServiceResponse: false,
                paramMode: HttpParamMode.XFORM);

            response.IsOK().ShouldBeTrue();
            response.StringData.ShouldNotBeNull();

            var body = JObject.Parse(response.StringData);
            body["headers"]["Accept"].ToString().ShouldBe("application/x-www-form-urlencoded");
        }

        [Fact]
        public async Task WhenMULTIPARTAddsHeader()
        {
            var headers = new Dictionary<string, string> { { "environment", "test" } };

            var response = await RestClientNoAuth.Current.ExecuteAsync<string>(
                HttpVerb.GET,
                action: "headers",
                apiRoutePrefix: "/",
                headers: headers,
                isServiceResponse: false,
                paramMode: HttpParamMode.MULTIPART);

            response.IsOK().ShouldBeTrue();
            response.StringData.ShouldNotBeNull();

            var body = JObject.Parse(response.StringData);
            body["headers"]["Accept"].ToString().ShouldBe("application/x-www-form-urlencoded");
        }

        [Fact]
        public async Task WhenAuthTokenAddsHeader()
        {
            var authToken = new AuthToken { AccessToken = "TestAccessToken", TokenType = "Basic" };

            var response = await RestClientNoAuth.Current.ExecuteAsync<string>(
                HttpVerb.GET,
                authToken: authToken,
                action: "headers",
                apiRoutePrefix: "/",
                isServiceResponse: false,
                paramMode: HttpParamMode.MULTIPART);

            response.IsOK().ShouldBeTrue();
            response.StringData.ShouldNotBeNull();

            var body = JObject.Parse(response.StringData);
            body["headers"]["Authorization"].ToString().ShouldBe($"{authToken.TokenType} {authToken.AccessToken}");
        }

        [Fact]
        public async Task WithBody()
        {
            var body = new TestUser { Email = "test@xamariners.com", GroupId = Guid.NewGuid() };

            var response = await RestClientNoAuth.Current.ExecuteAsync<string, TestUser>(
                HttpVerb.GET,
                requestBody: body,
                action: "anything",
                apiRoutePrefix: "/",
                isServiceResponse: true,
                paramMode: HttpParamMode.BODY);

            response.IsOK().ShouldBeTrue();
            response.Data.ShouldNotBeNull();
            response.Data.ShouldBe(JsonConvert.SerializeObject(body));
        }

        [Fact]
        public async Task WhenXFORMAddsParameters()
        {
            var parameters = new Dictionary<string, object> { { "Email", "test@xamariners.com" } };

            var response = await RestClientNoAuth.Current.ExecuteAsync<string>(
                HttpVerb.POST,
                action: "anything",
                apiRoutePrefix: "/",
                isServiceResponse: false,
                paramMode: HttpParamMode.XFORM,
                parameters: parameters);

            response.IsOK().ShouldBeTrue();
            response.StringData.ShouldNotBeNull();

            var data = JObject.Parse(response.StringData);
            var formData = Regex.Replace(data["form"].ToString(), @"\s+", "");
            formData.ShouldBe(JsonConvert.SerializeObject(parameters));
        }
    }
}
