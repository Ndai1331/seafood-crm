using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace SqlServ4r.RepGenerationPatten
{
    public interface IGenericRepository<T, TKey> where T : class where TKey : struct
    {
        Task<T?> GetAsync(Expression<Func<T, bool>> expression);
        Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> expression);
        Task<List<T>> GetListAsync(Expression<Func<T, bool>> expression);
        Task<List<T>> ToListAsync();
        List<T> ToList();
        Task<long> GetCountAsync();
        Task<bool> AnyAsync(Expression<Func<T, bool>> expression);
        void Update(T entity, bool save = true);
        Task UpdateAsync(T entity, bool save = true);
        void UpdateRange(IEnumerable<T> entities, bool save = true);
        void Add(T entity, bool save = true);
        Task<EntityEntry<T>> InsertAsync(T entity, bool save = true);
        Task AddAsync(T entity, bool save = true);
        Task AddRangeAsync(IEnumerable<T> entities, bool save = true);
        void Remove(T entity, bool save = true);
        void RemoveRange(IEnumerable<T> entities, bool save = true);
        void Save();
        void SaveAsync();
    }
}