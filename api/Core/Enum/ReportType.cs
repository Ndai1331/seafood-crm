using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Core.Enum
{
    public enum ReportType
    {
        [Description("Keyword")]
        Keyword,
        [Description("Domain")]
        Domain
    }
    public enum EmployeeLevel
    {
        [Description("Junior")]
        Junior =1,
        [Description("Middle")]
        Middle =2,
        [Description("Senior")]
        Senior =3,
        [Description("Lead")]
        Lead =4,
    }

    public enum TopRange
    {

        [Description("1")]
        Top1=1,
        [Description("2-3")]
        Top2ToTop3=2,
        [Description("4-5")]
        Top4ToTop5=3, 
        [Description("6-7")]
        Top6ToTop7=4, 
        [Description("8-10")]
        Top8ToTop10=5, 
        
        [Description("1-5")]
        Top1ToTop5=6,
        [Description("6-10")]
        Top6ToTop10=7,
        
        
    }
    public enum TopRangePIC
    {
        [Description("1")]
        Top1,
        [Description("2-3")]
        Top2ToTop3,
        [Description("4-5")]
        Top4ToTop5,
        [Description("6-10")]
        Top6ToTop10,
    }

    public enum ReportDateType
    {
        [Description("Tháng")]
        Month,
        [Description("Năm")]
        Year
    }


    public enum LevelSearchVolumne
    {
        [Description("Tất cả")]
        All,
        [Description("Rất Dễ")]
        VeryEasy,
        [Description("Dễ")]
        Easy,
        [Description("Trung Bình Khó")]
        Medium,
        [Description("Khó")]
        Hard,
        [Description("Khó Cao")]
        VeryHard,
        [Description("Rất Khó")]
        ExtremeHard,
        [Description("Cực Khó")]
        HardLevel4,
        [Description("Siêu Khó")]
        HardLevel5
    }


}
