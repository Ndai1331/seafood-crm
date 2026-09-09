using Application.Helpers;
using Contract.AppConfigs;
using Core.Const;
using Core.Exceptions;
using Domain.AppConfigs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SqlServ4r.Repository.AppConfigs;
using System.Net;
using Volo.Abp.DependencyInjection;

namespace Application.AppConfigs
{
    public class AppConfigService :ServiceBase,IAppConfigService,ITransientDependency
    {
        private readonly AppConfigRepository _appConfigRepository;
        private readonly IConfiguration _configuration;
        
        public AppConfigService(AppConfigRepository appConfigRepository
                ,IConfiguration configuration
           )
        {
            _appConfigRepository = appConfigRepository;
            _configuration = configuration;
        }

        public async Task<AppConfigDto> GetAppliedConfigAsync()
        {
            var config = await _appConfigRepository.GetQueryable().FirstOrDefaultAsync();
            if (config is null)
            {
                return new AppConfigDto()
                {
                    SearchVolumeEasyRange = "1000000",
                    SearchVolumeMediumRange = "1000000",
                    SearchVolumeHardLevel1Range = "1000000",
                    SearchVolumeHardLevel2Range = "1000000",
                    SearchVolumeHardLevel3Range = "1000000",
                    QuyTrinhDoiDomainHtml = string.Empty
                };
            }
            return ObjectMapper.Map<AppConfig,AppConfigDto>(config);
        }

        public async Task<AppConfigDto> UpdateConfigAsync(AppConfigDto appConfigDto)
        {
            var config = await _appConfigRepository.GetQueryable().FirstOrDefaultAsync();
            if (config is null)
            {
                config = ObjectMapper.Map<AppConfigDto, AppConfig>(appConfigDto);
                await _appConfigRepository.AddAsync(config);
                return ObjectMapper.Map<AppConfig,AppConfigDto>(config);
            }
            config.SearchVolumeVeryEasyRange = appConfigDto.SearchVolumeVeryEasyRange;
            config.SearchVolumeEasyRange = appConfigDto.SearchVolumeEasyRange;
            config.SearchVolumeMediumRange = appConfigDto.SearchVolumeMediumRange;
            config.SearchVolumeHardLevel1Range = appConfigDto.SearchVolumeHardLevel1Range;
            config.SearchVolumeHardLevel2Range = appConfigDto.SearchVolumeHardLevel2Range;
            config.SearchVolumeHardLevel3Range = appConfigDto.SearchVolumeHardLevel3Range;
            config.SearchVolumeHardLevel4Range = appConfigDto.SearchVolumeHardLevel4Range;
            config.SearchVolumeHardLevel5Range = appConfigDto.SearchVolumeHardLevel5Range;
            config.QuyTrinhDoiDomainHtml = appConfigDto.QuyTrinhDoiDomainHtml;
            await _appConfigRepository.UpdateAsync(config);
            return ObjectMapper.Map<AppConfig,AppConfigDto>(config);
        }
    }
}
