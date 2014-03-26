using System;
using System.Collections.Generic;
using System.Globalization;
using Flurl;

namespace BuildingApi
{
    public class EquipmentClient
    {
        public IEnumerable<Equipment> GetEquipmentAndPointRoles(string equipmentType, Company company, int offset = 0, int max = 250)
        {
            var url = apiBaseUrl.AppendPathSegment("equipment").SetQueryParams(new
            {
                type = equipmentType,
                _expand = "pointRoles",
                _offset = offset.ToString(CultureInfo.InvariantCulture),
                _limit = max.ToString(CultureInfo.InvariantCulture)
            }).ToString();
            var resp = HttpHelper.Get<Page<Equipment>>(company, url, tokens);
            return (resp == null || resp.Items == null) ? new List<Equipment>() : resp.Items;
        }

        public IEnumerable<Point> GetPointsAndSummary(IEnumerable<string> pointIds, Company company)
        {
            var url = apiBaseUrl.AppendPathSegment("points").SetQueryParams(new
            {
                id = string.Join(",", pointIds),
                _expand = "sampleSummary",
                _fields = "sampleSummary.updated",
                _offset = "0"
            }).ToString();
            var resp = HttpHelper.Get<Page<Point>>(company, url, tokens);
            return (resp == null || resp.Items == null) ? new List<Point>() : resp.Items;            
        }

        public EquipmentClient(ITokenProvider tokenProvider, string buildingApiUrl)
        {
            this.tokens = tokenProvider;
            this.apiBaseUrl = buildingApiUrl;
        }

        private readonly string apiBaseUrl;
        private readonly ITokenProvider tokens;
    }
}
