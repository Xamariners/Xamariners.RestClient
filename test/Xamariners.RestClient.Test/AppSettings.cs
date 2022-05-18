namespace Xamariners.RestClient.Test
{
    public class AppSettings
    {
        public double ApiTimeout { get; set; }
        public string BaseUrlBasic { get; set; }
        public string BaseUrlNoAuth { get; set; }
        public string BaseUrlMicrosoftLoginUrl { get; set; }
        public string BaseUrlHttpBin { get; set; }
        public AuthSettings IdentityServer { get; set; }
        public AuthSettings MicrosoftAuth { get; set; }
        public AuthSettings RenewPlus { get; set; }
    }

    public class AuthSettings
    {
        public string ApimKey { get; set; }
        public string BaseUrl { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string TenantId { get; set; }
        public string RegistrationScope { get; set; }
        public string Scope { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
}