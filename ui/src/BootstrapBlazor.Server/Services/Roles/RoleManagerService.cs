using BootstrapBlazor.Server.Http;
using Blazored.LocalStorage;

namespace BootstrapBlazor.Server.Services
{
    /// <summary>One page permission from the API catalog (mirrors Core.Const.PermissionDefinition).</summary>
    public class PermissionCatalogItem
    {
        public string Code { get; set; } = "";
        public string Module { get; set; } = "";
        public string ModuleText { get; set; } = "";
        public string Text { get; set; } = "";
        public string Url { get; set; } = "";
    }

    public interface IRoleManagerService
    {
        Task<List<RoleDto>> GetListAsync();
        Task<List<RoleClaimDto>> GetClaimListAsync(int roleId);
        Task<List<PermissionCatalogItem>> GetPermissionCatalogAsync();
        Task<List<RoleDefaultPageDto>> GetDefaultPagesAsync();
        Task SaveDefaultPagesAsync(List<RoleDefaultPageDto> input);
        Task CreateClaimAsync(int roleId, string claimType, string claimValue);
        Task DeleteClaimAsync(int roleId, string claimType, string claimValue);
    }

    public class RoleManagerService  : IRoleManagerService
    {
        private readonly ILocalStorageService _localStorage;

        public RoleManagerService(ILocalStorageService localStorage)
        {
            _localStorage = localStorage;
        }

        public async Task<List<RoleDto>> GetListAsync()
        {
           await EnsureRequestAuthAsync();
           return await RequestClient.GetAPIAsync<List<RoleDto>>("role");
        }

        public async Task<List<RoleClaimDto>> GetClaimListAsync(int roleId)
        {
            await EnsureRequestAuthAsync();
            return await RequestClient.GetAPIAsync<List<RoleClaimDto>>($"role/get-claims/{roleId}");
        }

        public async Task<List<PermissionCatalogItem>> GetPermissionCatalogAsync()
        {
            await EnsureRequestAuthAsync();
            return await RequestClient.GetAPIAsync<List<PermissionCatalogItem>>("role/permission-catalog");
        }

        public async Task<List<RoleDefaultPageDto>> GetDefaultPagesAsync()
        {
            await EnsureRequestAuthAsync();
            return await RequestClient.GetAPIAsync<List<RoleDefaultPageDto>>("role/default-pages") ?? new();
        }

        public async Task SaveDefaultPagesAsync(List<RoleDefaultPageDto> input)
        {
            await EnsureRequestAuthAsync();
            await RequestClient.PostAPIAsync<object>("role/default-pages", input, notifyOk: false);
        }

        public async Task CreateClaimAsync(int roleId, string claimType, string claimValue)
        {
            await EnsureRequestAuthAsync();
            await RequestClient.PostAPIAsync<object>("role/create-claim",
                new { RoleId = roleId, ClaimType = claimType, ClaimValue = claimValue }, notifyOk: false);
        }

        public async Task DeleteClaimAsync(int roleId, string claimType, string claimValue)
        {
            await EnsureRequestAuthAsync();
            await RequestClient.PostAPIAsync<object>("role/delete-claim",
                new { RoleId = roleId, ClaimType = claimType, ClaimValue = claimValue }, notifyOk: false);
        }

        private async Task EnsureRequestAuthAsync()
        {
            RequestClient.InjectServices(_localStorage);
            var token = await _localStorage.GetItemAsync<string>("my-access-token");
            if (!string.IsNullOrWhiteSpace(token))
            {
                RequestClient.AttachToken(token);
            }
        }
    }
}
