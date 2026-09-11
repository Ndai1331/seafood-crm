using Application.AppHistories;
using Contract;
using Contract.AppHistories;
using Core.Const;
using Core.Exceptions;
using Domain.AppHistories;
using Microsoft.EntityFrameworkCore;
using SqlServ4r.Repository.AppHistorys;
using SqlServ4r.Repository.Users;
using System.Net;
using Volo.Abp.DependencyInjection;

namespace Application.AppHistorys
{
    public class AppHistoryService : ServiceBase,IAppHistoryService,ITransientDependency
    {
       private readonly AppHistoryRepository _appHistoryRepository;
       private readonly UserRepository _userRepository;


        public AppHistoryService(AppHistoryRepository appHistoryRepository, UserRepository userRepository)
        {
            _appHistoryRepository = appHistoryRepository;
            _userRepository = userRepository;
        }
       
        
        public async Task CreateAsync(CreateUpdateAppHistoryDto input)
        {
            try
            {
                var appHistory = ObjectMapper.Map<CreateUpdateAppHistoryDto, AppHistory>(input);
                await _appHistoryRepository.AddAsync(appHistory);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
       
        }

        public async Task<AppHistoryDto> UpdateAsync(CreateUpdateAppHistoryDto input, int id)
        {
            var item = await _appHistoryRepository.FirstOrDefaultAsync(x => x.Id == id);

            if (item is null)
            {
                throw new GlobalException(HttpMessage.NotFound, HttpStatusCode.BadRequest);
            }
            
            var appHistory = ObjectMapper.Map(input,item);
            await _appHistoryRepository.UpdateAsync(appHistory);
            return ObjectMapper.Map<AppHistory,AppHistoryDto>(appHistory);
        }

        public async Task<List<AppHistoryDto>> GetListAsync()
        {
            var appHistorys = await _appHistoryRepository.GetQueryable().
                OrderByDescending(x=>x.Date).ToListAsync();
            return ObjectMapper.Map<List<AppHistory>, List<AppHistoryDto>>(appHistorys);
        }

        public async Task<List<FeatureUsageDto>> GetTopFeaturesAsync(AppHistoryStatsFilterDto filter)
        {
            var since = DateTime.Now.AddDays(-filter.Days);
            var data = await _appHistoryRepository.GetQueryable()
                .Where(x => x.Date >= since && (filter.UserId == null || x.UserId == filter.UserId))
                .GroupBy(x => x.Functions)
                .Select(g => new { Path = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(filter.Limit)
                .ToListAsync();

            return data.Select(x => new FeatureUsageDto
            {
                FunctionPath = x.Path ?? string.Empty,
                FeatureLabel = FeatureLabelMapper.GetLabel(x.Path ?? string.Empty),
                Count = x.Count
            }).ToList();
        }

        public async Task<List<UserActivityStatsDto>> GetUserActivityStatsAsync(AppHistoryStatsFilterDto filter)
        {
            var since = DateTime.Now.AddDays(-filter.Days);
            var data = await _appHistoryRepository.GetQueryable()
                .Include(x => x.User)
                .Where(x => x.Date >= since && (filter.UserId == null || x.UserId == filter.UserId))
                .GroupBy(x => new { x.UserId, x.User.FirstName, x.User.LastName })
                .Select(g => new
                {
                    g.Key.UserId,
                    g.Key.FirstName,
                    g.Key.LastName,
                    TotalRequests = g.Count(),
                    LastActivity = g.Max(x => x.Date)
                })
                .OrderByDescending(x => x.TotalRequests)
                .Take(filter.Limit)
                .ToListAsync();

            return data.Select(x => new UserActivityStatsDto
            {
                UserId = x.UserId,
                FullName = $"{x.FirstName} {x.LastName}".Trim(),
                TotalRequests = x.TotalRequests,
                LastActivity = x.LastActivity
            }).ToList();
        }

        public async Task<List<DailyActivityDto>> GetDailyActivityAsync(AppHistoryStatsFilterDto filter)
        {
            var since = DateTime.Now.Date.AddDays(-filter.Days + 1);
            var raw = await _appHistoryRepository.GetQueryable()
                .Where(x => x.Date >= since && (filter.UserId == null || x.UserId == filter.UserId))
                .GroupBy(x => x.Date.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .ToListAsync();

            var lookup = raw.ToDictionary(x => x.Date.Date, x => x.Count);
            var result = new List<DailyActivityDto>(filter.Days);
            for (int i = filter.Days - 1; i >= 0; i--)
            {
                var day = DateTime.Now.Date.AddDays(-i);
                result.Add(new DailyActivityDto
                {
                    Date = day.ToString("yyyy-MM-dd"),
                    Count = lookup.TryGetValue(day, out var c) ? c : 0
                });
            }
            return result;
        }

        public async Task<List<HourlyActivityDto>> GetHourlyActivityAsync(AppHistoryStatsFilterDto filter)
        {
            var since = DateTime.Now.AddDays(-filter.Days);
            var raw = await _appHistoryRepository.GetQueryable()
                .Where(x => x.Date >= since && (filter.UserId == null || x.UserId == filter.UserId))
                .GroupBy(x => x.Date.Hour)
                .Select(g => new { Hour = g.Key, Count = g.Count() })
                .ToListAsync();

            var lookup = raw.ToDictionary(x => x.Hour, x => x.Count);
            return Enumerable.Range(0, 24).Select(h => new HourlyActivityDto
            {
                Hour = h,
                Count = lookup.TryGetValue(h, out var c) ? c : 0
            }).ToList();
        }

        public async Task<ActivitySummaryDto> GetSummaryAsync(AppHistoryStatsFilterDto filter)
        {
            var today = DateTime.Now.Date;
            var since30 = DateTime.Now.AddDays(-30);

            var totalToday = await _appHistoryRepository.GetQueryable()
                .CountAsync(x => x.Date >= today && (filter.UserId == null || x.UserId == filter.UserId));

            var uniqueUsersToday = await _appHistoryRepository.GetQueryable()
                .Where(x => x.Date >= today && (filter.UserId == null || x.UserId == filter.UserId))
                .Select(x => x.UserId)
                .Distinct()
                .CountAsync();

            var totalLast30Days = await _appHistoryRepository.GetQueryable()
                .CountAsync(x => x.Date >= since30 && (filter.UserId == null || x.UserId == filter.UserId));

            var topFeature = await _appHistoryRepository.GetQueryable()
                .Where(x => x.Date >= since30 && (filter.UserId == null || x.UserId == filter.UserId))
                .GroupBy(x => x.Functions)
                .Select(g => new { Path = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .FirstOrDefaultAsync();

            var topUser = await _appHistoryRepository.GetQueryable()
                .Include(x => x.User)
                .Where(x => x.Date >= since30 && (filter.UserId == null || x.UserId == filter.UserId))
                .GroupBy(x => new { x.UserId, x.User.FirstName, x.User.LastName })
                .Select(g => new { g.Key.FirstName, g.Key.LastName, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .FirstOrDefaultAsync();

            return new ActivitySummaryDto
            {
                TotalToday = totalToday,
                UniqueUsersToday = uniqueUsersToday,
                TotalLast30Days = totalLast30Days,
                MostUsedFeature = topFeature != null ? FeatureLabelMapper.GetLabel(topFeature.Path ?? string.Empty) : string.Empty,
                MostActiveUser = topUser != null ? $"{topUser.FirstName} {topUser.LastName}".Trim() : string.Empty
            };
        }

        public async Task<ApiResponseBase<AppHistorySearchResponseDto>> GetListAsync(AppHistoryFilterPagingDto filter)
        {
            ApiResponseBase<AppHistorySearchResponseDto> result = new ApiResponseBase<AppHistorySearchResponseDto>();

            try
            {
                result.Data = new AppHistorySearchResponseDto();

                var appHistorys = _appHistoryRepository
                    .GetQueryable()
                    .Include(x => x.User)
                    .AsQueryable()
                    .Where(x => (filter.Date != null ? x.Date.Date == filter.Date : true)
                    && (string.IsNullOrEmpty(filter.Search)
                        || x.Functions.Contains(filter.Search.Trim())
                        || x.Operation.Contains(filter.Search.Trim())
                        || x.IpAddress.Contains(filter.Search.Trim())
                        || (x.UserAgent ?? string.Empty).Contains(filter.Search.Trim())
                        || (x.DeviceType ?? string.Empty).Contains(filter.Search.Trim())
                        || (x.Browser ?? string.Empty).Contains(filter.Search.Trim())
                        || (x.OperatingSystem ?? string.Empty).Contains(filter.Search.Trim())
                        || (x.User != null && (x.User.FirstName + " " + x.User.LastName).Contains(filter.Search.Trim())))
                    && (!string.IsNullOrEmpty(filter.Functions) ? x.Functions.Contains(filter.Functions.Trim()) : true)
                    && (!string.IsNullOrEmpty(filter.IpAddress) ? x.IpAddress.Contains(filter.IpAddress.Trim()) : true)
                    && (!string.IsNullOrEmpty(filter.Operation) ? x.Operation.Contains(filter.Operation.Trim()) : true)
                    && (!string.IsNullOrEmpty(filter.FullName) ? (x.User.FirstName + " " + x.User.LastName).Contains(filter.FullName.Trim()) : true)
                    && (filter.UserId == null || x.UserId == filter.UserId)
                    && (string.IsNullOrEmpty(filter.DeviceType) || x.DeviceType == filter.DeviceType)
                    && (string.IsNullOrEmpty(filter.Browser) || x.Browser == filter.Browser)
                    && (string.IsNullOrEmpty(filter.OperatingSystem) || x.OperatingSystem == filter.OperatingSystem)
                    && (filter.Succeeded == null || x.Succeeded == filter.Succeeded));

                result.Data.TotalItem = appHistorys.Count();

                if (filter.Take > 0)
                    appHistorys = appHistorys.OrderByDescending(x => x.Date).Skip(Math.Max(0, filter.Skip)).Take(Math.Min(filter.Take, 500));

                result.Data.Result = ObjectMapper.Map<List<AppHistory>, List<AppHistoryDto>>(await appHistorys.ToListAsync());
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
            }

            return result;
        }
    }
}
