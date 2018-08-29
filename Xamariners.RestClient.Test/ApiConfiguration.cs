using System;
using System.Collections.Generic;
using System.Text;
using Xamariners.RestClient.Interfaces;

namespace Xamariners.RestClient.Test
{
    public class ApiConfiguration : IApiConfiguration
    {
        public string BaseUrl => "https://www.test.com";
        public double ApiTimeout => 1000;
        public string LocalBaseUrl => "http://localhost";
        public string IdentityBaseUrl => "localhost/identity";
        public string IdentityClientId => "identityClientId";
        public string IdentityClientSecret => "identityClientSecret";
        public string IdentityScope => "TestClient";
        public bool EnableUrlRewrite => false;
    }
}
