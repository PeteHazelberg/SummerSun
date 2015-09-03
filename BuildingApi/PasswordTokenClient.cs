using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using BuildingApi.Logging;

namespace BuildingApi
{
    public class PasswordTokenClient : TokenClientBase, ITokenProvider
    {
        private readonly ILog log = LogProvider.For<ClientCredentialsTokenClient>();

        private static readonly ConcurrentDictionary<string, Token> Cache = new ConcurrentDictionary<string, Token>();

        
        private readonly Dictionary<string, PasswordCredential> serviceAccounts;



        /// <summary>
        /// The TokenProvider is used to manage getting security tokens
        /// </summary>
        /// <param name="id">client id (a.k.a. application id)</param>
        /// <param name="secret">client secret</param>
        /// <param name="serviceAccounts">list of company and service accounts</param>
        /// <param name="endpoint">token issueing endpoint</param>
        /// <param name="proxy">proxy with credentials</param>
        public PasswordTokenClient(string id, string secret, IEnumerable<PasswordCredential> serviceAccounts, string endpoint, IWebProxy proxy)
            : base(id, secret, endpoint, proxy)
        {
            this.serviceAccounts = serviceAccounts.ToDictionary(x => x.CompanyId, x => x);

        }

        /// <summary>
        /// Get a security token
        /// </summary>
        /// <param name="company"></param>
        /// <param name="invalidateCache">if set to true, requests a new token even if a valid one is already in the cache</param>
        /// <returns></returns>
        public Token Get(Company company, string scope = "panoptix.read", bool invalidateCache = false)
        {
            if (company == null)
            {
                throw new InvalidOperationException("PasswordTokenClient can only issue tokens for a specific company.");
            }

            var now = DateTime.UtcNow;
            Token token;

            // find credentials for this company
            if (!serviceAccounts.ContainsKey(company.Id))
            {
                log.WarnFormat("No service account is available for company with id '{0}'", company.Id);

                return new Token
                {
                    AccessToken = string.Empty,
                    Status = HttpStatusCode.NotFound
                };
            }
            var serviceAccount = serviceAccounts[company.Id];

            // so that instances created by different applications (as indicated by id passed at construction) don't clobber each other
            string cacheKey = String.Format("{0}:{1}:{2}:{3}", clientId, company.Id, serviceAccount.AccountId, scope);

            // check to see if we already have a token that will be valid for at least the next five minutes
            if (!invalidateCache && Cache.ContainsKey(cacheKey) && Cache[cacheKey].ExpirationTime > now.AddMinutes(5))
            {
                token = Cache[cacheKey];
            }
            else
            {
                var contentList = new List<KeyValuePair<string, string>>
                        {
                            new KeyValuePair<string, string>("grant_type", "password"),
                            new KeyValuePair<string, string>("scope", scope),
                            new KeyValuePair<string, string>("username", serviceAccount.AccountId),
                            new KeyValuePair<string, string>("password", serviceAccount.Password),
                        };

                token = FetchToken(company, cacheKey, Cache, contentList);
            }
            return token;
        }
    }
}