namespace Contract.MenuLayout
{
    public interface IMenuLayoutService
    {
        Task<List<MenuOverrideDto>> GetAsync();
        Task SaveAsync(MenuLayoutSnapshotDto input, string? updatedBy);
    }
}
