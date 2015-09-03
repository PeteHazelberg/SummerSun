using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BuildingApi.Logging;
using Flurl;

namespace BuildingApi
{
    [Obsolete("Use Flurl.Http package instead. This class will be removed in a future version.")]
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
        [Obsolete("Use async versions")]
        public IEnumerable<T> GetPage<T>(Url url, Company company)
        {
            var resp = HttpHelper.Get<Page<T>>(company, url.ToString(), Tokens);
            return ExtractItemsFromPage(resp);
        }

        /// <summary>
        /// GET a resource that is owned by a particular company.
        /// </summary>
        [Obsolete("Use async versions")]
        public T Get<T>(Url url, Company company)
        {
            return HttpHelper.Get<T>(company, url.ToString(), Tokens);
        }

        /// <summary>
        /// GET a resource that is not specific to a particular company (e.g., list /building/types/*, /companies, etc.).
        /// </summary>
        [Obsolete("Use async versions")]
        public T Get<T>(Url url)
        {
            return HttpHelper.Get<T>(url.ToString(), Tokens.Get());
        }

        /// <summary>
        /// GET all pages of a paged resource.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url"></param>
        /// <param name="company"></param>
        /// <returns></returns>
        [Obsolete("Use async versions")]
        public IEnumerable<T> GetAll<T>(Url url, Company company)
        {
            var resp = HttpHelper.Get<Page<T>>(company, url.ToString(), Tokens);
            var all = ExtractItemsFromPage(resp).ToList();
            while (resp != null && resp.Next != null && resp.Next.Href != null)
            {
                resp = HttpHelper.Get<Page<T>>(company, resp.Next.Href, Tokens);
                all.AddRange(ExtractItemsFromPage(resp));
            }
            return all;
        }

        /// <summary>
        /// Filter the list of resources to those that contain a specific string in their Name attribute.
        /// </summary>
        /// <returns>Null if unable to find only one instance</returns>
        public T FindExactlyOneInstanceByName<T>(Url url, string nameFragmentToSearchFor, Company company)
        {
            var urlWithFilter = new Url(url).SetQueryParam("name", nameFragmentToSearchFor);
            var instances = GetAll<T>(url, company).ToList();
            if (!instances.Any())
            {
                Log.Warn(string.Format("No {0} was found with {1} in its name.", url.Path.Last(), nameFragmentToSearchFor));
                return default(T);
            }
            if (instances.Count() == 1) return instances[0];
            if (Log.IsWarnEnabled())
            {
                var bldr = new StringBuilder("Ambiguous Name (Multiple Matches Found)");
                foreach (var inst in instances)
                {
                    dynamic instance = inst;
                    bldr.Append(Environment.NewLine).Append(instance.Name).Append("    ").Append(instance.Href);
                }
                Log.Warn(bldr.ToString());
            }
            return default(T);
        }

        [Obsolete("Use async versions")]
        public T Post<T>(Url url, Company company, T payload, string tokenScope = "panoptix.write")
        {
            return HttpHelper.Post(url.ToString(), Tokens.Get(company, tokenScope), payload);
        }

        /// <summary>
        /// GET the list of all companies that you have visibility to.
        /// </summary>
        [Obsolete("Use async versions")]
        public List<Company> GetCompanies()
        {
            var token = Tokens.Get();
            return HttpHelper.Get<List<Company>>(BaseUrl.AppendPathSegment("companies").ToString(), token);
        }

        /// <summary>
        /// GET a single page of resources that are owned by a particular company. GET the next page by requesting Page.Next, if desired.
        /// </summary>
        public async Task<IEnumerable<T>> GetPageAsync<T>(Url url, Company company)
        {
            var resp = await HttpHelper.GetAsync<Page<T>>(company, url.ToString(), Tokens);
            return ExtractItemsFromPage(resp);
        }

        /// <summary>
        /// GET a resource that is owned by a particular company.
        /// </summary>
        public Task<T> GetAsync<T>(Url url, Company company)
        {
            return HttpHelper.GetAsync<T>(company, url.ToString(), Tokens);
        }

        /// <summary>
        /// GET a resource that is not specific to a particular company (e.g., list /building/types/*, /companies, etc.).
        /// </summary>
        public Task<T> GetAsync<T>(Url url)
        {
            return HttpHelper.GetAsync<T>(url.ToString(), Tokens.Get());
        }

        /// <summary>
        /// GET all pages of a paged resource.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url"></param>
        /// <param name="company"></param>
        /// <returns></returns>
        public async Task<IEnumerable<T>> GetAllAsync<T>(Url url, Company company)
        {
            var resp = await HttpHelper.GetAsync<Page<T>>(company, url.ToString(), Tokens);
            var all = ExtractItemsFromPage(resp).ToList();
            while (resp != null && resp.Next != null && resp.Next.Href != null)
            {
                resp = await HttpHelper.GetAsync<Page<T>>(company, resp.Next.Href, Tokens);
                all.AddRange(ExtractItemsFromPage(resp));
            }
            return all;
        }

        public Task<T> PostAsync<T>(Url url, Company company, T payload, string tokenScope = "panoptix.write")
        {
            return HttpHelper.PostAsync(url.ToString(), Tokens.Get(company, tokenScope), payload);
        }

        /// <summary>
        /// GET the list of all companies that you have visibility to.
        /// </summary>
        public Task<List<Company>> GetCompaniesAsync()
        {
            var token = Tokens.Get();
            return HttpHelper.GetAsync<List<Company>>(BaseUrl.AppendPathSegment("companies").ToString(), token);
        }

        private static IEnumerable<T> ExtractItemsFromPage<T>(Page<T> resp)
        {
            return (resp == null || resp.Items == null) ? new List<T>() : resp.Items;
        }

        public ApiClient(ITokenProvider tokenProvider, string baseUrl)
        {
            this.tokens = tokenProvider;
            this.baseUrl = baseUrl;
        }

        private readonly string baseUrl;
        private readonly ITokenProvider tokens;
        private static readonly ILog Log = LogProvider.GetCurrentClassLogger();
    }
}
