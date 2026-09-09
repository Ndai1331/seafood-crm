using System.ComponentModel;

namespace Core.Enum
{
    public enum NotificationType
    {
        [Description("Dự án")]
        Project = 0,
        [Description("Tài liệu")]
        Document = 1,
        [Description("Nhiệm vụ")]
        Task = 2,
        [Description("Không xác định")]
        Unknown = 3,
        [Description("Appointment")]
        Appointment = 4,
        [Description("Examination Result")]
        ExaminationResult = 5,
        [Description("Service")]
        Service = 6,
        [Description("News")]
        News = 7,
        [Description("URL")]
        URL = 8,
        [Description("Sign")]
        Sign = 9,
    }
}
