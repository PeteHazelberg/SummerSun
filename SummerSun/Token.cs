using System;
using System.Net;
using Newtonsoft.Json;

namespace BuildingApi
{
    public class Token
    {
        [JsonProperty(PropertyName = "access_token")]
        public string AccessToken { get; set; }

        [JsonProperty(PropertyName = "expires_in")]
        public int ExpiresInSeconds { get; set; }
        
        [JsonIgnore]
        public DateTime ExpirationTime { get; set; }

        [JsonIgnore]
        public Guid CustomerId { get; set; }

        [JsonIgnore]
        public HttpStatusCode Status { get; set; }

        [JsonIgnore]
        public IWebProxy Proxy { get; set; }
    }
}