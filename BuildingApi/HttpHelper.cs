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
            return ParseResponse<T>(url, token, response, "GET");
        }

        private static T ParseResponse<T>(string url, Token token, HttpResponseMessage response, string httpAction)
        {
            if (response.IsSuccessStatusCode)
            {
                return DeserializeObject<T>(response.Content.ReadAsStringAsync().Result);
            }
            var errorDescription = response.Content.ReadAsStringAsync().Result;
            var message = string.Format("Unsuccessful {0}: {1}{2}\tResponse: {3} {4} {5} {6}", httpAction, url, Environment.NewLine, response.StatusCode,
                response.StatusCode, response.ReasonPhrase, errorDescription);

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
                Log.DebugFormat("Result={0} Action=GET Url='{1}' TimeTakenMs={2}", (Int32)result.StatusCode, url, sw.ElapsedMilliseconds);
                return result;
            }
        }


        public static T Post<T>(string url, Token token, T payload)
        {
            var resp = Post(url, token, payload as Object);
            return ParseResponse<T>(url, token, resp, "POST");
        }

        public static HttpResponseMessage Post(string url, Token token, Object payload)
        {
            var content = new StringContent(SerializeObject(payload), System.Text.Encoding.UTF8, "application/json");

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
                Log.DebugFormat("Result={0} Action=POST Url='{1}' TimeTakenMs={2}", (Int32)result.StatusCode, url, sw.ElapsedMilliseconds);
                return result;
            }
        }

        public static HttpResponseMessage Delete(string url, Token token)
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
                var result = client.DeleteAsync(url).Result;
                Log.DebugFormat("Result={0} Action=DELETE Url='{1}' TimeTakenMs={2}", (Int32)result.StatusCode, url, sw.ElapsedMilliseconds);
                return result;
            }
        }

        public static T DeserializeObject<T>(string payloadText)
        {
            return JsonConvert.DeserializeObject<T>(payloadText,
                new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver(),
                    NullValueHandling = NullValueHandling.Ignore
                });
        }

        public static string SerializeObject(object payload)
        {
            return JsonConvert.SerializeObject(payload, new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                NullValueHandling = NullValueHandling.Ignore
            });
        }

        private static readonly ILog Log = LogManager.GetCurrentClassLogger();
    }
}
