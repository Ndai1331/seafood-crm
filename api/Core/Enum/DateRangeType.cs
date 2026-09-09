using System.ComponentModel;

namespace Core.Enum
{
    public enum DateRangeType
    {
        [Description("Today")]
        Today = 1,
        [Description("Yesterday")]
        Yesterday = 2,
        [Description("Last 7 Days")]
        Last7Days = 3,
        [Description("Last 30 Days")]
        Last30Days = 4,
        [Description("Last Month")]
        LastMonth = 5,
        [Description("This Month")]
        ThisMonth = 6,
        [Description("3 Month Ago")]
        _3MonthsAgo = 7,
        [Description("Last Year")]
        LastYear
    }
}