using System.Collections.Generic;
using System.Linq;
using Flurl;

namespace BuildingApi
{
    public class ApiClient
    {
        private readonly string baseUrl;
        private readonly ITokenProvider tokens;
        public ApiClient(ITokenProvider tokenProvider, string baseUrl)
        {
            this.tokens = tokenProvider;
            this.baseUrl = baseUrl;
        }

        public string BaseUrl { get { return baseUrl; } }

        public IEnumerable<T> GetPage<T>(Url url, Company company)
        {
            var resp = HttpHelper.Get<Page<T>>(company, url.ToString(), tokens);
            return (resp == null || resp.Items == null) ? new List<T>() : resp.Items;
        }

        public List<Company> GetCompanies()
        {
            var token = this.tokens.Get();
            return HttpHelper.Get<List<Company>>(BaseUrl.AppendPathSegment("companies").ToString(), token);
        }
    }
}
