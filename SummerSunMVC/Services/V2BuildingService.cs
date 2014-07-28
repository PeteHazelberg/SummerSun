using BuildingApi;
using SummerSunMVC.App_Start;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;
using System.Diagnostics;
using NLog;
using System.Configuration;
using System.Net;
using Flurl;

namespace SummerSunMVC.Services
{
    public class V2BuildingService : IBuildingService
    {
        private const string K_COMPANIES_CACHE_KEY = "Companies";
        private const string K_EQUIPMENTTYPES_CACHE_KEY = "EquipmentTypes";
        private const int _cacheExpirationTimeInMinutes = 30;
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private ITokenProvider _tokenProvider = null;
        private readonly ApiClient _api = null;

        private Stopwatch _stopWatch =  new Stopwatch();

        public V2BuildingService()
        {
            string clientId = ConfigurationManager.AppSettings["JciClientId"];
            string clientSecret = ConfigurationManager.AppSettings["JciClientSecret"];
            string tokenEndpoint = ConfigurationManager.AppSettings["JciTokenEndpoint"];
            string buildingApiEndpoint = ConfigurationManager.AppSettings["JciBuildingApiEndpoint"];
            IWebProxy proxy = WebProxy.GetDefaultProxy();

            _tokenProvider = new ClientCredentialsTokenClient(clientId, clientSecret, tokenEndpoint, proxy);
            _api = new ApiClient(_tokenProvider, buildingApiEndpoint);
        }

        public string APIBaseUrl { get { return _api.BaseUrl; } }

        public IEnumerable<Company> GetCompanies()
        { 
            // No need to get back to Px everytime
            var companies = HttpRuntime.Cache.Get(K_COMPANIES_CACHE_KEY) as IEnumerable<Company>;
            if (companies == null)
            {
                var token = _tokenProvider.Get();
                companies = HttpHelper.Get<Company[]>(_api.BaseUrl.AppendPathSegment("companies").ToString(), token);

                HttpRuntime.Cache.Insert(K_COMPANIES_CACHE_KEY, companies, null, DateTime.UtcNow.AddMinutes(_cacheExpirationTimeInMinutes), Cache.NoSlidingExpiration);
            }

            return companies;
        }

        public string GetAccessToken(string companyId)
        {
            var company = GetCompanies().FirstOrDefault(c => c.Id == companyId);
            if (company == null)
                return string.Empty;
            else
                return GetAccessToken(company);
        }

        public string GetAccessToken(Company c)
        {
            return _tokenProvider.Get(c).AccessToken;
        }

        public IEnumerable<EquipmentType> GetEquipmentTypes()
        {
            var types = HttpRuntime.Cache.Get(K_EQUIPMENTTYPES_CACHE_KEY) as IEnumerable<EquipmentType>;
            if (types == null)
            {
                // The Types API require a customer context. A token requested through the client_credential grant_type
                // is not enough. 
                // Let's pick the first in the list...
                Company c = GetCompanies().FirstOrDefault();
                _stopWatch.Restart();
                var url = _api.BaseUrl.AppendPathSegment("building/types/Equipment");
                var resp = HttpHelper.Get<Page<EquipmentType>>(url, _tokenProvider.Get(c));
                 types = (resp == null || resp.Items == null) ? new List<EquipmentType>() : resp.Items;

                _stopWatch.Stop();
                _logger.Debug(string.Format("GetEquipmentTypes executed in {0} ms",  _stopWatch.ElapsedMilliseconds));

                HttpRuntime.Cache.Insert(K_EQUIPMENTTYPES_CACHE_KEY, types, null, DateTime.UtcNow.AddMinutes(_cacheExpirationTimeInMinutes), Cache.NoSlidingExpiration);
            }
            return types;
        }

        public IEnumerable<Equipment> GetEquipmentByCompany(string equipmentTypeSearch, Company company)
        {
            // TO DO
            // Cache locally ?
            _stopWatch.Restart();
            var url = _api.BaseUrl.AppendPathSegment("building/equipment").SetQueryParams(new
            {
                type = equipmentTypeSearch,
                _expand = "pointRoles"
            });
            var equipList = _api.GetPage<Equipment>(url, company);
            _stopWatch.Stop();
            _logger.Debug(string.Format("GetEquipmentAndPointRoles -> {0} found {1} '{2}' in {3} ms", company.Name, equipList.Count(), equipmentTypeSearch, _stopWatch.ElapsedMilliseconds));
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
            foreach (var pointIdList in requests)
                points.AddRange(GetPointsAndSummary(pointIdList, c));
            
            _stopWatch.Stop();
            _logger.Debug(string.Format("GetPointsSummary -> {0} found {1} points with {2} requests in {3} ms", c.Name, points.Count(), requests.Count, _stopWatch.ElapsedMilliseconds));
            return points;
        }

        private IEnumerable<Point> GetPointsAndSummary(IEnumerable<string> pointIds, Company company)
        {
            var url = _api.BaseUrl.AppendPathSegment("building/points").SetQueryParams(new
            {
                id = string.Join(",", pointIds),
                _expand = "sampleSummary",
                _fields = "sampleSummary.updated",
                _offset = "0"
            });
            return _api.GetPage<Point>(url, company);
        }
    }
}