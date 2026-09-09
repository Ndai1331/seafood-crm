using System.Linq;
using System.Net;
using System.Security.Claims;
using Application.Identity.RoleManager;
using Application.Identity.UserManager;
using Contract;
using Contract.Identity.UserManager;
using Contract.Uploads;
using Core.Const;
using Core.Enum;
using Core.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Domain.Identity.Users;
using Microsoft.AspNetCore.Identity;
using WebApi.Authorization;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/user/")]
    [Authorize]
    public class UserController : ControllerBase
    {
        private IUserManagerService _userManagerService;
        private IConfiguration _configuration;
        private readonly UserManager<User> _identityUserManager;
        private readonly IPermissionResolver _permissionResolver;

        public UserController(IUserManagerService userManagerService, IConfiguration configuration,
            UserManager<User> identityUserManager, IPermissionResolver permissionResolver)
        {
            _userManagerService = userManagerService;
            _configuration = configuration;
            _identityUserManager = identityUserManager;
            _permissionResolver = permissionResolver;
        }

        private bool CallerIsSuperAdmin => User.IsInRole(RoleNames.SuperAdmin);

        /// <summary>
        /// True when the caller may manage OTHER users (holds the user-manager permission).
        /// ADMIN/SUPER_ADMIN always qualify (permission bypass). Used to tell admin
        /// management apart from self-service on the shared update-user-with-roles endpoint.
        /// </summary>
        private async Task<bool> IsUserManagerAsync()
        {
            if (User.IsInRole(RoleNames.Admin) || User.IsInRole(RoleNames.SuperAdmin))
            {
                return true;
            }
            var roles = User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList();
            var permissions = await _permissionResolver.GetPermissionsForRolesAsync(roles);
            return permissions.Contains(Permissions.UserManager);
        }

        /// <summary>
        /// SUPER_ADMIN escalation guard: only a SUPER_ADMIN may grant the role or
        /// modify/delete an account that already holds it.
        /// </summary>
        private async Task EnsureSuperAdminGuardAsync(IEnumerable<string>? requestedRoles, int? targetUserId)
        {
            if (CallerIsSuperAdmin)
            {
                return;
            }

            if (requestedRoles != null && requestedRoles.Any(r =>
                    string.Equals(r?.Trim(), RoleNames.SuperAdmin, StringComparison.OrdinalIgnoreCase)))
            {
                throw new Core.Exceptions.GlobalException(
                    "Chỉ SUPER_ADMIN mới được cấp quyền SUPER_ADMIN.", System.Net.HttpStatusCode.Forbidden);
            }

            if (targetUserId.HasValue)
            {
                var target = await _identityUserManager.FindByIdAsync(targetUserId.Value.ToString());
                if (target != null && await _identityUserManager.IsInRoleAsync(target, RoleNames.SuperAdmin))
                {
                    throw new Core.Exceptions.GlobalException(
                        "Chỉ SUPER_ADMIN mới được sửa/xóa tài khoản SUPER_ADMIN.", System.Net.HttpStatusCode.Forbidden);
                }
            }
        }

        [HttpGet]
        [Route("get-list-with-nav")]
        public async Task<List<UserWithNavigationPropertiesDto>> GetListWithNavigationAsync()
        {
            return await _userManagerService.GetListWithNavigationAsync();
        }

        [HttpPost]
        [Route("get-basic-info-users-with-navigation-properties")]
        public async Task<List<UserBasicInfoDto>> GetBasicInfoUsersWithNavigationAsync(
            PhoneBookFilter filter
        )
        {
            return await _userManagerService.GetBasicInfoUsersWithNavigationAsync(filter);
        }

        [HttpGet]
        [Route("get-with-nav-properties/{id}")]
        public async Task<UserWithNavigationPropertiesDto> GetWithNavigationProperties(int id)
        {
            return await _userManagerService.GetWithNavigationProperties(id);
        }

        [HttpPost]
        [Route("create-user-with-roles")]
        [HasPermission(Permissions.UserManager)]
        public async Task<UserDto> CreateUserWithNavigationPropertiesAsync(CreateUserDto input)
        {
            await EnsureSuperAdminGuardAsync(input.Roles, null);
            return await _userManagerService.CreateUserWithNavigationPropertiesAsync(input);
        }

        // Shared endpoint: user-managers edit anyone; every other user self-services their
        // own profile (UI UpdateMyProfileAsync). So it CANNOT be locked to [HasPermission] —
        // that would 403 self-service. Instead: non-managers may only edit their own id and
        // cannot change privileged fields (roles/type/active are forced to stored values).
        [HttpPost]
        [Route("update-user-with-roles/{id}")]
        public async Task<UserDto> UpdateUserWithNavigationPropertiesAsync(
            UpdateUserDto input,
            int id
        )
        {
            if (!await IsUserManagerAsync())
            {
                var currentUserId = GetCurrentUserId();
                if (currentUserId == null || currentUserId.Value != id)
                {
                    throw new GlobalException("Bạn chỉ được sửa hồ sơ của chính mình.", HttpStatusCode.Forbidden);
                }

                // Ignore any privileged fields the client sent — pin them to stored values.
                var stored = await _userManagerService.GetWithNavigationProperties(id);
                if (stored?.User != null)
                {
                    input.Roles = stored.RoleNames ?? new List<string>();
                    input.UserType = stored.User.UserType;
                    input.IsActive = stored.User.IsActive;
                }
            }

            await EnsureSuperAdminGuardAsync(input.Roles, id);
            return await _userManagerService.UpdateUserWithNavigationPropertiesAsync(input, id);
        }

        [HttpPost]
        [Route("update-basic-info-from-doctor-info/{id}")]
        public async Task<UserDto> UpdateBasicInfoFromDoctorInfo(UpdateUserDto input, int id)
        {
            return await _userManagerService.UpdateBasicInfoFromDoctorInfo(input, id);
        }

        [HttpPost]
        [Route("delete-with-nav")]
        [HasPermission(Permissions.UserManager)]
        public async Task DeleteWithNavigationAsync(int id)
        {
            await EnsureSuperAdminGuardAsync(null, id);
            await _userManagerService.DeleteWithNavigationAsync(id);
        }

        [HttpGet]
        public async Task<List<UserDto>> GetListAsync()
        {
            return await _userManagerService.GetListAsync();
        }

        [HttpGet]
        [Route("get-basic-user-info")]
        public async Task<List<UserIdentityDto>> GetBasicUserInfosAsync()
        {
            return await _userManagerService.GetBasicUserInfosAsync();
        }

        [HttpGet]
        [Route("get-basic-seo-user-info")]
        public async Task<List<UserIdentityDto>> GetBasicUserInfoNotPatientAsync(
            [FromQuery] int? userId = null, [FromQuery] string? role = null, [FromQuery] UserType? userType = null, [FromQuery] int? teamId = null)
        {
            return await _userManagerService.GetBasicSeoUserInfosAsync(userId, role, userType, teamId);
        }

        private int? GetCurrentUserId()
        {
            // Try different claim types for user ID
            var userIdClaim = HttpContext.User.FindFirst("sub") ?? 
                             HttpContext.User.FindFirst("userId") ?? 
                             HttpContext.User.FindFirst("primarysid") ??
                             HttpContext.User.FindFirst("http://schemas.microsoft.com/ws/2008/06/identity/claims/primarysid");
            return userIdClaim != null && int.TryParse(userIdClaim.Value, out var userId) ? userId : null;
        }

        [HttpPost]
        public async Task<UserDto> CreateAsync(CreateUserDto input)
        {
            return await _userManagerService.CreateAsync(input);
        }

        [HttpPut]
        [Route("{id}")]
        public async Task<UserDto> UpdateAsync(UpdateUserDto input, int id)
        {
            return await _userManagerService.UpdateAsync(input, id);
        }

        [HttpDelete]
        [Route("{id}")]
        public async Task DeleteAsync(int id)
        {
            await EnsureSuperAdminGuardAsync(null, id);
            await _userManagerService.DeleteAsync(id);
        }

        [HttpPost]
        [Route("sign-in")]
        [AllowAnonymous]
        public async Task<TokenDto> SignInAsync(UserModel input)
        {
            return await _userManagerService.SignInAsync(input);
        }

        // "Login as user" — SUPER_ADMIN only. Returns a short-lived token for the target with
        // full rights of that account. Target SUPER_ADMIN is rejected in the service.
        [HttpPost]
        [Route("impersonate/{id}")]
        public async Task<TokenDto> ImpersonateAsync(int id)
        {
            if (!CallerIsSuperAdmin)
            {
                throw new GlobalException("Chỉ SUPER_ADMIN mới được dùng chức năng đăng nhập hộ.", HttpStatusCode.Forbidden);
            }
            var impersonatorId = GetCurrentUserId();
            if (impersonatorId == null)
            {
                throw new GlobalException("Không xác định được người dùng hiện tại.", HttpStatusCode.Unauthorized);
            }
            return await _userManagerService.ImpersonateAsync(id, impersonatorId.Value);
        }

        [HttpPost]
        [Route("sign-up")]
        [AllowAnonymous]
        public async Task<UserDto> SignUpAsync(CreateUserDto input)
        {
            return await _userManagerService.SignUpAsync(input);
        }

        [HttpPost]
        [Route("update-profile")]
        public async Task<UserDto> UpdateProfile(UpdateUserProfileRequestDto input)
        {
            return await _userManagerService.UpdateProfile(input);
        }
        
        // Was [AllowAnonymous] — SetNewPasswordAsync only does FindByName + reset (no old-password
        // or reset-token check), so anonymous callers could reset ANY account (account takeover).
        // Now requires auth and forces the target to the caller; the self-service modal never
        // collects a username, so behavior is unchanged for legitimate use.
        [HttpPost]
        [Route("set-password")]
        public async Task<bool> SetNewPasswordAsync(NewUserPasswordDto input)
        {
            input.UserName = User.Identity?.Name;
            return await _userManagerService.SetNewPasswordAsync(input);
        }

        [HttpPost]
        [Route("refresh-token")]
        [AllowAnonymous]
        public async Task<TokenDto> RefreshTokenAsync(TokenModel token)
        {
            return await _userManagerService.RefreshTokenAsync(token);
        }

        [HttpGet]
        [Route("get-with-roles/{roleName}")]
        public async Task<List<UserDto>> GetListByRoles(string roleName)
        {
            UserFilterPagingModel filter = new UserFilterPagingModel();

            filter.RoleName = roleName;
            filter.Take = 0;
            return await _userManagerService.GetListByRoles(filter);
        }

        [HttpGet]
        [Route("get-with-roles/{createBy}/{roleName}")]
        public async Task<List<UserDto>> GetListByRoles(string createBy, string roleName)
        {
            UserFilterPagingModel filter = new UserFilterPagingModel();

            filter.RoleName = roleName;
            filter.CreatedBy =
                int.TryParse(createBy, out int userId) && userId != null ? userId : null;
            filter.Take = 0;

            return await _userManagerService.GetListByRoles(filter);
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        public Task<List<UserDto>> GetListByRoles(UserFilterPagingModel? filter = null)
        {
            throw new NotImplementedException();
        }

        [HttpPost]
        [Route("get-with-filters")]
        public async Task<ApiResponseBase<List<UserDto>>> GetListByFilterAsync(
            UserFilterPagingModel filter
        )
        {
            return await _userManagerService.GetListByFilterAsync(filter);
        }

        [HttpPost]
        [Route("sign-out")]
        public async Task Logout(UserLogOutModel input)
        {
            await _userManagerService.Logout(input);
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("sign-out-website")]
        public async Task LogoutWebsite(WebisteUserLogOutModel token)
        {
            await _userManagerService.LogoutWebsite(token);
        }

        [HttpPost]
        [Route("clone-user")]
        public async Task<UserDto> CloneUserAsync(CloneUserDto input)
        {
            return await _userManagerService.CloneUserAsync(input);
        }
    }
}
