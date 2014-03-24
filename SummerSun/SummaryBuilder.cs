using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Common.Logging;
using Flurl;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using SummerSun.Api;

namespace SummerSun
{
    public class SummaryBuilder
    {
        public IEnumerable<Equipment> GetEquipmentAndPointRoles(Guid customerId, string equipmentType, string pointRoleType, int offset = 0, int max = 250)
        {
            var url = apiBaseUrl.AppendPathSegment("equipment").SetQueryParams(new
            {
                type = equipmentType,
                _expand = "pointRoles",
                _offset = offset.ToString(CultureInfo.InvariantCulture),
                _limit = max.ToString(CultureInfo.InvariantCulture)
            }).ToString();
            var resp = HttpGet<Page<Equipment>>(customerId, url);
            return (resp == null || resp.Items == null) ? new List<Equipment>() : resp.Items;
        }

        public IEnumerable<Point> GetPoints(Guid customerGuid, IEnumerable<string> pointIds)
        {
            var url = apiBaseUrl.AppendPathSegment("points").SetQueryParams(new
            {
                id = string.Join(",", pointIds),
                _expand = "sampleSummary",
                _offset = "0"
            }).ToString();
            var resp = HttpGet<Page<Point>>(customerGuid, url);
            return (resp == null || resp.Items == null) ? new List<Point>() : resp.Items;            
        }

        public TResult HttpGet<TResult>(Guid customerGuid, string url)
        {
            var token = tokens.Get(customerGuid);
            TResult result = default(TResult);

            try
            {
                var handler = new HttpClientHandler
                {
                    Proxy = token.Proxy
                };

                if (handler.SupportsAutomaticDecompression)
                {
                    handler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                }

                using (var client = new HttpClient(handler))
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);

                    var response = client.GetAsync(url).Result;
                    if (response.StatusCode == HttpStatusCode.NotFound)
                    {
                        var message = string.Format("{0}: {1}", response.StatusCode, url);
                        Log.WarnFormat(message);
                        return result;
                    }

                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        // response.Content.ReadAsAsync blows up with bad data, JsonConvert.DeserializeObject is more stable with the same data
                        result = JsonConvert.DeserializeObject<TResult>(response.Content.ReadAsStringAsync().Result, new JsonSerializerSettings{ ContractResolver = new CamelCasePropertyNamesContractResolver(), NullValueHandling = NullValueHandling.Ignore});
                    }
                    else // TODO: check for expired token and auto-retry
                    {
                        LogAndThrowException("GET", response, url, token.AccessToken);
                    }
                }
            }
            catch (Exception e)
            {
                Log.ErrorFormat("{0}\n{1}", e.Message, e.StackTrace);
                throw;
            }

            return result;
        }

        private static void LogAndThrowException(string requestType, HttpResponseMessage response, string url, string token)
        {
            var message = string.Format("Unable to {0}: {1}\r\n\tError: {2} {3}", requestType, url, (int)response.StatusCode, response.ReasonPhrase);

            if (response.StatusCode == HttpStatusCode.InternalServerError)
            {
                message += "\r\n\tReason: ";
                try
                {
                    var errorEntity = response.Content.ReadAsStringAsync().Result;
                    message += errorEntity;
                }
                catch (Exception)
                {
                    message += "Failed to parse the Internal Server Error message.";
                }
            }

            message += "\nToken: " + token;

            Log.Error(message);
            throw new HttpRequestException(message);
        }

        public SummaryBuilder(Jci.Panoptix.Cda.Building.Storage.ITokenRepository tokenRepository, string buildingApiUrl)
        {
            this.tokens = tokenRepository;
            this.apiBaseUrl = buildingApiUrl;
        }

        private static readonly ILog Log = LogManager.GetCurrentClassLogger();
        private readonly string apiBaseUrl;
        private readonly Jci.Panoptix.Cda.Building.Storage.ITokenRepository tokens;
    }
}
