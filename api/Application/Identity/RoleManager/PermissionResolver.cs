using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Core.Const;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using SqlServ4r.EntityFramework;
using Volo.Abp.DependencyInjection;

namespace Application.Identity.RoleManager
{
    public interface IPermissionResolver
    {
        /// <summary>Resolve the union of permission codes for the given role names.</summary>
        Task<HashSet<string>> GetPermissionsForRolesAsync(IEnumerable<string> roleNames);

        /// <summary>Drop all cached role→permission entries (call after claim changes).</summary>
        void InvalidateCache();
    }

    /// <summary>
    /// Resolves role names → permission codes from RoleClaims, cached in-memory.
    /// Invalidation uses a version counter baked into cache keys, so stale entries
    /// are orphaned immediately and expire via TTL.
    /// </summary>
    public class PermissionResolver : IPermissionResolver, ITransientDependency
    {
        private static int _cacheVersion;
        private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(5);

        private readonly DreamContext _context;
        private readonly IMemoryCache _cache;

        public PermissionResolver(DreamContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        public async Task<HashSet<string>> GetPermissionsForRolesAsync(IEnumerable<string> roleNames)
        {
            var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var roleName in roleNames.Where(r => !string.IsNullOrWhiteSpace(r)).Distinct())
            {
                var codes = await GetPermissionsForRoleAsync(roleName.Trim());
                result.UnionWith(codes);
            }
            return result;
        }

        public void InvalidateCache()
        {
            Interlocked.Increment(ref _cacheVersion);
        }

        private async Task<HashSet<string>> GetPermissionsForRoleAsync(string roleName)
        {
            var cacheKey = $"perm:v{Volatile.Read(ref _cacheVersion)}:{roleName.ToUpperInvariant()}";
            if (_cache.TryGetValue(cacheKey, out HashSet<string>? cached) && cached != null)
            {
                return cached;
            }

            var normalized = roleName.ToUpper();
            var codes = await (from role in _context.Roles
                    join claim in _context.RoleClaims on role.Id equals claim.RoleId
                    where role.Name != null && role.Name.ToUpper() == normalized
                          && claim.ClaimType == PermissionClaimType.Permission
                          && claim.ClaimValue != null
                    select claim.ClaimValue!)
                .ToListAsync();

            var set = new HashSet<string>(codes, StringComparer.OrdinalIgnoreCase);
            _cache.Set(cacheKey, set, CacheTtl);
            return set;
        }
    }
}
