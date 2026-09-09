using System;
using Domain.UserDepartments;
using JetBrains.Annotations;
using SqlServ4r.EntityFramework;
using SqlServ4r.RepGenerationPatten;
using SqlServ4r.Repository.Users;
using Volo.Abp.DependencyInjection;

namespace SqlServ4r.Repository.UserDepartments
{
    public class UserDepartmentRepository : GenericRepository<UserDepartment, Guid>, ITransientDependency
    {
        public UserDepartmentRepository([System.Diagnostics.CodeAnalysis.NotNull] DreamContext context) : base(context)
        {
        }
    }
}