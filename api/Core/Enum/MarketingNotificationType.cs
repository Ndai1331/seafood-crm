using System.ComponentModel;

namespace Core.Enum
{
    public enum MarketingNotificationType
    {
        [Description("News")]
        News = 0,
        [Description("Service")]
        Service,
        [Description("Examination Result")]
        ExaminationResult,
        [Description("No Event")]
        NoEvent,
        [Description("URL")]
        URL,
    }
}
