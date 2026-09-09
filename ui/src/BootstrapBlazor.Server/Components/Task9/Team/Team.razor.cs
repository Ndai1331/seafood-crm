using Microsoft.AspNetCore.Components;
using BootstrapBlazor.Components;
using BootstrapBlazor.Server.Services;
using BootstrapBlazor.Server.Data;

namespace BootstrapBlazor.Server.Components.Task9.Team
{
    /// <summary>
    /// Team component class
    /// </summary>
    public partial class Team : ComponentBase
    {
        /// <summary>
        /// Gets or sets the TeamService
        /// </summary>
        [Inject]
        private ITeamService? TeamService { get; set; }

        /// <summary>
        /// Gets or sets the ToastService
        /// </summary>
        [Inject]
        private ToastService? ToastService { get; set; }

        /// <summary>
        /// Gets or sets the Logger
        /// </summary>
        [Inject]
        private ILogger<Team>? Logger { get; set; }

        /// <summary>
        /// Gets or sets the Table reference
        /// </summary>
        private Table<TeamDto>? TableRef;

        /// <summary>
        /// Gets the page items source
        /// </summary>
        private static IEnumerable<int> PageItemsSource => new int[]
        {
            10,
            20,
            40
        };

        /// <summary>
        /// Gets or sets loading state
        /// </summary>
        private bool IsLoading { get; set; }

        /// <summary>
        /// Gets or sets error message
        /// </summary>
        private string? ErrorMessage { get; set; }

        /// <summary>
        /// OnInitializedAsync method to load initial data
        /// </summary>
        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();

            // Check if service is available
            if (TeamService == null)
            {
                ErrorMessage = "TeamService không được khởi tạo";
                Logger?.LogError("TeamService is not injected");
                return;
            }

            // Test service directly
            try
            {
                Logger?.LogInformation("Testing TeamService directly...");
                var testResult = await TeamService.GetByIdAsync(1);
                if (testResult != null && testResult.Data != null)
                {
                    Logger?.LogInformation("Service test successful. Got: {@Data}", testResult.Data);
                }
                else
                {
                    Logger?.LogWarning("Service test failed or returned null. Message: {Message}", testResult?.Message);
                }
            }
            catch (Exception ex)
            {
                Logger?.LogError(ex, "Exception during service test");
                ErrorMessage = $"Service test failed: {ex.Message}";
            }

            // The table will automatically query data when it renders
            // We don't need to manually trigger data loading here
        }

