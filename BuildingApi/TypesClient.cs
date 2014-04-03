using System;
using System.Collections.Generic;
using System.Globalization;
using Flurl;


namespace BuildingApi
{
    public class TypesClient
    {
        private readonly string _apiBaseUrl;
        private readonly ITokenProvider _tokenProvider;

        public TypesClient(ITokenProvider tokenProvider, string buildingApiUrl)
        {
            this._tokenProvider = tokenProvider;
            this._apiBaseUrl = buildingApiUrl.AppendPathSegment("building").ToString();
        }

        public IEnumerable<EquipmentType> GetEquipmentTypes(Company c)
        {
            var token = _tokenProvider.Get(c);

            var url = _apiBaseUrl.AppendPathSegment("types/Equipment");
            var resp = HttpHelper.Get<Page<EquipmentType>>(url, token);
            return (resp == null || resp.Items == null) ? new List<EquipmentType>() : resp.Items;
        }
    }
}
