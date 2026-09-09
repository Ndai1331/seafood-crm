using Domain.Identity.UserLogins;
using JetBrains.Annotations;
using SqlServ4r.EntityFramework;
using SqlServ4r.RepGenerationPatten;
using Volo.Abp.DependencyInjection;

namespace SqlServ4r.Repository.UserLogins
{
    public class UserLoginRepository : GenericRepository<UserLogin, Guid>, ITransientDependency
    {
        public UserLoginRepository([System.Diagnostics.CodeAnalysis.NotNull]DreamContext context) : base(context)
        {
        }
    }
}
