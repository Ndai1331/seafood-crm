using Application.AppHistorys;
using Contract;
using Contract.AppHistories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
        public async Task<List<AppHistoryDto>> GetListAsync()
        {
            return await _appHistoryService.GetListAsync();
        }

        [HttpPost]
        [Route("search")]
        public async Task<ApiResponseBase<AppHistorySearchResponseDto>> GetListAsync(AppHistoryFilterPagingDto filter)
        {
            return await _appHistoryService.GetListAsync(filter);
        }

        [HttpPost("stats/top-features")]
        [Authorize(Roles = "ADMIN")]
        public async Task<List<FeatureUsageDto>> GetTopFeaturesAsync(AppHistoryStatsFilterDto filter)
            => await _appHistoryService.GetTopFeaturesAsync(filter);

        [HttpPost("stats/by-user")]
        [Authorize(Roles = "ADMIN")]
        public async Task<List<UserActivityStatsDto>> GetUserActivityStatsAsync(AppHistoryStatsFilterDto filter)
            => await _appHistoryService.GetUserActivityStatsAsync(filter);

        [HttpPost("stats/by-day")]
        [Authorize(Roles = "ADMIN")]
        public async Task<List<DailyActivityDto>> GetDailyActivityAsync(AppHistoryStatsFilterDto filter)
            => await _appHistoryService.GetDailyActivityAsync(filter);

        [HttpPost("stats/by-hour")]
        [Authorize(Roles = "ADMIN")]
        public async Task<List<HourlyActivityDto>> GetHourlyActivityAsync(AppHistoryStatsFilterDto filter)
            => await _appHistoryService.GetHourlyActivityAsync(filter);

        [HttpPost("stats/summary")]
        [Authorize(Roles = "ADMIN")]
        public async Task<ActivitySummaryDto> GetSummaryAsync(AppHistoryStatsFilterDto filter)
            => await _appHistoryService.GetSummaryAsync(filter);
    }
}