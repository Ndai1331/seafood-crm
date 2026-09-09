using BootstrapBlazor.Server.Data;
using BootstrapBlazor.Server.Http;
using BootstrapBlazor.Server.Exceptions;
using Microsoft.AspNetCore.Components.Authorization;
using Blazored.LocalStorage;
using Microsoft.JSInterop;

namespace BootstrapBlazor.Server.Services
{
    public interface ITeamService
    {
        /// <summary>
        /// Gets a list of teams with pagination and filtering
        /// </summary>
        /// <param name="filter">Filter and pagination options</param>
        /// <returns>Response containing list of teams</returns>
        Task<ResponseHttpBase<List<TeamDto>>> GetListAsync(BaseFilterPagingDto filter);
        
        /// <summary>
        /// Gets a team by ID
        /// </summary>
        /// <param name="id">Team ID</param>
        /// <returns>Response containing team data</returns>
        Task<ResponseHttpBase<TeamDto>> GetByIdAsync(int id);
        
        /// <summary>
        /// Creates a new team
        /// </summary>
        /// <param name="dto">Team creation data</param>
        /// <returns>Response indicating success or failure</returns>
        Task<ResponseHttpBase<bool>> CreateAsync(CreateTeamDto dto);
        
        /// <summary>
        /// Updates an existing team
        /// </summary>
        /// <param name="id">Team ID</param>
        /// <param name="dto">Team update data</param>
        /// <returns>Response indicating success or failure</returns>
        Task<ResponseHttpBase<bool>> UpdateAsync(int id, UpdateTeamDto dto);
        
        /// <summary>
        /// Deletes a team by ID
        /// </summary>
        /// <param name="id">Team ID</param>
        /// <returns>Response indicating success or failure</returns>
        Task<ResponseHttpBase<bool>> DeleteAsync(int id);
    }

    /// <summary>
    /// Service for managing teams
    /// </summary>
    public class TeamService : ITeamService
    {
        private readonly ILocalStorageService _localStorage;
        private readonly AuthenticationStateProvider _authenticationStateProvider;
        private readonly IJSRuntime _jsRuntime;

        /// <summary>
        /// Initializes a new instance of TeamService
        /// </summary>
        /// <param name="localStorage">Local storage service</param>
        /// <param name="authenticationStateProvider">Authentication state provider</param>
        /// <param name="jsRuntime">JavaScript runtime</param>
        public TeamService(ILocalStorageService localStorage, AuthenticationStateProvider authenticationStateProvider, IJSRuntime jsRuntime)
        {
            _localStorage = localStorage;
            _authenticationStateProvider = authenticationStateProvider;
            _jsRuntime = jsRuntime;
        }

        private async Task LogToBrowserAsync(string message)
        {
            try
            {
                await _jsRuntime.InvokeVoidAsync("console.log", $"[TeamService] {message}");
            }
            catch
            {
                // Fallback to server console if JS runtime is not available
                System.Console.WriteLine($"[TeamService] {message}");
            }
        }

        private async Task EnsureAuthenticationAsync()
        {
            try
            {
                // Inject services and ensure authentication state is checked
                RequestClient.InjectServices(_localStorage);

                // Get authentication state to trigger token attachment
                var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
                var isAuth = authState.User.Identity?.IsAuthenticated ?? false;

                await LogToBrowserAsync($"Authentication state: IsAuthenticated={isAuth}");

                if (!isAuth)
                {
                    await LogToBrowserAsync("User not authenticated - API calls may fail");
                    return;
                }

                // Also manually attach token if available
                var token = await _localStorage.GetItemAsync<string>("my-access-token");
                if (!string.IsNullOrEmpty(token))
                {
                    RequestClient.AttachToken(token);
                    await LogToBrowserAsync($"Token attached: {token.Substring(0, Math.Min(20, token.Length))}...");

                    // Verify token is not expired
                    if (IsTokenExpired(token))
                    {
                        await LogToBrowserAsync("Token appears to be expired - refresh may be needed");
                    }
                }
                else
                {
                    await LogToBrowserAsync("No token found in local storage");
                }
            }
            catch (Exception ex)
            {
                // Log authentication error but don't throw to avoid breaking the flow
                await LogToBrowserAsync($"Authentication setup error: {ex.Message}");
            }
        }

