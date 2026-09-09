using System;
using System.Collections.Generic;
using Domain.Identity.RoleClaims;
using Domain.Identity.Roles;

namespace SqlServ4r.Repository.RoleClaims
{
    public interface IRoleClaimRepository
    {
        List<RoleClaim> GetRoleClaimsByRoles(List<int> roleIds);
    }
}