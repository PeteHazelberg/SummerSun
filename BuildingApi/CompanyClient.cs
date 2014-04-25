using System.Collections.Generic;
using Flurl;

namespace BuildingApi
{
    public class CompanyClient : ICompanyProvider
    {
        private readonly string endpoint;
        private readonly ITokenProvider tokenProvider;

        public CompanyClient(ITokenProvider tokenProvider,string endpoint)
        {
            this.endpoint = endpoint;
            this.tokenProvider = tokenProvider;

        }

        public IEnumerable<Company> Get(string companyId = null)
        {
            var token = tokenProvider.Get();
            return HttpHelper.Get<Company[]>(endpoint.AppendPathSegment("companies").ToString(), token);
        }
    }
}