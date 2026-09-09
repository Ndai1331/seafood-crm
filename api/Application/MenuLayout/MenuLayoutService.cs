using Contract.MenuLayout;
using Domain.MenuLayout;
using Microsoft.EntityFrameworkCore;
using SqlServ4r.EntityFramework;
using Volo.Abp.DependencyInjection;

namespace Application.MenuLayout
{
    public class MenuLayoutService : IMenuLayoutService, ITransientDependency
    {
        private readonly DreamContext _db;

        public MenuLayoutService(DreamContext db)
        {
            _db = db;
        }

        public async Task<List<MenuOverrideDto>> GetAsync()
        {
            return await _db.MenuOverrides
                .OrderBy(x => x.SortOrder)
                .Select(x => new MenuOverrideDto
                {
                    ItemType = x.ItemType,
                    ItemKey = x.ItemKey,
                    ModuleText = x.ModuleText,
                    TextOverride = x.TextOverride,
                    SortOrder = x.SortOrder,
                    Icon = x.Icon,
                    IsEnabled = x.IsEnabled
                })
                .ToListAsync();
        }

        public async Task SaveAsync(MenuLayoutSnapshotDto input, string? updatedBy)
        {
            var existing = await _db.MenuOverrides.ToListAsync();
            _db.MenuOverrides.RemoveRange(existing);
            foreach (var item in input.Items)
            {
                _db.MenuOverrides.Add(new MenuOverride
                {
                    ItemType = item.ItemType,
                    ItemKey = item.ItemKey,
                    ModuleText = item.ModuleText,
                    TextOverride = item.TextOverride,
                    SortOrder = item.SortOrder,
                    Icon = item.Icon,
                    IsEnabled = item.IsEnabled,
                    UpdatedBy = updatedBy,
                    UpdatedAt = DateTime.UtcNow
                });
            }
            await _db.SaveChangesAsync();
        }
    }
}
