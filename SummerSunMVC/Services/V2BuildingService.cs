using BuildingApi;
using SummerSunMVC.App_Start;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;

namespace SummerSunMVC.Services
{
    public class V2BuildingService : IBuildingService
    {
        private const string K_COMPANIES_CACHE_KEY = "Companies";
        private const string K_EQUIPMENTTYPES_CACHE_KEY = "EquipmentTypes";
        private const int _cacheExpirationTimeInMinutes = 30;

        public IEnumerable<Company> GetCompanies()
        { 
            // No need to get back to Px everytime
            var companies = HttpRuntime.Cache.Get(K_COMPANIES_CACHE_KEY) as IEnumerable<Company>;
            if (companies == null)
            {
                ICompanyProvider client = BuildingAPIClient.CompanyProvider;
                companies = client.Get();
                HttpRuntime.Cache.Insert(K_COMPANIES_CACHE_KEY, companies, null, DateTime.UtcNow.AddMinutes(_cacheExpirationTimeInMinutes), Cache.NoSlidingExpiration);
            }
            return companies;
        }

        public IEnumerable<EquipmentType> GetEquipmentTypes()
        {
            var types = HttpRuntime.Cache.Get(K_EQUIPMENTTYPES_CACHE_KEY) as IEnumerable<EquipmentType>;
            if (types == null)
            {
                // The Types API require a customer context. A token requested through the client_credential grant_type
                // is not enough. It needs to be fixed on the API side
                // Need an extra call to get a security token with a customer context
                // Let's pick the first in the list...
                Company c = GetCompanies().FirstOrDefault();
                types = BuildingAPIClient.TypesClient.GetEquipmentTypes(c);
                HttpRuntime.Cache.Insert(K_EQUIPMENTTYPES_CACHE_KEY, types, null, DateTime.UtcNow.AddMinutes(_cacheExpirationTimeInMinutes), Cache.NoSlidingExpiration);
            }
            return types;
        }

        public IEnumerable<Equipment> GetEquipmentByCompany(string equipmentType, Company company)
        {
            // TO DO
            // Cache locally ?
            return BuildingAPIClient.EquipmentClient.GetEquipmentAndPointRoles(equipmentType, company);
        }

        public IEnumerable<Point> GetPointsSummary(IEnumerable<string> ids, Company c)
        {

            return BuildingAPIClient.EquipmentClient.GetPointsAndSummary(ids, c);
        }

    }
}