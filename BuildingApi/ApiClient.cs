using System.Collections.Generic;
using System.Linq;
using Flurl;

namespace BuildingApi
{
    public class ApiClient
    {
        /// <summary>
        /// The base Url of the JCI Building Api (typically https://api.panoptix.com/).
        /// </summary>
        public string BaseUrl { get { return baseUrl; } }

        /// <summary>
        /// The token provider configured for this client. Either a PasswordTokenClient (typical for most applications) or a ClientCredientialsTokenClient if you have priveleged access to many customers.
        /// </summary>
        public ITokenProvider Tokens { get { return tokens; } }

        /// <summary>
        /// GET a single page of resources that are owned by a particular company. GET the next page by requesting Page.Next, if desired.
        /// </summary>
        public IEnumerable<T> GetPage<T>(Url url, Company company)
        {
            var resp = HttpHelper.Get<Page<T>>(company, url.ToString(), Tokens);
            return (resp == null || resp.Items == null) ? new List<T>() : resp.Items;
        }

        /// <summary>
        /// GET a resource that is owned by a particular company.
        /// </summary>
        public T Get<T>(Url url, Company company)
        {
            return HttpHelper.Get<T>(company, url.ToString(), Tokens);
        }

        /// <summary>
        /// GET a resource that is not specific to a particular company (e.g., list /building/types/*, /companies, etc.).
        /// </summary>
        public T Get<T>(Url url)
        {
            return HttpHelper.Get<T>(url.ToString(), Tokens.Get());
        }

        /// <summary>
        /// GET the list of all companies that you have visibility to.
        /// </summary>
        public List<Company> GetCompanies()
        {
            var token = Tokens.Get();
            return HttpHelper.Get<List<Company>>(BaseUrl.AppendPathSegment("companies").ToString(), token);
        }

        public ApiClient(ITokenProvider tokenProvider, string baseUrl)
        {
            this.tokens = tokenProvider;
            this.baseUrl = baseUrl;
        }

        private readonly string baseUrl;
        private readonly ITokenProvider tokens;
    }
}
