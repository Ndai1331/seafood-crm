using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;

namespace SqlServ4r.Helper
{
    public static class LinqExtension
    {
        public static IQueryable<TSource> OrderByIf<TSource, TKey>(this  IQueryable<TSource> y, 
            bool condition, Expression<Func<TSource, TKey>> keySelector)
        {
            return condition ? y.OrderBy(keySelector) : y;
        }
        public static IQueryable<TSource> OrderByDescIf<TSource, TKey>(this  IQueryable<TSource> y, 
            bool condition, Expression<Func<TSource, TKey>> keySelector)
        {
            return condition ? y.OrderByDescending(keySelector) : y;
        }
    }
}