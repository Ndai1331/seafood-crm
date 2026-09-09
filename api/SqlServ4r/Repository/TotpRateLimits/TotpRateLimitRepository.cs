using Domain.Identity.TotpRateLimits;
using SqlServ4r.EntityFramework;
using SqlServ4r.RepGenerationPatten;
using Volo.Abp.DependencyInjection;

namespace SqlServ4r.Repository.TotpRateLimits
{
    public class TotpRateLimitRepository : GenericRepository<TotpRateLimit, int>, ITransientDependency
    {
        public TotpRateLimitRepository(DreamContext context) : base(context)
        {
        }
    }
}
