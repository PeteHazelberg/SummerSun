using System;
using System.Diagnostics;
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
            var errorDescription = response.Content.ReadAsStringAsync().Result;
            var message = string.Format("Unsuccessful GET: {0}{1}\tResponse: {2} {3} {4} {5}", url, Environment.NewLine, response.StatusCode, response.StatusCode, response.ReasonPhrase, errorDescription);

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

                var sw = new Stopwatch();
                sw.Start();
                var result =  client.GetAsync(url).Result;
                Log.DebugFormat("Result={0} Action='GET' Url='{1}' TimeTakenMs='{2}'", (Int32)result.StatusCode, url, sw.ElapsedMilliseconds);
                return result;
            }
        }

        public static HttpResponseMessage Post(string url, Token token, Object payload)
        {
            var content = new StringContent(JsonConvert.SerializeObject(payload));

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

                var sw = new Stopwatch();
                sw.Start();
                var result = client.PostAsync(url, content).Result;
                Log.DebugFormat("Result={0} Action='POST' Url='{1}' TimeTakenMs='{2}'", (Int32)result.StatusCode, url, sw.ElapsedMilliseconds);
                return result;
            }
        }

        private static readonly ILog Log = LogManager.GetCurrentClassLogger();
    }
}
