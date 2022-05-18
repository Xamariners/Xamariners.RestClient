using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Shouldly;
using Xamariners.RestClient.Helpers.Models;
using Xamariners.RestClient.Infrastructure;
using Xamariners.RestClient.Services;
using Xunit;

namespace Xamariners.RestClient.Test
{
    public class IDSClientTest
    {
        public static Dictionary<string, string> Headers => new Dictionary<string, string>
            {{Constants.APIM_SUBSCRIPTION, Startup.Config.IdentityServer.ApimKey}};

        public IDSClientTest()
        {
            Startup.Init();

            RestClientIDS.GetInstance(Startup.Config.IdentityServer.BaseUrl,
                Startup.Config.IdentityServer.ClientId,
                Startup.Config.IdentityServer.Scope,
                Startup.Config.IdentityServer.TenantId,
                Startup.Config.IdentityServer.RegistrationScope);
        }

        public async Task GetClientCredentialToken()
        {
            var response = await RestClientIDS.Current.GetClientCredentialToken(ScopeType.Registration, null, Headers);

            response.IsOK().ShouldBeTrue();

            response.Data.ShouldNotBeNull();

            string.IsNullOrEmpty(response.Data.AccessToken).ShouldBeFalse();

            response.Data.ExpiresAt.ShouldBeGreaterThan(DateTime.UtcNow);

            RestClientIDS.Current.SetCurrentAuthToken(response.Data);
        }


        public async Task<AuthToken> GetPasswordToken()
        {
            var response = await RestClientIDS.Current.GetPasswordToken(
                Startup.Config.IdentityServer.Username, Startup.Config.IdentityServer.Password, RestClientIDS.Current.CurrentAuthToken,
                Headers);

            response.IsOK().ShouldBeTrue();

            response.Data.ShouldNotBeNull();

            string.IsNullOrEmpty(response.Data.AccessToken).ShouldBeFalse();

            response.Data.ExpiresAt.ShouldBeGreaterThan(DateTime.UtcNow);

            return response.Data;
        }

        public async Task GetRefreshToken(AuthToken token)
        {
            RestClientIDS.Current.SetCurrentAuthToken(token);

            var response = await RestClientIDS.Current.RefreshPasswordToken(Headers);

            response.IsOK().ShouldBeTrue();

            response.Data.ShouldNotBeNull();

            string.IsNullOrEmpty(response.Data.AccessToken).ShouldBeFalse();

            response.Data.ExpiresAt.ShouldBeGreaterThan(DateTime.UtcNow);

            RestClientIDS.Current.SetCurrentAuthToken(response.Data);
        }

        public async Task GetPocoUsingIDSToken()
        {
            var body = new { email = Startup.Config.IdentityServer.Username, flow = 1 };
            var headers = new Dictionary<string, string> { { Constants.APIM_SUBSCRIPTION, Startup.Config.RenewPlus.ApimKey } };

            var response = await RestClientIDS.Current.ExecuteAsync<string, object>(
                requestVerb: HttpVerb.POST,
                action: "otp/GetOTP",
                requestBody: body,
                apiRoutePrefix: "api",
                isServiceResponse: true,
                headers: headers);

            response.IsOK().ShouldBeTrue();

            response.Message.ShouldNotBeNull();
            response.Message.ShouldContain("OTP sent to email address");
        }

        [Fact]
        public async Task TestAllIDSFlows()
        {
            // Uses Live URL
            // Need to setup a live IDS instance tu use solely for testing at CI/CD Time
            // and point our stuff there
            // may disable when satisfied with dev
            // TODO: COMMENTED AS WE DO NOT HAVE AN API TO RUN THESE TESTS
            //await GetClientCredentialToken();
            //var token = await GetPasswordToken();
            //await GetRefreshToken(token);
            //await GetPocoUsingIDSToken();
        }
    }
}
