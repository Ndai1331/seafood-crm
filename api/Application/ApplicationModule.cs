using Volo.Abp.Modularity;

namespace Application
{
    [DependsOn()]
    public class ApplicationModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
        }
    }
}
