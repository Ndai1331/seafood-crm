using Microsoft.AspNetCore.Authorization;

namespace WebApi.Authorization
{
    /// <summary>Requirement carrying a single page/module permission code.</summary>
    public class PermissionRequirement : IAuthorizationRequirement
    {
        public string Code { get; }

        public PermissionRequirement(string code)
        {
            Code = code;
        }
    }
}
