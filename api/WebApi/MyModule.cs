using Application;
using Contract;
using SqlServ4r;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace WebApi
{
    [DependsOn(
        typeof(AbpAutofacModule),
        typeof(ApplicationModule),
        typeof(DbModule),
        typeof(ContractModule)
    )]
    public class MyModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            // Configure services here if needed
        }
    }
}