        private bool IsTokenExpired(string token)
        {
            try
            {
                var parts = token.Split('.');
                if (parts.Length != 3) return true;

                var payload = parts[1];
                // Pad base64 string if needed
                payload = payload.Replace('-', '+').Replace('_', '/');
                switch (payload.Length % 4)
                {
                    case 2: payload += "=="; break;
                    case 3: payload += "="; break;
                }

                var jsonPayload = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(Convert.FromBase64String(payload));
                if (jsonPayload != null && jsonPayload.ContainsKey("exp"))
                {
                    var exp = long.Parse(jsonPayload["exp"].ToString());
                    var expTime = DateTimeOffset.FromUnixTimeSeconds(exp);
                    return DateTimeOffset.UtcNow >= expTime;
                }
            }
            catch
            {
                return true; // If we can't parse, assume expired
            }
            return false;
        }

        public async Task<ResponseHttpBase<List<TeamDto>>> GetListAsync(BaseFilterPagingDto filter)
        {
            try
            {
                // Ensure authentication is set up before making API calls
                await EnsureAuthenticationAsync();

                await LogToBrowserAsync($"GETLIST: Request - Skip: {filter.Skip}, Take: {filter.Take}, FilterText: {filter.FilterText}");

                // Use RequestClient like other services - this will include authentication headers
                try
                {
                    var response = await RequestClient.PostAPIAsync<ResponseHttpBase<List<TeamDto>>>("team/list", filter);

                    await LogToBrowserAsync($"GETLIST: Response - Status: {response?.Status}, Data count: {response?.Data?.Count ?? 0}, Total: {response?.Total}, Message: {response?.Message}");

                    if (response != null && response.Data != null)
                    {
                        return response;
                    }
                    else
                    {
                        return new ResponseHttpBase<List<TeamDto>>
                        {
                            Data = new List<TeamDto>(),
                            Status = false,
                            Message = response?.Message ?? "No data received from API"
                        };
                    }
                }
                catch (UnauthorizedException)
                {
                    // Handle unauthorized access - user needs to login
                    return new ResponseHttpBase<List<TeamDto>>
                    {
                        Data = new List<TeamDto>(),
                        Status = false,
                        Message = "Bạn cần đăng nhập để truy cập dữ liệu này"
                    };
                }
                catch (ServerErrorException)
                {
                    // Handle server errors
                    return new ResponseHttpBase<List<TeamDto>>
                    {
                        Data = new List<TeamDto>(),
                        Status = false,
                        Message = "Lỗi server. Vui lòng thử lại sau."
                    };
                }
            }
            catch (Exception ex)
            {
                return new ResponseHttpBase<List<TeamDto>>
                {
                    Data = new List<TeamDto>(),
                    Status = false,
                    Message = $"Error loading data: {ex.Message}"
                };
            }
        }

        public async Task<ResponseHttpBase<TeamDto>> GetByIdAsync(int id)
        {
            try
            {
                await EnsureAuthenticationAsync();
                var response = await RequestClient.GetAPIAsync<ResponseHttpBase<TeamDto>>($"team/{id}");
                return response;
            }
            catch (Exception ex)
            {
                return new ResponseHttpBase<TeamDto>
                {
                    Status = false,
                    Message = $"Exception: {ex.Message}"
                };
            }
        }

