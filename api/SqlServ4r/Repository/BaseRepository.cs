using SqlServ4r.EntityFramework;
using Volo.Abp.DependencyInjection;

namespace SqlServ4r.Repository
{
    public class BaseRepository : ITransientDependency
    {
        public readonly DreamContext _context; 

        public BaseRepository(DreamContext context)
        {
            _context = context;
        }
    }
}