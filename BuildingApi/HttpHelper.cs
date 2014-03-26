using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Common.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace BuildingApi
{
    public static class HttpHelper
    {

        public static T Get<T>(Company company, string url, ITokenProvider tokens)
        {
            return Get<T>(url, tokens.Get(company));
        }

        public static T Get<T>(string url, Token token)
        {
            var response = Get(url, token);
            if (response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<T>(response.Content.ReadAsStringAsync().Result, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver(), NullValueHandling = NullValueHandling.Ignore });
            }
            var message = string.Format("Unsuccessful GET: {0}{1}\tResponse: {2} {3} {4}", url, Environment.NewLine, response.StatusCode, response.StatusCode, response.ReasonPhrase);

            if (response.StatusCode == HttpStatusCode.InternalServerError)
            {
                message += "\r\n\tReason: ";
                try
                {
                    message += response.Content.ReadAsStringAsync().Result;
                }
                catch (Exception)
                {
                    message += " (Failed to parse the Internal Server Error message.)";
                }
            }

            message += "\nToken: " + token.AccessToken;

            Log.Warn(message);
            throw new HttpRequestException(message);
        }

        public static HttpResponseMessage Get(string url, Token token)
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

                return client.GetAsync(url).Result;
            }
        }

        private static readonly ILog Log = LogManager.GetCurrentClassLogger();
    }
}