        public async Task<ResponseHttpBase<bool>> CreateAsync(CreateTeamDto dto)
        {
            try
            {
                await EnsureAuthenticationAsync();
                await LogToBrowserAsync($"CREATE: Sending DTO - Code: {dto.Code}, Name: {dto.Name}");

                var response = await RequestClient.PostAPIAsync<ResponseHttpBase<object>>("team", dto);

                await LogToBrowserAsync($"CREATE: API Response - Status: {response?.Status}, Message: {response?.Message}");

                if (response != null)
                {
                    return new ResponseHttpBase<bool>
                    {
                        Data = response.Status,
                        Status = response.Status,
                        Message = response.Message ?? "Create operation completed"
                    };
                }
                else
                {
                    return new ResponseHttpBase<bool>
                    {
                        Data = false,
                        Status = false,
                        Message = "API response was null"
                    };
                }
            }
            catch (UnauthorizedException ex)
            {
                await LogToBrowserAsync($"CREATE: Unauthorized - {ex.Message}");
                return new ResponseHttpBase<bool>
                {
                    Data = false,
                    Status = false,
                    Message = "Bạn cần đăng nhập để thực hiện thao tác này"
                };
            }
            catch (ServerErrorException ex)
            {
                await LogToBrowserAsync($"CREATE: Server Error - {ex.Message}");
                return new ResponseHttpBase<bool>
                {
                    Data = false,
                    Status = false,
                    Message = "Lỗi server. Vui lòng thử lại sau."
                };
            }
            catch (Exception ex)
            {
                await LogToBrowserAsync($"CREATE: Exception - {ex.Message}");
                return new ResponseHttpBase<bool>
                {
                    Data = false,
                    Status = false,
                    Message = $"Lỗi: {ex.Message}"
                };
            }
        }

        public async Task<ResponseHttpBase<bool>> UpdateAsync(int id, UpdateTeamDto dto)
        {
            try
            {
                await EnsureAuthenticationAsync();
                await LogToBrowserAsync($"UPDATE: Sending DTO - Id: {dto.Id}, Code: {dto.Code}, Name: {dto.Name}");

                var response = await RequestClient.PutAPIAsync<ResponseHttpBase<object>>("team", dto);

                await LogToBrowserAsync($"UPDATE: API Response - Status: {response?.Status}, Data: {response?.Data}, Message: {response?.Message}");

                if (response != null)
                {
                    // For UPDATE, check if Status is true OR if Data is not null (indicating successful update)
                    var isSuccess = response.Status || response.Data != null;

                    return new ResponseHttpBase<bool>
                    {
                        Data = isSuccess,
                        Status = isSuccess,
                        Message = response.Message ?? "Update operation completed"
                    };
                }
                else
                {
                    return new ResponseHttpBase<bool>
                    {
                        Data = false,
                        Status = false,
                        Message = "API response was null"
                    };
                }
            }
            catch (UnauthorizedException ex)
            {
                await LogToBrowserAsync($"UPDATE: Unauthorized - {ex.Message}");
                return new ResponseHttpBase<bool>
                {
                    Data = false,
                    Status = false,
                    Message = "Bạn cần đăng nhập để thực hiện thao tác này"
                };
            }
            catch (ServerErrorException ex)
            {
                await LogToBrowserAsync($"UPDATE: Server Error - {ex.Message}");
                return new ResponseHttpBase<bool>
                {
                    Data = false,
                    Status = false,
                    Message = "Lỗi server. Vui lòng thử lại sau."
                };
            }
            catch (Exception ex)
            {
                await LogToBrowserAsync($"UPDATE: Exception - {ex.Message}");
                return new ResponseHttpBase<bool> { Data = false, Status = false, Message = $"Lỗi: {ex.Message}" };
            }
        }

        public async Task<ResponseHttpBase<bool>> DeleteAsync(int id)
        {
            try
            {
                await EnsureAuthenticationAsync();
                var response = await RequestClient.DeleteAPIAsync<ResponseHttpBase<bool>>($"team/{id}");
                return response ?? new ResponseHttpBase<bool> { Data = false, Status = false, Message = "API response was null" };
            }
            catch (Exception ex)
            {
                return new ResponseHttpBase<bool> { Data = false, Status = false, Message = $"Exception: {ex.Message}" };
            }
        }
    }
}

