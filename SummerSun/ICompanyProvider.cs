using System.Collections.Generic;

namespace BuildingApi
{
    public interface ICompanyProvider
    {
        IEnumerable<Company> Get(string companyId = null);
    }
}