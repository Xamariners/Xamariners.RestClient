using System;
using System.Collections.Generic;
using System.Security;
using System.Threading.Tasks;
using Shouldly;
using Xamariners.RestClient.Helpers.Models;
using Xamariners.RestClient.Infrastructure;
using Xamariners.RestClient.Services;
using Xunit;

namespace Xamariners.RestClient.Test
{
    public class MSALClientTest
    {
        public static Dictionary<string, string> Headers => new Dictionary<string, string>
            {{Constants.APIM_SUBSCRIPTION, Startup.Config.MicrosoftAuth.ApimKey}};

        public MSALClientTest()
        {
            Startup.Init();

            RestClientNoAuth.GetInstance(Startup.Config.BaseUrlMicrosoftLoginUrl);

            RestClientMSAL.GetInstance(Startup.Config.MicrosoftAuth.BaseUrl,
                Startup.Config.MicrosoftAuth.ClientId,
                Startup.Config.MicrosoftAuth.Scope,
                Startup.Config.MicrosoftAuth.ClientSecret,
                Startup.Config.MicrosoftAuth.RegistrationScope,
                Startup.Config.MicrosoftAuth.TenantId);
        }

        public async Task GetClientCredentialToken()
        {
            var response = await RestClientMSAL.Current.GetClientCredentialToken(ScopeType.Registration, null, Headers);

            response.IsOK().ShouldBeTrue();

            response.Data.ShouldNotBeNull();

            string.IsNullOrEmpty(response.Data.AccessToken).ShouldBeFalse();

            response.Data.ExpiresAt.ShouldBeGreaterThan(DateTime.UtcNow);

            RestClientMSAL.Current.SetCurrentAuthToken(response.Data);
        }

        public async Task<AuthToken> GetPasswordToken()
        {
            var response = await RestClientMSAL.Current.GetPasswordToken(
                Startup.Config.MicrosoftAuth.Username, Startup.Config.MicrosoftAuth.Password, RestClientMSAL.Current.CurrentAuthToken,
                Headers);

            response.IsOK().ShouldBeTrue();

            response.Data.ShouldNotBeNull();

            string.IsNullOrEmpty(response.Data.AccessToken).ShouldBeFalse();

            response.Data.ExpiresAt.ShouldBeGreaterThan(DateTime.UtcNow);

            return response.Data;
        }

        public async Task GetRefreshToken(AuthToken token)
        {
            RestClientMSAL.Current.SetCurrentAuthToken(token);

            var response = await RestClientMSAL.Current.RefreshPasswordToken(Headers);

            response.IsOK().ShouldBeTrue();

            response.Data.ShouldNotBeNull();

            string.IsNullOrEmpty(response.Data.AccessToken).ShouldBeFalse();

            response.Data.ExpiresAt.ShouldBeGreaterThan(DateTime.UtcNow);

            RestClientMSAL.Current.SetCurrentAuthToken(response.Data);
        }

        public async Task GetPocoUsingMSALToken()
        {
            var response = await RestClientMSAL.Current
                .ExecuteAsync<TestUser, Guid>(
                    HttpVerb.GET,
                    "getorcreateuser",
                    isServiceResponse: true,
                    apiRoutePrefix: "user",
                    headers: Headers);

            response.IsOK().ShouldBeTrue();

            response.Data.ShouldNotBeNull();

            response.Data.Email.Length.ShouldBeGreaterThan(0);
        }

        public async Task Authenticate()
        {
            var response = await RestClientMSAL.Current.AdAuthService.AuthenticateUsernamePassword(Startup.Config.MicrosoftAuth.Username, ConvertToSecureString(Startup.Config.MicrosoftAuth.Password));
            
            response.IsOK().ShouldBeTrue();
            response.Data.authToken.ShouldNotBeNull();
            
            RestClientMSAL.Current.SetCurrentAuthToken(response.Data.authToken);
        }

        [Fact]
        public async Task TestAllMSALFlows()
        {

            // TODO: COMMENTED AS WE DO NOT HAVE AN API TO RUN THESE TESTS

           
            await Authenticate();

            await GetPasswordToken();
            await GetClientCredentialToken();
            await GetPocoUsingMSALToken();
        }

        private SecureString ConvertToSecureString(string password)
        {
            if (password == null)
                throw new ArgumentNullException("password");

            var securePassword = new SecureString();

            foreach (char c in password)
                securePassword.AppendChar(c);

            securePassword.MakeReadOnly();
            return securePassword;
        }
    }
}