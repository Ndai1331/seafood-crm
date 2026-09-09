using Domain.AppHistories;
using JetBrains.Annotations;
using SqlServ4r.EntityFramework;
using SqlServ4r.RepGenerationPatten;
using Volo.Abp.DependencyInjection;

namespace SqlServ4r.Repository.AppHistorys
{
    public class AppHistoryRepository  : GenericRepository<AppHistory, Guid>, ITransientDependency
    {
        public AppHistoryRepository([System.Diagnostics.CodeAnalysis.NotNull] DreamContext context) : base(context)
        {
        }
    }
}