using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Identity.RoleClaims;
using Domain.Identity.Roles;
using JetBrains.Annotations;
using SqlServ4r.EntityFramework;
using SqlServ4r.RepGenerationPatten;
using Volo.Abp.DependencyInjection;

namespace SqlServ4r.Repository.RoleClaims
{
    public class RoleClaimRepository : GenericRepository<RoleClaim,Guid>,ITransientDependency,IRoleClaimRepository
    {
        public RoleClaimRepository([System.Diagnostics.CodeAnalysis.NotNull] DreamContext context) : base(context)
        {
        }

  
        public List<RoleClaim> GetRoleClaimsByRoles(List<int> roleIds)
        {
            var result = from roleId in roleIds
                join claim in _context.RoleClaims on roleId equals claim.RoleId
                select claim;
            return result.ToList();
        }

      
    }
}