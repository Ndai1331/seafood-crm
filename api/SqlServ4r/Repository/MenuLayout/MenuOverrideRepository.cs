using Domain.MenuLayout;
using SqlServ4r.EntityFramework;
using SqlServ4r.RepGenerationPatten;
using Volo.Abp.DependencyInjection;

namespace SqlServ4r.Repository.MenuLayout
{
    public class MenuOverrideRepository : GenericRepository<MenuOverride, int>, ITransientDependency
    {
        public MenuOverrideRepository(DreamContext context) : base(context)
        {
        }
    }
}
