using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Contract.AppHistories;

namespace Contract.AppHistories
{
    public interface IAppHistoryService
    {
        Task<List<AppHistoryDto>> GetListAsync();
        Task CreateAsync(CreateUpdateAppHistoryDto input);
        Task<AppHistoryDto> UpdateAsync(CreateUpdateAppHistoryDto input,int id);
        Task<List<FeatureUsageDto>> GetTopFeaturesAsync(AppHistoryStatsFilterDto filter);
        Task<List<UserActivityStatsDto>> GetUserActivityStatsAsync(AppHistoryStatsFilterDto filter);
        Task<List<DailyActivityDto>> GetDailyActivityAsync(AppHistoryStatsFilterDto filter);
        Task<List<HourlyActivityDto>> GetHourlyActivityAsync(AppHistoryStatsFilterDto filter);
        Task<ActivitySummaryDto> GetSummaryAsync(AppHistoryStatsFilterDto filter);
    }
}