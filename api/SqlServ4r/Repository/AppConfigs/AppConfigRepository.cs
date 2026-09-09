using System;
using System.Collections.Generic;
using Contract.AppConfigs;
using Domain.AppConfigs;
using JetBrains.Annotations;
using SqlServ4r.EntityFramework;
using SqlServ4r.RepGenerationPatten;
using Volo.Abp.DependencyInjection;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace SqlServ4r.Repository.AppConfigs
{
    public class AppConfigRepository  : GenericRepository<AppConfig, int>, ITransientDependency
    {
        public AppConfigRepository([System.Diagnostics.CodeAnalysis.NotNull] DreamContext context) : base(context)
        {
        }
    }
}