using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Contract.AppConfigs;

namespace Contract.AppConfigs
{
    public interface IAppConfigService
    {
        Task<AppConfigDto> GetAppliedConfigAsync();
        Task<AppConfigDto> UpdateConfigAsync(AppConfigDto appConfigDto);
    }
}