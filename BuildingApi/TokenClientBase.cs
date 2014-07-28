using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Common.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace BuildingApi
{
    public abstract class TokenClientBase
    {
        protected readonly IWebProxy proxy;
        protected readonly string clientId;
        protected readonly string credentials;
        protected readonly string endpointUrl;
        private static readonly ILog Log = LogManager.GetLogger<TokenClientBase>();

        protected TokenClientBase(string id, string secret, string endpointUrl, IWebProxy proxy)
        {
            this.endpointUrl = endpointUrl;
            this.clientId = id;
            // base 64 encode client and secret for the Authorization header
            credentials = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(id + ":" + secret));
            this.proxy = proxy;
        }

        protected Token FetchToken(Company company, string cacheKey, ConcurrentDictionary<string, Token> cache, List<KeyValuePair<string, string>> queryParameters)
        {
            var now = DateTime.UtcNow;

            var token = new Token
                {
                    Status = HttpStatusCode.InternalServerError
                };
            try
            {
                var clientHandler = new HttpClientHandler();
                if (proxy != null)
                {
                    clientHandler.Proxy = proxy;
                    token.Proxy = proxy;
                }

                using (var client = new HttpClient(clientHandler))
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);

                    var content = new FormUrlEncodedContent(queryParameters);

                    var response = client.PostAsync(endpointUrl, content).Result;
                    token.Status = response.StatusCode;
                    if (response.IsSuccessStatusCode)
                    {
                        var item = JsonConvert.DeserializeObject<Token>(response.Content.ReadAsStringAsync().Result, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver(), NullValueHandling = NullValueHandling.Ignore });
                        token.AccessToken = item.AccessToken;
                        token.ExpirationTime = now + TimeSpan.FromSeconds(item.ExpiresInSeconds);
                        token.Company = company;

                        cache.AddOrUpdate(cacheKey, token, (key, value) =>
                            {
                                value.AccessToken = token.AccessToken;
                                value.ExpirationTime = token.ExpirationTime;
                                value.Status = token.Status;
                                value.Company = token.Company;
                                return value;
                            });
                    }
                    else
                    {
                        var errorDescription = response.Content.ReadAsStringAsync().Result;
                        var message = string.Format("Unable to Get Token from: {0}{1}Status: {2} {3} {1}Reason: {4} ErrorDescription: {5}", endpointUrl, Environment.NewLine, (int)response.StatusCode, response.StatusCode, response.ReasonPhrase, errorDescription);
                        Log.WarnFormat(message);
                        Token removeToken;
                        cache.TryRemove(cacheKey, out removeToken);
                        throw new HttpRequestException(message);
                    }
                }
            }
            catch (Exception e)
            {
                Log.WarnFormat("{0}\n{1}", e.Message, e.StackTrace);
                Token removeToken;
                cache.TryRemove(cacheKey, out removeToken);
                throw;
            }

            return token;
        }

    }
}
