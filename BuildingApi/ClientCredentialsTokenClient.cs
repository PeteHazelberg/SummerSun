using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;

namespace BuildingApi
{
    public class ClientCredentialsTokenClient : TokenClientBase, ITokenProvider
    {
        private static readonly ConcurrentDictionary<string, Token> Cache = new ConcurrentDictionary<string, Token>();

        /// <summary>
        /// The TokenProvider is used to manage getting security tokens
        /// </summary>
        /// <param name="id">client id (a.k.a. application id)</param>
        /// <param name="secret">client secret</param>
        /// <param name="endpoint">token issueing endpoint</param>
        /// <param name="proxy">proxy with credentials</param>
        public ClientCredentialsTokenClient(string id, string secret, string endpoint, IWebProxy proxy)
            : base(id, secret, endpoint, proxy)
        {
        }

        /// <summary>
        /// Get a security token
        /// </summary>
        /// <param name="company"></param>
        /// <param name="scope"></param>
        /// <param name="invalidateCache">if set to true, requests a new token even if a valid one is already in the cache</param>
        /// <returns></returns>
        public Token Get(Company company = null, string scope = "panoptix.read", bool invalidateCache = false)
        {
            var now = DateTime.UtcNow;
            Token token;

            // so that instances created by different applications (as indicated by id passed at construction) don't clobber each other
            var cacheKey = String.Format("{0}:{1}:{2}", clientId, company == null ? string.Empty: company.Id, scope);

            // check to see if we already have a token that will be valid for at least the next five minutes
            if (!invalidateCache && Cache.ContainsKey(cacheKey) && Cache[cacheKey].ExpirationTime > now.AddMinutes(5))
            {
                token = Cache[cacheKey];
            }
            else
            {
                var contentList = new List<KeyValuePair<string, string>>
                        {
                            new KeyValuePair<string, string>("grant_type", "client_credentials")
                        };

                if (company != null && company.Id != null)
                {
                    contentList.Add(new KeyValuePair<string, string>("jci_company_id", company.Id));
                    contentList.Add(new KeyValuePair<string, string>("scope", scope));
                }

                token = FetchToken(company, cacheKey, Cache, contentList);
            }
            return token;
        }
    }
}