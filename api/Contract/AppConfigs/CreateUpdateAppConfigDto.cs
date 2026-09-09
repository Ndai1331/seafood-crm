using Core.Const;
using System;
using System.ComponentModel.DataAnnotations;

namespace Contract.AppConfigs
{
    public class CreateUpdateAppConfigDto
    {
        public string? SearchVolumeVeryEasyRange { get; set; }
        public string? SearchVolumeEasyRange { get; set; }
        public string? SearchVolumeMediumRange { get; set; }
        public string? SearchVolumeHardLevel1Range { get; set; }
        public string? SearchVolumeHardLevel2Range { get; set; }
        public string? SearchVolumeHardLevel3Range { get; set; }
        public string? SearchVolumeHardLevel4Range { get; set; }
        public string? SearchVolumeHardLevel5Range { get; set; }
        public string? QuyTrinhDoiDomainHtml { get; set; }
        
    }
}
