using System.ComponentModel;
namespace BootstrapBlazor.Server.Data;

public enum SeoPaymentTicketApprovalStep
{
    [Description("Tạo mới")]
    Create = 0,
    [Description("Hệ thống tự động duyệt")]
    PendingForAutoApprove = 1,
    [Description("IC duyệt")]
    IC_Check = 2,
    [Description("Assistant duyệt")]
    Assistant_Review = 3,
    [Description("Manager duyệt")]
    Manager_Approve = 4,
    [Description("Đã phê duyệt")]
    Approved = 5,
    [Description("Đã từ chối")]
    Rejected = 6,

    [Description("Triển khai")]
    Processing = 7,
    [Description("Hoàn thành")]
    Processed = 8
}