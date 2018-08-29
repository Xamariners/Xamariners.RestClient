using System;
using System.Collections.Generic;
using System.Text;
using Xamariners.RestClient.Models;
using Xamariners.RestClient.Providers;

namespace Xamariners.RestClient.Test
{
    public class CredentialsProvider : ICredentialsProvider
    {
        public AuthToken AuthToken { get; set; }
        public String Password { get; set; }
        public String Username { get; set; }
        public Guid UserID { get; set; }
        public String UserRole { get; set; }
    }
}
