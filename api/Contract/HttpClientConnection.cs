using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Contract
{
    public static class HttpTransfer
    {
        public static HttpClient HttpClientAsync(this HttpClient httpClient, string Token = "")
        {
            httpClient = new HttpClient();
            if (!string.IsNullOrEmpty(Token))
                httpClient.DefaultRequestHeaders.Add("authorization", "bearer " + Token);
            return httpClient;
        }

        public static async Task<List<HttpObject.APIresult>> GetAPIAsync(
            this HttpClient httpClient,
            [Required] string URL
        )
        {
            HttpResponseMessage httpResponseMessage = new HttpResponseMessage();
            httpResponseMessage = await httpClient.GetAsync(URL);

            return await returnListAPI(httpResponseMessage);
        }

        public static async Task<HttpObject.APIresult> GetAPIObjectAsync(
            this HttpClient httpClient,
            [Required] string URL
        )
        {
            HttpResponseMessage httpResponseMessage = new HttpResponseMessage();
            httpResponseMessage = await httpClient.GetAsync(URL);
            return await returnAPI(httpResponseMessage);
        }

        public static async Task<List<HttpObject.APIresult>> PostAPIAsync(
            this HttpClient httpClient,
            [Required] string URL,
            HttpContent httpContent
        )
        {
            HttpResponseMessage httpResponseMessage = new HttpResponseMessage();
            httpResponseMessage = await httpClient.PostAsync(URL, httpContent);
            return await returnListAPI(httpResponseMessage);
        }

        public static async Task<List<HttpObject.APIresult>> PutAPIAsync(
            this HttpClient httpClient,
            [Required] string URL,
            HttpContent httpContent
        )
        {
            HttpResponseMessage httpResponseMessage = new HttpResponseMessage();
            httpResponseMessage = await httpClient.PutAsync(URL, httpContent);
            return await returnListAPI(httpResponseMessage);
        }

        public static async Task<HttpObject.APIresult> DeleteAPIObjectAsync(
            this HttpClient httpClient,
            [Required] string URL
        )
        {
            HttpResponseMessage httpResponseMessage = new HttpResponseMessage();
            httpResponseMessage = await httpClient.DeleteAsync(URL);
            return await returnAPI(httpResponseMessage);
        }

        public static async Task<List<HttpObject.APIresult>> DeleteAPIAsync(
            this HttpClient httpClient,
            [Required] string URL
        )
        {
            HttpResponseMessage httpResponseMessage = new HttpResponseMessage();
            httpResponseMessage = await httpClient.DeleteAsync(URL);
            return await returnListAPI(httpResponseMessage);
        }

        private static async Task<List<HttpObject.APIresult>> returnListAPI(
            HttpResponseMessage httpResponseMessage
        )
        {
            List<HttpObject.APIresult> TGen = new List<HttpObject.APIresult>();
            if (httpResponseMessage.IsSuccessStatusCode == true)
            {
                TGen.Add(
                    new HttpObject.APIresult
                    {
                        code = HttpObject.Enums.Httpstatuscode_API.OK,
                        Data = await httpResponseMessage.Content.ReadAsStringAsync() ?? null,
                        Messenger = "Success! "
                    }
                );
            }
            else
            {
                TGen.Add(
                    new HttpObject.APIresult
                    {
                        code = HttpObject.Enums.Httpstatuscode_API.WARN,
                        Data = await httpResponseMessage.Content.ReadAsStringAsync() ?? null,
                        Messenger = "An error occurred!"
                    }
                );
            }
            return TGen;
        }

        private static async Task<HttpObject.APIresult> returnAPI(
            HttpResponseMessage httpResponseMessage
        )
        {
            HttpObject.APIresult TGen = new HttpObject.APIresult();

            if (httpResponseMessage.IsSuccessStatusCode == true)
            {
                TGen = new HttpObject.APIresult
                {
                    code = HttpObject.Enums.Httpstatuscode_API.OK,
                    Data = await httpResponseMessage.Content.ReadAsStringAsync() ?? null,
                    Messenger = "Success! "
                };
            }
            else
            {
                TGen = new HttpObject.APIresult
                {
                    code = HttpObject.Enums.Httpstatuscode_API.WARN,
                    Data = await httpResponseMessage.Content.ReadAsStringAsync() ?? null,
                    Messenger = "An error occurred!"
                };
            }
            return TGen;
        }
    }

    public class HttpObject
    {
        public class Enums
        {
            public enum Httpstatuscode_API
            {
                OK = 200,
                ERROR = 401,
                NULL = 402,
                WARN = 403,
            }
        }

        public class APIresult
        {
            public Enums.Httpstatuscode_API code { get; set; } = Enums.Httpstatuscode_API.OK;
            public dynamic Data { get; set; }
            public string Messenger { get; set; } = "Success!";
        }

        public class DataObject { }
    }
}
