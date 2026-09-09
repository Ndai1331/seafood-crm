using Domain.Identity.UserTokens;
using JetBrains.Annotations;
using SqlServ4r.EntityFramework;
using SqlServ4r.RepGenerationPatten;
using Volo.Abp.DependencyInjection;

namespace SqlServ4r.Repository.UserTokens
{
    public class UserTokenRepository : GenericRepository<UserToken, Guid>, ITransientDependency
    {
        public UserTokenRepository([System.Diagnostics.CodeAnalysis.NotNull]DreamContext context) : base(context)
        {
        }
    }
}