        /// <summary>
        /// OnAfterRenderAsync method to ensure data is loaded after component is fully rendered
        /// </summary>
        /// <param name="firstRender">Whether this is the first render</param>
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);

            if (firstRender)
            {
                Logger?.LogInformation("Team component first render completed");

                // Wait a bit for everything to be ready
                await Task.Delay(100);

                if (TableRef != null)
                {
                    Logger?.LogInformation("TableRef is available, triggering data refresh");
                    // Trigger data refresh on first render to ensure data is loaded
                    await RefreshDataAsync();
                }
                else
                {
                    Logger?.LogWarning("TableRef is null on first render");
                }
            }
        }

        /// <summary>
        /// Refreshes the data in the table
        /// </summary>
        private async Task RefreshDataAsync()
        {
            try
            {
                if (TableRef != null)
                {
                    Logger?.LogInformation("Refreshing table data");
                    await TableRef.QueryAsync();
                }
            }
            catch (Exception ex)
            {
                Logger?.LogError(ex, "Error refreshing data");
                ErrorMessage = $"Lỗi khi làm mới dữ liệu: {ex.Message}";
            }
        }

        /// <summary>
        /// Handles the query async operation
        /// </summary>
        /// <param name="options">The query options</param>
        /// <returns>The query data</returns>
        private async Task<QueryData<TeamDto>> OnQueryAsync(QueryPageOptions options)
        {
            Logger?.LogInformation("OnQueryAsync called with PageIndex: {PageIndex}, PageItems: {PageItems}, SearchText: {SearchText}",
                options.PageIndex, options.PageItems, options.SearchText);

            if (TeamService == null)
            {
                Logger?.LogError("TeamService is null in OnQueryAsync");
                ErrorMessage = "Dịch vụ không khả dụng";
                return new QueryData<TeamDto>() { Items = new List<TeamDto>(), TotalCount = 0 };
            }

            try
            {
                IsLoading = true;
                StateHasChanged();

                // Map pagination from Table to BaseFilterPagingDto (Skip/Take)
                Logger?.LogInformation("Pagination options: PageIndex={PageIndex}, PageItems={PageItems}, StartIndex={StartIndex}",
                    options.PageIndex, options.PageItems, options.StartIndex);

                // Calculate Skip manually for reliable pagination
                // BootstrapBlazor Table's StartIndex might not be working correctly
                var calculatedSkip = (options.PageIndex - 1) * options.PageItems;

                var filter = new BaseFilterPagingDto
                {
                    Skip = calculatedSkip, // Use calculated Skip instead of StartIndex
                    Take = options.PageItems,
                    FilterText = options.SearchText,
                    Sort = string.IsNullOrEmpty(options.SortName) ? null : $"{options.SortName} {options.SortOrder}"
                };

                Logger?.LogInformation("Pagination calculation: PageIndex={PageIndex}, PageItems={PageItems}, CalculatedSkip={CalculatedSkip}, StartIndex={StartIndex}",
                    options.PageIndex, options.PageItems, calculatedSkip, options.StartIndex);

                Logger?.LogInformation("Calling TeamService.GetListAsync with filter: {@Filter}", filter);

                var response = await TeamService.GetListAsync(filter);

                if (response.Status && response.Data != null)
                {
                    Logger?.LogInformation("Successfully retrieved {Count} items, TotalCount: {TotalCount}",
                        response.Data.Count, response.Total);

                    ErrorMessage = null;
                    return new QueryData<TeamDto>()
                    {
                        Items = response.Data,
                        TotalCount = response.Total
                    };
                }
                else
                {
                    var errorMsg = $"API call failed: {response.Message}";
                    Logger?.LogWarning("API call to GetListAsync was not successful. Message: {Message}", response.Message);
                    ErrorMessage = errorMsg;

                    if (ToastService != null)
                    {
                        await ToastService.Warning("Cảnh báo", $"Không thể tải dữ liệu: {response.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                var errorMsg = $"Lỗi khi lấy dữ liệu: {ex.Message}";
                Logger?.LogError(ex, "Exception in OnQueryAsync");
                ErrorMessage = errorMsg;

                if (ToastService != null)
                {
                    await ToastService.Error("Lỗi", "Không thể tải dữ liệu. Vui lòng thử lại sau.");
                }
            }
            finally
            {
                IsLoading = false;
                StateHasChanged();
            }

            return new QueryData<TeamDto>() { Items = new List<TeamDto>(), TotalCount = 0 };
        }

        /// <summary>
        /// Handles the save async operation
        /// </summary>
        /// <param name="item">The item to save</param>
        /// <param name="changedType">The type of change</param>
        /// <returns>True if successful, false otherwise</returns>
        private async Task<bool> OnSaveAsync(TeamDto item, ItemChangedType changedType)
        {
            if (TeamService != null)
            {
                try
                {
                    // Validation: Only check for duplicate name if Name is provided (allow empty Name)
                    if (!string.IsNullOrWhiteSpace(item.Name))
                    {
                        // Check for duplicate name (excluding current item for UPDATE)
                        var existingItems = await GetExistingTeams();
                        bool isDuplicateName = existingItems.Any(x =>
                            x.Name.Equals(item.Name.Trim(), StringComparison.OrdinalIgnoreCase) &&
                            x.Id != item.Id);

                        if (isDuplicateName)
                        {
                            if (ToastService != null)
                            {
                                await ToastService.Warning("Cảnh báo", $"Tên team '{item.Name}' đã tồn tại. Vui lòng chọn tên khác.");
                            }
                            return false;
                        }
                    }

                    bool ok = false;
                    if (changedType == ItemChangedType.Add)
                    {
                        var createDto = new CreateTeamDto
                        {
                            Code = item.Code?.Trim() ?? string.Empty,
                            Name = item.Name?.Trim() ?? string.Empty
                        };
                        var res = await TeamService.CreateAsync(createDto);
                        ok = res.Status;
                        if (ok && ToastService != null)
                        {
                            await ToastService.Success("Thành công", "Đã thêm team mới.");
                        }
                        else if (!ok && ToastService != null)
                        {
                            await ToastService.Error("Lỗi", "Không thể thêm team. Tên có thể đã bị trùng.");
                        }
                    }
                    else if (changedType == ItemChangedType.Update)
                    {
                        var updateDto = new UpdateTeamDto
                        {
                            Id = item.Id,
                            Code = item.Code?.Trim() ?? string.Empty,
                            Name = item.Name?.Trim() ?? string.Empty
                        };
                        var res = await TeamService.UpdateAsync(item.Id, updateDto);
                        ok = res.Status;
                        if (ok && ToastService != null)
                        {
                            await ToastService.Success("Thành công", "Đã cập nhật team.");
                        }
                        else if (!ok && ToastService != null)
                        {
                            await ToastService.Error("Lỗi", "Không thể cập nhật team. Tên có thể đã bị trùng.");
                        }
                    }

                    if (ok)
                    {
                        // Use the RefreshDataAsync method to ensure proper state management
                        await RefreshDataAsync();
                    }

                    return ok;
                }
                catch (Exception ex)
                {
                    Logger?.LogError(ex, "Lỗi khi lưu dữ liệu");
                    if (ToastService != null)
                    {
                        await ToastService.Error("Lỗi", "Không thể lưu dữ liệu. Vui lòng thử lại sau.");
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Gets existing teams to check for duplicates
        /// </summary>
        /// <returns>List of existing teams</returns>
        private async Task<List<TeamDto>> GetExistingTeams()
        {
            try
            {
                if (TeamService != null)
                {
                    // Get all items (no pagination) to check for duplicates
                    var filter = new BaseFilterPagingDto
                    {
                        Skip = 0,
                        Take = 1000, // Get a large number to get all items
                        FilterText = string.Empty
                    };
                    var response = await TeamService.GetListAsync(filter);

                    if (response.Status && response.Data != null)
                    {
                        return response.Data.ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger?.LogError(ex, "Error getting existing teams for validation");
            }

            return new List<TeamDto>();
        }

        /// <summary>
        /// Handles the delete async operation
        /// </summary>
        /// <param name="items">The items to delete</param>
        /// <returns>True if successful, false otherwise</returns>
        private async Task<bool> OnDeleteAsync(IEnumerable<TeamDto> items)
        {
            if (TeamService != null)
            {
                try
                {
                    bool allSuccess = true;
                    int successCount = 0;
                    foreach (var item in items)
                    {
                        var res = await TeamService.DeleteAsync(item.Id);
                        if (res.Status)
                        {
                            successCount++;
                        }
                        else
                        {
                            allSuccess = false;
                        }
                    }

                    if (allSuccess)
                    {
                        // Use the RefreshDataAsync method to ensure proper state management
                        await RefreshDataAsync();
                        if (ToastService != null)
                        {
                            await ToastService.Success("Thành công", $"Đã xóa {successCount} team.");
                        }
                    }
                    else if (successCount > 0 && ToastService != null)
                    {
                        await ToastService.Warning("Cảnh báo", $"Đã xóa {successCount}/{items.Count()} team. Một số mục không thể xóa.");
                    }

                    return allSuccess;
                }
                catch (Exception ex)
                {
                    Logger?.LogError(ex, "Lỗi khi xóa dữ liệu");
                    if (ToastService != null)
                    {
                        await ToastService.Error("Lỗi", "Không thể xóa dữ liệu. Vui lòng thử lại sau.");
                    }
                }
            }
            return false;
        }
    }
}

