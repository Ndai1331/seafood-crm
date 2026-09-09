using System.ComponentModel;

namespace Core.Enum
{
    public enum WebMenuType
    {
        [Description("_dt_")] PostDetail = 1,
        [Description("_n_")] PostCategory = 2,
        [Description("_p_")] ServiceType = 3,
        [Description("_pdt_")] ServiceDetail = 4,
        [Description("_a_")] Url = 5,
        [Description("_sf_")] Doctor = 6
    }
}