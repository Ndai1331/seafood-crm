using System.Collections.Generic;

namespace Contract.Identity.RoleManager
{
    public class MyPermissionsResultDto
    {
        public List<string> Permissions { get; set; } = new();
        public string? DefaultPageUrl { get; set; }
    }
}
