namespace Xamariners.RestClient.Interfaces
{
    public interface IIDSConfiguration
    {
        // IdentityServer USER API Authentication
        string IdentityClientId { get; }
        string IdentityTenantId { get; }
        string IdentityClientSecret { get; }
        string IdentityScope { get; }

        // IdentityServer REGISTRATION API Authentication
        string IdentityRegistrationScope { get; }
    }
}
