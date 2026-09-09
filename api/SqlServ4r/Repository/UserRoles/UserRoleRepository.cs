using System;
using Domain.Identity.UserRoles;
using JetBrains.Annotations;
using SqlServ4r.EntityFramework;
using SqlServ4r.RepGenerationPatten;
using SqlServ4r.Repository.RoleClaims;
using Volo.Abp.DependencyInjection;

namespace SqlServ4r.Repository.UserRoles
{
    public class UserRoleRepository : GenericRepository<UserRole,Guid>,ITransientDependency
    {
        public UserRoleRepository([System.Diagnostics.CodeAnalysis.NotNull] DreamContext context) : base(context)
        {
        }
    }
}