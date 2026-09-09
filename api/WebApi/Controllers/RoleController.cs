using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Application.Identity.RoleManager;
using Contract;
using Contract.Identity.RoleManager;
using Core.Const;
using Microsoft.AspNetCore.Authorization;
using Domain.Identity.Roles;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebApi.Authorization;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/role/")]
    [Authorize]
    public class RoleController : ControllerBase, IRoleManagerService
    {
        public RoleManagerService RoleManagerService;
        private readonly IPermissionResolver _permissionResolver;
        private readonly RoleManager<Role> _identityRoleManager;

        public RoleController(RoleManagerService roleManagerService,
            IPermissionResolver permissionResolver,
            RoleManager<Role> identityRoleManager)
        {
            RoleManagerService = roleManagerService;
            _permissionResolver = permissionResolver;
            _identityRoleManager = identityRoleManager;
        }

        /// <summary>Only SUPER_ADMIN may mutate the SUPER_ADMIN role itself.</summary>
        private async Task EnsureSuperAdminRoleGuardAsync(int roleId)
        {
            if (User.IsInRole(RoleNames.SuperAdmin))
            {
                return;
            }
            var role = await _identityRoleManager.FindByIdAsync(roleId.ToString());
            if (role != null && string.Equals(role.Name, RoleNames.SuperAdmin, StringComparison.OrdinalIgnoreCase))
            {
                throw new Core.Exceptions.GlobalException(
                    "Chỉ SUPER_ADMIN mới được thao tác trên role SUPER_ADMIN.", System.Net.HttpStatusCode.Forbidden);
            }
        }

        /// <summary>
        /// Permission codes of the current user, resolved from their role claims.
        /// UI calls this after login to build the menu and route guard.
        /// </summary>
        [HttpGet]
        [Route("my-permissions")]
        public async Task<MyPermissionsResultDto> GetMyPermissionsAsync()
        {
            var roles = User.Claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value)
                .ToList();

            string? defaultPageUrl = null;
            foreach (var roleName in roles)
            {
                var role = await _identityRoleManager.FindByNameAsync(roleName);
                if (!string.IsNullOrWhiteSpace(role?.DefaultPageUrl))
                {
                    defaultPageUrl = role.DefaultPageUrl;
                    break;
                }
            }

            // ADMIN/SUPER_ADMIN bypass mirrors PermissionAuthorizationHandler: full catalog
            // regardless of stored claims, so they can never be locked out of the UI.
            var permissions = roles.Contains(RoleNames.Admin) || roles.Contains(RoleNames.SuperAdmin)
                ? Permissions.All.Select(d => d.Code).OrderBy(x => x).ToList()
                : (await _permissionResolver.GetPermissionsForRolesAsync(roles)).OrderBy(x => x).ToList();

            return new MyPermissionsResultDto
            {
                Permissions = permissions,
                DefaultPageUrl = defaultPageUrl
            };
        }

        [HttpGet]
        [Route("default-pages")]
        [HasPermission(Permissions.MenuArrangement)]
        public async Task<List<RoleDefaultPageDto>> GetDefaultPagesAsync()
        {
            return await RoleManagerService.GetDefaultPagesAsync();
        }

        [HttpPost]
        [Route("default-pages")]
        [HasPermission(Permissions.MenuArrangement)]
        public async Task SaveDefaultPagesAsync(List<RoleDefaultPageDto> input)
        {
            await RoleManagerService.SaveDefaultPagesAsync(input);
        }

        /// <summary>
        /// Full permission catalog (module → pages) for the admin permission matrix page.
        /// </summary>
        [HttpGet]
        [Route("permission-catalog")]
        [HasPermission(Permissions.PermissionMatrix)]
        public List<PermissionDefinition> GetPermissionCatalog()
        {
            return Permissions.All.ToList();
        }

        [HttpGet]
        public async Task<List<RoleDto>> GetListAsync()
        {
            return await RoleManagerService.GetListAsync();
        }

        [HttpPost]
        [HasPermission(Permissions.UserManager)]
        public async Task<RoleDto> CreateAsync(CreateUpdateRoleDto input)
        {
            return await RoleManagerService.CreateAsync(input);
        }

        [HttpPut]
        [Route("{id}")]
        [HasPermission(Permissions.UserManager)]
        public async Task<RoleDto> UpdateAsync(CreateUpdateRoleDto input, int id)
        {
            await EnsureSuperAdminRoleGuardAsync(id);
            return await RoleManagerService.UpdateAsync(input, id);
        }

        [HttpDelete]
        [Route("{id}")]
        [HasPermission(Permissions.UserManager)]
        public async Task DeleteAsync(int id)
        {
             await EnsureSuperAdminRoleGuardAsync(id);
             await RoleManagerService.DeleteAsync(id);
        }

        [HttpGet]
        [Route("get-claims/{roleId}")]
        [HasPermission(Permissions.PermissionMatrix)]
        public async Task<List<RoleClaimDto>> GetClaimListAsync(int roleId)
        {
            return await RoleManagerService.GetClaimListAsync(roleId);
        }

        // Was [Authorize("EmployeeOnly")] — that policy was never registered and threw at runtime.
        [HttpPost]
        [Route("create-claim")]
        [HasPermission(Permissions.PermissionMatrix)]
        public async Task<CreateUpdateClaimRole> CreateClaimAsync(CreateUpdateClaimRole input)
        {
            return await RoleManagerService.CreateClaimAsync(input);
        }

        [HttpPost]
        [Route("delete-claim")]
        [HasPermission(Permissions.PermissionMatrix)]
        public async Task DeleteClaimAsync(RoleClaimModel input)
        {
             await RoleManagerService.DeleteClaimAsync(input);
        }
    }
}
