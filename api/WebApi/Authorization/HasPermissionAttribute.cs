using Microsoft.AspNetCore.Authorization;

namespace WebApi.Authorization
{
    /// <summary>
    /// Usage: [HasPermission(Permissions.UserManager)] — enforces the permission
    /// via the dynamic "perm:" policy provider.
    /// </summary>
    public class HasPermissionAttribute : AuthorizeAttribute
    {
        public HasPermissionAttribute(string permissionCode)
        {
            Policy = PermissionPolicyProvider.PolicyPrefix + permissionCode;
        }
    }
}
