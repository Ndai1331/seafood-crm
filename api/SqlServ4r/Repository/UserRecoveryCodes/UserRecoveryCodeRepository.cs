using Domain.Identity.UserRecoveryCodes;
using SqlServ4r.EntityFramework;
using SqlServ4r.RepGenerationPatten;
using Volo.Abp.DependencyInjection;

namespace SqlServ4r.Repository.UserRecoveryCodes
{
    public class UserRecoveryCodeRepository : GenericRepository<UserRecoveryCode, int>, ITransientDependency
    {
        public UserRecoveryCodeRepository(DreamContext context) : base(context)
        {
        }
    }
}
