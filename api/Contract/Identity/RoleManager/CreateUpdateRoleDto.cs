using System.Collections.Generic;

namespace Contract.Identity.RoleManager
{
    public class CreateUpdateRoleDto
    {
        public string Name { get; set; }
        public string? Code { get; set; }

        public List<CreateUpdateClaimRole> Claims { get; set; } = new List<CreateUpdateClaimRole>();
    }
}