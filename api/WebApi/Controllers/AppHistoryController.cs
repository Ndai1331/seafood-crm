using Application.AppHistorys;
using Contract;
using Contract.AppHistories;
using Core.Const;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.Authorization;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/appHistories/")]
    [Authorize]
    public class AppHistoryController : IAppHistoryService
    {
        private AppHistoryService _appHistoryService;
        public AppHistoryController(AppHistoryService appHistoryService)
        {
            _appHistoryService = appHistoryService;
        }
        
        [HttpPost]
        public async Task CreateAsync(CreateUpdateAppHistoryDto input)
        {
            await _appHistoryService.CreateAsync(input); 
        }

        [HttpPut]
        [Route("{id}")]
        public async Task<AppHistoryDto> UpdateAsync(CreateUpdateAppHistoryDto input, int id)
        {
            return  await _appHistoryService.UpdateAsync(input,id);
        }

        [HttpGet]
        [HasPermission(Permissions.SystemAudit)]
        public async Task<List<AppHistoryDto>> GetListAsync()
        {
            return await _appHistoryService.GetListAsync();
        }

        [HttpPost]
        [Route("search")]
        [HasPermission(Permissions.SystemAudit)]
        public async Task<ApiResponseBase<AppHistorySearchResponseDto>> GetListAsync(AppHistoryFilterPagingDto filter)
        {
            return await _appHistoryService.GetListAsync(filter);
        }

        [HttpPost("stats/top-features")]
        [HasPermission(Permissions.SystemAudit)]
        public async Task<List<FeatureUsageDto>> GetTopFeaturesAsync(AppHistoryStatsFilterDto filter)
            => await _appHistoryService.GetTopFeaturesAsync(filter);

        [HttpPost("stats/by-user")]
        [HasPermission(Permissions.SystemAudit)]
        public async Task<List<UserActivityStatsDto>> GetUserActivityStatsAsync(AppHistoryStatsFilterDto filter)
            => await _appHistoryService.GetUserActivityStatsAsync(filter);

        [HttpPost("stats/by-day")]
        [HasPermission(Permissions.SystemAudit)]
        public async Task<List<DailyActivityDto>> GetDailyActivityAsync(AppHistoryStatsFilterDto filter)
            => await _appHistoryService.GetDailyActivityAsync(filter);

        [HttpPost("stats/by-hour")]
        [HasPermission(Permissions.SystemAudit)]
        public async Task<List<HourlyActivityDto>> GetHourlyActivityAsync(AppHistoryStatsFilterDto filter)
            => await _appHistoryService.GetHourlyActivityAsync(filter);

        [HttpPost("stats/summary")]
        [HasPermission(Permissions.SystemAudit)]
        public async Task<ActivitySummaryDto> GetSummaryAsync(AppHistoryStatsFilterDto filter)
            => await _appHistoryService.GetSummaryAsync(filter);
    }
}
