using BuildingApi;
using SummerSunMVC.App_Start;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;
using log4net;
using System.Diagnostics;

namespace SummerSunMVC.Services
{
    public class V2BuildingService : IBuildingService
    {
        private const string K_COMPANIES_CACHE_KEY = "Companies";
        private const string K_EQUIPMENTTYPES_CACHE_KEY = "EquipmentTypes";
        private const int _cacheExpirationTimeInMinutes = 30;
        private static readonly ILog _logger = LogManager.GetLogger(typeof(V2BuildingService));
        private Stopwatch _stopWatch =  new Stopwatch();


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
                _stopWatch.Restart();
                types = BuildingAPIClient.TypesClient.GetEquipmentTypes(c);
                _stopWatch.Stop();
                _logger.Debug(string.Format("GetEquipmentTypes executed in {0} ms",  _stopWatch.ElapsedMilliseconds));
                HttpRuntime.Cache.Insert(K_EQUIPMENTTYPES_CACHE_KEY, types, null, DateTime.UtcNow.AddMinutes(_cacheExpirationTimeInMinutes), Cache.NoSlidingExpiration);
            }
            return types;
        }

        public IEnumerable<Equipment> GetEquipmentByCompany(string equipmentType, Company company)
        {
            // TO DO
            // Cache locally ?
            _stopWatch.Restart();
            var equipList = BuildingAPIClient.EquipmentClient.GetEquipmentAndPointRoles(equipmentType, company);
            _stopWatch.Stop();
            _logger.Debug(string.Format("GetEquipmentAndPointRoles -> {0} found {1} equipment in {2} ms", company.Name, equipList.Count(), _stopWatch.ElapsedMilliseconds));
            return equipList;
        }

        public IEnumerable<Point> GetPointsSummary(IEnumerable<string> ids, Company c)
        {
            _stopWatch.Restart();
            var points = new List<Point>();
            var requests = new List<IEnumerable<string>>();
            // Limit of 50 ids per requests.
            while (ids.Any())
            {
                requests.Add(ids.Take(50).ToList());
                ids = ids.Skip(50).ToList();
            }
            foreach (var item in requests)
                points.AddRange(BuildingAPIClient.EquipmentClient.GetPointsAndSummary(item, c));
            
            _stopWatch.Stop();
            _logger.Debug(string.Format("GetPointsSummary -> {0} found {1} points with {2} requests in {3} ms", c.Name, points.Count(), requests.Count, _stopWatch.ElapsedMilliseconds));
            return points;
        }

    }
}