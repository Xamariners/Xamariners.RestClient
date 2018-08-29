using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Xamariners.RestClient.Models
{
    public class AuthToken
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("token_type")]
        public string TokenType { get; set; }
        
        [JsonProperty("expires_in")]
        public long ExpiresIn { get; set; }
        
        [JsonProperty("username")]
        public string Username { get; set; }
        
        [JsonProperty(".issued")]
        public DateTime IssuedAt { get; set; }
        
        [JsonProperty(".expires")]
        public DateTime ExpiresAt { get; set; }
    }
}