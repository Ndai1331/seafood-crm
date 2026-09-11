using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using BootstrapBlazor.Server.Exceptions;
using BootstrapBlazor.Server.Services;

namespace BootstrapBlazor.Server.Http
{
    public static class RequestClient
    {
        private readonly static HttpClient _client;

        /// <summary>
        /// Serialize with camelCase to match ASP.NET API default binding
        /// </summary>
        private static readonly JsonSerializerSettings _jsonSettings = new()
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            NullValueHandling = NullValueHandling.Ignore
        };

        private static readonly JsonSerializerSettings _jsonSettingsIncludingNulls = new()
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            NullValueHandling = NullValueHandling.Include
        };

        //intergated service
        public static IConfiguration Config;
        private static CancellationTokenSource _tokenSource;
        private static readonly AsyncLocal<ILocalStorageService?> _localStorage = new();
        private static readonly AsyncLocal<RequestClientUnauthorizedNotifier?> _unauthorizedNotifier = new();
        private static readonly AsyncLocal<RequestClientCircuitGate?> _circuitGate = new();
        private static readonly AsyncLocal<string?> _accessToken = new();
        private static readonly AsyncLocal<string?> _clientUserAgent = new();
        private static long UploadLimit = 25214400;
        static RequestClient()
        {
            _client = new HttpClient();
            _tokenSource = new CancellationTokenSource();
        }

        public static void CancelToken()
        {
            //ko được sử dụng cancel ở đây exception IO
        }

        public static void Initialize(IConfiguration configuration)
        {
            Config = configuration;
            _client.BaseAddress = new Uri(Config["RemoteServices:BaseUrl"]);
        }

        public static void InjectServices(ILocalStorageService localStorage)
        {
            _localStorage.Value = localStorage;
        }

        internal static async Task RunWithServicesAsync(
            ILocalStorageService localStorage,
            RequestClientUnauthorizedNotifier unauthorizedNotifier,
            RequestClientCircuitGate circuitGate,
            Func<Task> action)
        {
            var previousLocalStorage = _localStorage.Value;
            var previousNotifier = _unauthorizedNotifier.Value;
            var previousCircuitGate = _circuitGate.Value;
            _localStorage.Value = localStorage;
            _unauthorizedNotifier.Value = unauthorizedNotifier;
            _circuitGate.Value = circuitGate;
            try
            {
                await action();
            }
            finally
            {
                _localStorage.Value = previousLocalStorage;
                _unauthorizedNotifier.Value = previousNotifier;
                _circuitGate.Value = previousCircuitGate;
            }
        }

        internal static async Task RunTokenMutationAsync(Func<Task> action)
        {
            var semaphore = _circuitGate.Value?.Semaphore;
            if (semaphore != null)
            {
                await semaphore.WaitAsync();
            }

            try
            {
                await action();
            }
            finally
            {
                semaphore?.Release();
            }
        }

        public static void AttachToken(string Token = "")
        {
            if (!string.IsNullOrEmpty(Token))
            {
                _accessToken.Value = Token;
                System.Console.WriteLine($"[AuthFlow] RequestClient.AttachToken {DescribeToken(Token)}");
            }
        }

        public static void SetClientUserAgent(string? userAgent)
        {
            _clientUserAgent.Value = string.IsNullOrWhiteSpace(userAgent) ? null : userAgent.Trim();
        }

        public static string DescribeToken(string? token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return "token=missing";
            }

            var prefix = token.Length <= 12 ? token : token[..12];
            var suffix = token.Length <= 8 ? token : token[^8..];
            var parts = new List<string> { $"token=len:{token.Length}", $"mask:{prefix}...{suffix}" };

            try
            {
                var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
                var userId = jwt.Claims.FirstOrDefault(x => x.Type == ClaimTypes.PrimarySid)?.Value;
                var role = jwt.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Role)?.Value;
                var exp = jwt.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Exp || x.Type == "exp")?.Value;
                if (!string.IsNullOrWhiteSpace(userId))
                {
                    parts.Add($"userId:{userId}");
                }

                if (!string.IsNullOrWhiteSpace(role))
                {
                    parts.Add($"role:{role}");
                }

                if (long.TryParse(exp, out var epochTime))
                {
                    var expiresAtLocal = DateTimeOffset.FromUnixTimeSeconds(epochTime).ToLocalTime();
                    parts.Add($"exp:{expiresAtLocal:yyyy-MM-dd HH:mm:ss zzz}");
                }
            }
            catch (Exception ex)
            {
                parts.Add($"parse:{ex.GetType().Name}");
            }

            return string.Join(" ", parts);
        }


        public static async Task<T> GetAPIAsync<T>([Required] string URL)
        {
            try
            {
                HttpResponseMessage httpResponseMessage = new HttpResponseMessage();
                using var request = await CreateRequestMessageAsync(HttpMethod.Get, URL);
                httpResponseMessage = await _client.SendAsync(request, _tokenSource.Token);
                if (!httpResponseMessage.IsSuccessStatusCode)
                {
                    System.Console.WriteLine($"[RequestClient] GET {URL} returned {(int)httpResponseMessage.StatusCode} {httpResponseMessage.StatusCode}");
                }
                return await ReturnApiResponse<T>(httpResponseMessage);
            }
            catch (Exception ex)
            {
                var message = string.IsNullOrWhiteSpace(ex.Message) ? ex.GetType().Name : ex.Message;
                System.Console.WriteLine($"[RequestClient] GET {URL} failed: {message}");
                return await ReturnApiResponse<T>(null);
            }
        }

        public static async Task<T> PostAPIAsync<T>([Required] string URL, dynamic input, bool notifyOk = true)
        {
            HttpResponseMessage httpResponseMessage = new HttpResponseMessage();
            StringContent content =
                new StringContent(JsonConvert.SerializeObject(input, _jsonSettings), Encoding.UTF8, "application/json");

            using var request = await CreateRequestMessageAsync(HttpMethod.Post, URL, content);
            httpResponseMessage = await _client.SendAsync(request, _tokenSource.Token);


            return await ReturnApiResponse<T>(httpResponseMessage);
        }

        public static async Task<T> PostAPIAsync<T>([Required] string URL, dynamic input, TimeSpan timeout, bool notifyOk = true)
        {
            using var client = new HttpClient
            {
                BaseAddress = _client.BaseAddress,
                Timeout = timeout
            };
            StringContent content =
                new StringContent(JsonConvert.SerializeObject(input, _jsonSettings), Encoding.UTF8, "application/json");

            using var request = await CreateRequestMessageAsync(HttpMethod.Post, URL, content);
            var httpResponseMessage = await client.SendAsync(request, _tokenSource.Token);

            return await ReturnApiResponse<T>(httpResponseMessage);
        }

        public static async Task<byte[]> PostAPIBytesAsync([Required] string URL, dynamic input)
        {
            HttpResponseMessage httpResponseMessage = new HttpResponseMessage();
            StringContent content =
                new StringContent(JsonConvert.SerializeObject(input, _jsonSettings), Encoding.UTF8, "application/json");

            using var request = await CreateRequestMessageAsync(HttpMethod.Post, URL, content);
            httpResponseMessage = await _client.SendAsync(request, _tokenSource.Token);

            if (httpResponseMessage.IsSuccessStatusCode)
            {
                return await httpResponseMessage.Content.ReadAsByteArrayAsync();
            }

            if (httpResponseMessage.StatusCode == HttpStatusCode.Unauthorized)
            {
                var responseBody = await httpResponseMessage.Content.ReadAsStringAsync();
                System.Console.WriteLine($"[RequestClient] HTTP 401 Unauthorized. Body: {responseBody}");
                await HandleUnauthorizedAsync(httpResponseMessage);

                throw new UnauthorizedException(ApiErrorMessage.Describe(responseBody, "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại."));
            }

            if (httpResponseMessage.StatusCode == HttpStatusCode.InternalServerError)
            {
                var responseBody = await httpResponseMessage.Content.ReadAsStringAsync();
                System.Console.WriteLine($"[RequestClient] HTTP 500 InternalServerError. Body: {responseBody}");
                throw new ServerErrorException(ApiErrorMessage.Describe(responseBody, "Máy chủ đang gặp sự cố. Vui lòng thử lại sau."));
            }

            if (httpResponseMessage.StatusCode == HttpStatusCode.BadRequest)
            {
                var responseBody = await httpResponseMessage.Content.ReadAsStringAsync();
                throw new BadRequestException(ApiErrorMessage.Describe(responseBody, "Dữ liệu chưa hợp lệ. Vui lòng kiểm tra lại thông tin."));
            }

            throw new Exception($"API call failed with status code: {httpResponseMessage.StatusCode}");
        }

        public static async Task<T> PatchAPIAsync<T>([Required] string URL, dynamic input, bool notifyOk = true)
        {
            HttpResponseMessage httpResponseMessage = new HttpResponseMessage();


            StringContent content =
                new StringContent(JsonConvert.SerializeObject(input, _jsonSettings), Encoding.UTF8, "application/json");

            using var request = await CreateRequestMessageAsync(HttpMethod.Patch, URL, content);
            httpResponseMessage = await _client.SendAsync(request, _tokenSource.Token);
            return await ReturnApiResponse<T>(httpResponseMessage);
        }

        public static async Task<T> PostAPIWithFileAsync<T>([Required] string URL, IBrowserFile file)
        {
            HttpResponseMessage httpResponseMessage = new HttpResponseMessage();
            var streams = new List<MemoryStream>();

            using (var content = new MultipartFormDataContent())
            {
                var stream = file.OpenReadStream(UploadLimit);
                var streamContent = new StreamContent(stream);
                streamContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
                content.Add(streamContent, "file", file.Name);
                using var request = await CreateRequestMessageAsync(HttpMethod.Post, URL, content);
                httpResponseMessage = await _client.SendAsync(request, _tokenSource.Token);
                var response = await httpResponseMessage.Content.ReadAsStringAsync();
                foreach (var item in streams)
                {
                    item.Close();
                }
                return await ReturnApiResponse<T>(httpResponseMessage);
            }
        }

        public static async Task<T> PostAPIWithMultipleFileAsync<T>([Required] string URL, List<IBrowserFile> files)
        {
            HttpResponseMessage httpResponseMessage = new HttpResponseMessage();

            var streams = new List<MemoryStream>();

            using (var content = new MultipartFormDataContent())
            {
                foreach (var file in files)
                {
                    var ms = new MemoryStream();
                    await file.OpenReadStream(UploadLimit).CopyToAsync(ms);
                    ms.Seek(0, SeekOrigin.Begin);
                    content.Add(new StreamContent(ms), $"files", file.Name);
                    streams.Add(ms);

                }

                using var request = await CreateRequestMessageAsync(HttpMethod.Post, URL, content);
                httpResponseMessage = await _client.SendAsync(request, _tokenSource.Token);

                var response = await httpResponseMessage.Content.ReadAsStringAsync();

                foreach (var item in streams)
                {
                    item.Close();
                }

                return await ReturnApiResponse<T>(httpResponseMessage);
            }
        }


        public static async Task<T> PutAPIAsync<T>(
            [Required] string URL, dynamic input, bool includeNullValues = false)
        {
            HttpResponseMessage httpResponseMessage = new HttpResponseMessage();
            StringContent content =
                new StringContent(SerializeInput(input, includeNullValues), Encoding.UTF8, "application/json");
            using var request = await CreateRequestMessageAsync(HttpMethod.Put, URL, content);
            httpResponseMessage = await _client.SendAsync(request, _tokenSource.Token);
            return await ReturnApiResponse<T>(httpResponseMessage);
        }

        private static string SerializeInput(object input, bool includeNullValues = false) =>
            JsonConvert.SerializeObject(input, includeNullValues ? _jsonSettingsIncludingNulls : _jsonSettings);

        public static async Task<T> DeleteAPIAsync<T>([Required] string URL)
        {
            HttpResponseMessage httpResponseMessage = new HttpResponseMessage();
            using var request = await CreateRequestMessageAsync(HttpMethod.Delete, URL);
            httpResponseMessage = await _client.SendAsync(request, _tokenSource.Token);
            return await ReturnApiResponse<T>(httpResponseMessage);
        }


        private static SemaphoreSlim Coordinator = new SemaphoreSlim(initialCount: 1, 1);
        private static bool IsWorking = true;
        private static async Task<T> ReturnApiResponse<T>(HttpResponseMessage? httpResponseMessage)
        {
            if (httpResponseMessage == null)
            {
                return default(T);
            }

            if (httpResponseMessage.IsSuccessStatusCode)
            {
                string? jsonResponse = await httpResponseMessage.Content.ReadAsStringAsync() ?? null;
                return JsonConvert.DeserializeObject<T>(jsonResponse);
            }


            if (httpResponseMessage.StatusCode == HttpStatusCode.Unauthorized)
            {
                var responseBody = await httpResponseMessage.Content.ReadAsStringAsync();
                System.Console.WriteLine($"[RequestClient] HTTP 401 Unauthorized. Body: {responseBody}");
                await HandleUnauthorizedAsync(httpResponseMessage);

                throw new UnauthorizedException(ApiErrorMessage.Describe(responseBody, "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại."));
            }

            if (httpResponseMessage.StatusCode == HttpStatusCode.InternalServerError)
            {
                var responseBody = await httpResponseMessage.Content.ReadAsStringAsync();
                System.Console.WriteLine($"[RequestClient] HTTP 500 InternalServerError. Body: {responseBody}");
                throw new ServerErrorException(ApiErrorMessage.Describe(responseBody, "Máy chủ đang gặp sự cố. Vui lòng thử lại sau."));
            }

            if (httpResponseMessage.StatusCode == HttpStatusCode.TooManyRequests)
            {
                throw new TooManyRequests("Too many request");
            }

            if (httpResponseMessage.StatusCode == HttpStatusCode.Conflict)
            {
                string? jsonResponse = await httpResponseMessage.Content.ReadAsStringAsync() ?? null;
                throw new ConflictException(ApiErrorMessage.Describe(jsonResponse, "Dữ liệu đã tồn tại hoặc đang bị xung đột."));
            }

            if (httpResponseMessage.StatusCode == HttpStatusCode.BadGateway)
            {
                var responseBody = await httpResponseMessage.Content.ReadAsStringAsync();
                System.Console.WriteLine($"[RequestClient] HTTP 502 BadGateway. Body: {responseBody}");
                throw new DbConnectionException(ApiErrorMessage.Describe(responseBody, "Không thể kết nối đến máy chủ dữ liệu. Vui lòng thử lại sau."));
            }

            if (httpResponseMessage.StatusCode == HttpStatusCode.BadRequest)
            {
                string? jsonResponse = await httpResponseMessage.Content.ReadAsStringAsync() ?? null;
                System.Console.WriteLine($"[RequestClient] HTTP 400 BadRequest. Body: {jsonResponse}");
                throw new BadRequestException(ApiErrorMessage.Describe(jsonResponse, "Dữ liệu chưa hợp lệ. Vui lòng kiểm tra lại thông tin."));
            }

            var fallbackBody = await httpResponseMessage.Content.ReadAsStringAsync();
            System.Console.WriteLine($"[RequestClient] HTTP {(int)httpResponseMessage.StatusCode} {httpResponseMessage.StatusCode}. Body: {fallbackBody}");
            return default;
        }

        private static async Task HandleUnauthorizedAsync(HttpResponseMessage response)
        {
            var rejectedToken = response.RequestMessage?.Headers.Authorization?.Parameter;
            var localStorage = _localStorage.Value;
            if (localStorage == null || string.IsNullOrWhiteSpace(rejectedToken))
            {
                return;
            }

            var semaphore = _circuitGate.Value?.Semaphore;
            if (semaphore != null)
            {
                await semaphore.WaitAsync();
            }

            try
            {
                var currentToken = await localStorage.GetItemAsync<string>("my-access-token");
                if (!string.Equals(currentToken, rejectedToken, StringComparison.Ordinal))
                {
                    return;
                }

                _accessToken.Value = null;
                await localStorage.RemoveItemAsync("my-access-token");
                await localStorage.RemoveItemAsync("my-refresh-token");
                _unauthorizedNotifier.Value?.Notify();
            }
            finally
            {
                semaphore?.Release();
            }
        }

        private static async Task<HttpRequestMessage> CreateRequestMessageAsync(HttpMethod method, string url, HttpContent? content = null)
        {
            var request = new HttpRequestMessage(method, url);
            if (content != null)
            {
                request.Content = content;
            }

            var localStorage = _localStorage.Value;
            var token = localStorage == null
                ? _accessToken.Value
                : await localStorage.GetItemAsync<string>("my-access-token");
            _accessToken.Value = token;

            if (!string.IsNullOrWhiteSpace(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                System.Console.WriteLine($"[AuthFlow] {method.Method} {url} using bearer {DescribeToken(token)}");
            }
            else
            {
                System.Console.WriteLine($"[RequestClient] {method.Method} {url} sent without bearer token");
            }

            var clientUserAgent = _clientUserAgent.Value;
            if (!string.IsNullOrWhiteSpace(clientUserAgent))
            {
                request.Headers.TryAddWithoutValidation("X-Client-UserAgent", clientUserAgent);
            }

            return request;
        }
    }

    public class ResponseApi
    {
        public string message { get; set; }
        public int status { get; set; }
    }
}
