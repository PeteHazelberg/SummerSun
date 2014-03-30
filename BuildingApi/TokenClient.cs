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
    public class TokenClient : ITokenProvider
    {
        private readonly ILog log = LogManager.GetLogger<TokenClient>();

        private static readonly ConcurrentDictionary<string, Token> Cache = new ConcurrentDictionary<string, Token>();

        private readonly string clientId;
        private readonly string credentials;
        private readonly string endpointUrl;
        private readonly IWebProxy proxy;

        /// <summary>
        /// The TokenProvider is used to manage getting security tokens
        /// </summary>
        /// <param name="id">client id (a.k.a. application id)</param>
        /// <param name="secret">client secret</param>
        /// <param name="endpoint">token issueing endpoint</param>
        /// <param name="proxy">proxy with credentials</param>
        public TokenClient(string id, string secret, string endpoint, IWebProxy proxy)
        {
            this.endpointUrl = endpoint;
            this.proxy = proxy;
            this.clientId = id;
            // base 64 encode client and secret for the Authorization header
            credentials = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(id + ":" + secret));
        }

        /// <summary>
        /// Get a security token
        /// </summary>
        /// <param name="company"></param>
        /// <param name="invalidateCache">if set to true, requests a new token even if a valid one is already in the cache</param>
        /// <returns></returns>
        public Token Get(Company company = null, bool invalidateCache = false)
        {
            var now = DateTime.UtcNow;
            Token token;

            // so that instances created by different applications (as indicated by id passed at construction) don't clobber each other
            string cacheKey = String.Format("{0}:{1}", clientId, company == null ? string.Empty: company.Id);

            // check to see if we already have a token that will be valid for at least the next five minutes
            if (!invalidateCache && Cache.ContainsKey(cacheKey) && Cache[cacheKey].ExpirationTime > now.AddMinutes(5))
            {
                token = Cache[cacheKey];
            }
            else
            {
                token = new Token
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
                        var contentList = new List<KeyValuePair<string, string>>
                        {
                            new KeyValuePair<string, string>("grant_type", "client_credentials")
                        };

                        if (company != null && company.Id != null)
                        {
                            contentList.Add(new KeyValuePair<string, string>("jci_company_id", company.Id));
                            contentList.Add(new KeyValuePair<string, string>("scope", "panoptix.read panoptix.write panoptix.manage panoptix.delete"));
                        }
                        var content = new FormUrlEncodedContent(contentList);

                        var response = client.PostAsync(endpointUrl, content).Result;
                        token.Status = response.StatusCode;
                        if (response.IsSuccessStatusCode)
                        {
                            var item = JsonConvert.DeserializeObject<Token>(response.Content.ReadAsStringAsync().Result, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver(), NullValueHandling = NullValueHandling.Ignore });
                            token.AccessToken = item.AccessToken;
                            token.ExpirationTime = now + TimeSpan.FromSeconds(item.ExpiresInSeconds);
                            token.Company = company;

                            Cache.AddOrUpdate(cacheKey, token, (key, value) =>
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
                            log.WarnFormat(message);
                            Token removeToken;
                            Cache.TryRemove(cacheKey, out removeToken);
                            throw new HttpRequestException(message);
                        }
                    }
                }
                catch (Exception e)
                {
                    log.WarnFormat("{0}\n{1}",e.Message,e.StackTrace);
                    Token removeToken;
                    Cache.TryRemove(cacheKey, out removeToken);
                    throw;
                }
            }
            return token;
        }
    }
}