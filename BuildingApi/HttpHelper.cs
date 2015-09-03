using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Common.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace BuildingApi
{
    [Obsolete("Use Flurl.Http package instead. This class will be removed in a future version.")]
    public static class HttpHelper
    {
        [Obsolete("Use async methods")]
        public static T Get<T>(Company company, string url, ITokenProvider tokens)
        {
            return Get<T>(url, tokens.Get(company));
        }

        [Obsolete("Use async methods")]
        public static T Get<T>(string url, Token token)
        {
            var response = Get(url, token);
            return ParseResponse<T>(url, token, response, "GET");
        }

        [Obsolete("Use async methods")]
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

        [Obsolete("Use async methods")]
        public static HttpResponseMessage Get(string url, Token token)
        {
            using (var client = CreateHttpClient(token))
            {
                var sw = new Stopwatch();
                sw.Start();
                var result = client.GetAsync(url).Result;
                Log.DebugFormat("Result={0} Action=GET Url='{1}' TimeTakenMs={2}", (Int32)result.StatusCode, url, sw.ElapsedMilliseconds);
                return result;
            }
        }

        [Obsolete("Use async methods")]
        public static T Post<T>(string url, Token token, T payload)
        {
            var resp = Post(url, token, payload as Object);
            return ParseResponse<T>(url, token, resp, "POST");
        }

        [Obsolete("Use async methods")]
        public static HttpResponseMessage Post(string url, Token token, Object payload)
        {
            var content = new StringContent(SerializeObject(payload), System.Text.Encoding.UTF8, "application/json");

            using (var client = CreateHttpClient(token))
            {
                var sw = new Stopwatch();
                sw.Start();
                var result = client.PostAsync(url, content).Result;
                Log.DebugFormat("Result={0} Action=POST Url='{1}' TimeTakenMs={2}", (Int32)result.StatusCode, url, sw.ElapsedMilliseconds);
                return result;
            }
        }

        public static HttpResponseMessage Delete(string url, Token token)
        {
            using (var client = CreateHttpClient(token))
            {
                var sw = new Stopwatch();
                sw.Start();
                var result = client.DeleteAsync(url).Result;
                Log.DebugFormat("Result={0} Action=DELETE Url='{1}' TimeTakenMs={2}", (Int32)result.StatusCode, url, sw.ElapsedMilliseconds);
                return result;
            }
        }

        public static Task<T> GetAsync<T>(Company company, string url, ITokenProvider tokens)
        {
            return GetAsync<T>(url, tokens.Get(company));
        }

        public async static Task<T> GetAsync<T>(string url, Token token)
        {
            var response = GetAsync(url, token);
            return await ParseResponseAsync<T>(url, token, await response, "GET");
        }

        private async static Task<T> ParseResponseAsync<T>(string url, Token token, HttpResponseMessage response, string httpAction)
        {
            if (response.IsSuccessStatusCode)
            {
                return DeserializeObject<T>(await response.Content.ReadAsStringAsync());
            }
            var errorDescription = await response.Content.ReadAsStringAsync();
            var message = string.Format("Unsuccessful {0}: {1}{2}\tResponse: {3} {4} {5} {6}", httpAction, url, Environment.NewLine, response.StatusCode,
                response.StatusCode, response.ReasonPhrase, errorDescription);

            if (response.StatusCode == HttpStatusCode.InternalServerError)
            {
                message += "\r\n\tReason: ";
                try
                {
                    message += errorDescription;
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

        public async static Task<HttpResponseMessage> GetAsync(string url, Token token)
        {
            using (var client = CreateHttpClient(token))
            {
                var sw = new Stopwatch();
                sw.Start();
                var result = await client.GetAsync(url);
                Log.DebugFormat("Result={0} Action=GET Url='{1}' TimeTakenMs={2}", (Int32)result.StatusCode, url, sw.ElapsedMilliseconds);
                return result;
            }
        }

        public async static Task<T> PostAsync<T>(string url, Token token, T payload)
        {
            var resp = PostAsync(url, token, payload as Object);
            return await ParseResponseAsync<T>(url, token, resp, "POST");
        }

        public static HttpResponseMessage PostAsync(string url, Token token, Object payload)
        {
            var content = new StringContent(SerializeObject(payload), System.Text.Encoding.UTF8, "application/json");

            using (var client = CreateHttpClient(token))
            {
                var sw = new Stopwatch();
                sw.Start();
                var result = client.PostAsync(url, content).Result;
                Log.DebugFormat("Result={0} Action=POST Url='{1}' TimeTakenMs={2}", (Int32)result.StatusCode, url, sw.ElapsedMilliseconds);
                return result;
            }
        }

        public async static Task<HttpResponseMessage> DeleteAsync(string url, Token token)
        {
            using (var client = CreateHttpClient(token))
            {
                var sw = new Stopwatch();
                sw.Start();
                var result = await client.DeleteAsync(url);
                Log.DebugFormat("Result={0} Action=DELETE Url='{1}' TimeTakenMs={2}", (Int32)result.StatusCode, url, sw.ElapsedMilliseconds);
                return result;
            }
        }

        private static HttpClient CreateHttpClient(Token token)
        {
            // TODO: Some indications that HttpClient is thread safe (on methods we use) and should be re-used:
            // http://stackoverflow.com/questions/11178220/is-httpclient-safe-to-use-concurrently
            var handler = new HttpClientHandler
            {
                Proxy = token.Proxy
            };

            if (handler.SupportsAutomaticDecompression)
            {
                handler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            }

            var client = new HttpClient(handler);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);

            return client;
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
