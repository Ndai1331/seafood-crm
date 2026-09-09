using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.AppConfigs
{
    [Table("appconfigs")]
    public class AppConfig
    {
        public int Id { get; set; }
        [MaxLength(255)]
        public string? SearchVolumeVeryEasyRange { get; set; }
        [MaxLength(255)]
        public string? SearchVolumeEasyRange { get; set; }
        [MaxLength(255)]
        public string? SearchVolumeMediumRange { get; set; }
        [MaxLength(255)]
        public string? SearchVolumeHardLevel1Range { get; set; }
        [MaxLength(255)]
        public string? SearchVolumeHardLevel2Range { get; set; }
        [MaxLength(255)]
        public string? SearchVolumeHardLevel3Range { get; set; }
        [MaxLength(255)]
        public string? SearchVolumeHardLevel4Range { get; set; }
        [MaxLength(255)]
        public string? SearchVolumeHardLevel5Range { get; set; }
        [Column(TypeName = "text")]
        public string? QuyTrinhDoiDomainHtml { get; set; }
    }
}
