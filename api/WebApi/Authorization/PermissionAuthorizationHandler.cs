using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Application.Identity.RoleManager;
using Microsoft.AspNetCore.Authorization;

namespace WebApi.Authorization
{
    /// <summary>
    /// Grants a PermissionRequirement when any of the user's roles holds the permission claim.
    /// ADMIN bypasses all permission checks (legacy god-mode behavior preserved).
    /// </summary>
    public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
    {
        private readonly IPermissionResolver _permissionResolver;

        public PermissionAuthorizationHandler(IPermissionResolver permissionResolver)
        {
            _permissionResolver = permissionResolver;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
            PermissionRequirement requirement)
        {
            var roles = context.User.Claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value)
                .ToList();

            if (roles.Count == 0)
            {
                return;
            }

            if (roles.Any(r => r == Core.Const.RoleNames.Admin || r == Core.Const.RoleNames.SuperAdmin))
            {
                context.Succeed(requirement);
                return;
            }

            var permissions = await _permissionResolver.GetPermissionsForRolesAsync(roles);
            if (permissions.Contains(requirement.Code))
            {
                context.Succeed(requirement);
            }
        }
    }
}
