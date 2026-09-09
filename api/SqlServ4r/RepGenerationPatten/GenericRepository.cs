using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using SqlServ4r.EntityFramework;
using Volo.Abp.DependencyInjection;

namespace SqlServ4r.RepGenerationPatten
{
    public class GenericRepository<T, Key> : IGenericRepository<T, Key>
        where T : class
        where Key : struct
    {
        protected readonly DreamContext _context;

        public GenericRepository(DreamContext context)
        {
            _context = context;
        }

        public DbSet<T> GetQueryable()
        {
            return _context.Set<T>();
        }

        public async Task<T?> GetAsync(Expression<Func<T, bool>> expression)
        {
            return await _context.Set<T>().SingleOrDefaultAsync(expression);
        }
        public async Task<bool> AnyAsync(Expression<Func<T, bool>> expression)
        {
            return await _context.Set<T>().AnyAsync(expression);
        }
        public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> expression)
        {
            return await _context.Set<T>().FirstOrDefaultAsync(expression);
        }

        public async Task<List<T>> GetListAsync(Expression<Func<T, bool>>? expression)
        {
            if(expression != null)
                return await _context.Set<T>().Where(expression).ToListAsync();
            else
                return await _context.Set<T>().ToListAsync();
        }

        public async Task<List<T>> ToListAsync()
        {
            return await _context.Set<T>().ToListAsync();
        }

        public List<T> ToList()
        {
            return _context.Set<T>().ToList();
        }

        public async Task<long> GetCountAsync()
        {
            return await _context.Set<T>().CountAsync();
        }

        public void Update(T entity, bool save = true)
        {
            _context.Set<T>().Update(entity);
            if (save)
            {
                _context.SaveChanges();
            }
        }

        public async Task UpdateAsync(T entity, bool save = true)
        {
            _context.Set<T>().Update(entity);
            if (save)
            {
                await _context.SaveChangesAsync();
            }
        }

        public void UpdateRange(IEnumerable<T> entities, bool save = true)
        {
            _context.Set<T>().UpdateRange(entities);
            if (save)
            {
                _context.SaveChanges();
            }
        }

        public void Add(T entity, bool save = true)
        {
            _context.Set<T>().Add(entity);
            if (save)
            {
                _context.SaveChanges();
            }
        }

        public async Task<EntityEntry<T>> InsertAsync(T entity, bool save = true)
        {
            var item = await _context.Set<T>().AddAsync(entity);
            if (save)
            {
                await _context.SaveChangesAsync();
            }
            return item;
        }

        public async Task AddAsync(T entity, bool save = true)
        {
            await _context.Set<T>().AddAsync(entity);
            if (save)
            {
                await _context.SaveChangesAsync();
            }
        }

        public async Task AddRangeAsync(IEnumerable<T> entities, bool save = true)
        {
            await _context.Set<T>().AddRangeAsync(entities);
            if (save)
            {
                await _context.SaveChangesAsync();
            }
        }

        public void Remove(T entity, bool save = true)
        {
            _context.Set<T>().Remove(entity);
            if (save)
            {
                _context.SaveChanges();
            }
        }

        public void RemoveRange(IEnumerable<T> entities, bool save = true)
        {
            _context.Set<T>().RemoveRange(entities);
            if (save)
            {
                _context.SaveChanges();
            }
        }

        public void Save()
        {
            _context.SaveChanges();
        }

        public void SaveAsync()
        {
            _context.SaveChangesAsync();
        }

        // public async Task<bool> AnyAsync( Expression<Func<T, bool>> expression)
        // {
        //     return await _context.Set<T>().AnyAsync(expression);
        // }
        //
        // public  IEnumerable<TSource> Select<TSource, TKey>(Func<TSource, TKey> keySelector)
        // {
        //    return _context.Set<T>().Select(keySelector);
        // }
        //
        // public  IEnumerable<TKey> Select<TSource, TKey>(Func<T, TKey> keySelector)
        // {
        //     return _context.Set<T>().Select(keySelector);
        // }
    }
}
