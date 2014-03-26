using System.Collections.Generic;

namespace BuildingApi
{
    public class CompanyClient : ICompanyProvider
    {
        private readonly string endpoint;
        private readonly ITokenProvider tokenProvider;

        public CompanyClient(string endpoint, ITokenProvider tokenProvider)
        {
            this.endpoint = endpoint;
            this.tokenProvider = tokenProvider;

        }

        public IEnumerable<Company> Get(string companyId = null)
        {
            var token = tokenProvider.Get();
            return HttpHelper.Get<Company[]>(endpoint, token);
        }
    }
}