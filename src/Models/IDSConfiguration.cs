using Xamariners.RestClient.Interfaces;

namespace Xamariners.RestClient.Models
{
    public class IDSConfiguration : IIDSConfiguration
    {
        public IDSConfiguration(string identityBaseUrl,
            string identityClientId,
            string identityTenantId,
            string identityClientSecret,
            string identityClientResource,
            string identityScope,
            string identityRegistrationScope
            )
        {
            IdentityClientId = identityClientId;
            IdentityTenantId = identityTenantId;
            IdentityClientSecret = identityClientSecret;
            IdentityScope = identityScope;
            IdentityRegistrationScope = identityRegistrationScope;
        }

    
        public string IdentityClientId { get; }
        public string IdentityTenantId { get; }
        public string IdentityClientSecret { get; }
        public string IdentityScope { get; }
        public string IdentityRegistrationScope { get; }
    }
}
