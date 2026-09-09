using Domain.Teams;
using SqlServ4r.EntityFramework;
using SqlServ4r.RepGenerationPatten;
using Volo.Abp.DependencyInjection;

namespace SqlServ4r.Repository.Teams
{
    public class TeamRepository : GenericRepository<Team, int>, ITransientDependency
    {
        public TeamRepository(DreamContext context) : base(context)
        {
        }

        // Check if code exists for create operation
        public async Task<bool> IsCodeExistsAsync(string code)
        {
            return await AnyAsync(x => x.Code == code);
        }

        // Check if code exists for update operation (exclude current id)
        public async Task<bool> IsCodeExistsAsync(string code, int excludeId)
        {
            return await AnyAsync(x => x.Code == code && x.Id != excludeId);
        }
    }
}

