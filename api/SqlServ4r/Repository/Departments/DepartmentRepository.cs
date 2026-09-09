using System;
using Domain.Departments;
using JetBrains.Annotations;
using SqlServ4r.EntityFramework;
using SqlServ4r.RepGenerationPatten;
using Volo.Abp.DependencyInjection;
using System.Linq;

namespace SqlServ4r.Repository.Departments
{
    public class DepartmentRepository  : GenericRepository<Department, Guid>, ITransientDependency
    {
        public DepartmentRepository([System.Diagnostics.CodeAnalysis.NotNull] DreamContext context) : base(context)
        {
        }
        
        public (bool ExistCode, bool ExistName) CheckDuplicateInformation(string code, string name)
        {
            var query = from user in _context.Positions
                select new ValueTuple<bool,bool>(
                    _context.Positions.Any(x => x.Code == code && x.Code != null),
                    _context.Positions.Any(x => x.Name== name)
                );
            
            return query.FirstOrDefault();
        }
        
        public (bool ExistCode, bool ExistName) CheckDuplicateInformation(string code, string name,int id)
        {
            var query = from user in _context.Positions
                select new ValueTuple<bool,bool>(
                    _context.Positions.Any(x => x.Code == code && x.Code != null && x.Id != id),
                    _context.Positions.Any(x => x.Name== name && x.Id != id)
                );
            
            return query.FirstOrDefault();
        }
    }
}