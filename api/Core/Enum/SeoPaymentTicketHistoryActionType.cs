namespace Core.Enum
{
    /// <summary>
    /// Action types for SEO Payment Ticket History
    /// </summary>
    public enum SeoPaymentTicketHistoryActionType
    {
        Created = 1,              // Phiếu được tạo mới
        AutoApproved = 2,          // Phiếu được duyệt tự động
        Rejected = 3,              // Phiếu bị từ chối
        StatusChanged = 4,         // Thay đổi trạng thái
        StepChanged = 5,           // Thay đổi bước duyệt
        Updated = 6,               // Cập nhật thông tin
        Commented = 7,             // Có comment mới
        Resubmitted = 8           // Resubmit sau khi bị reject
    }
}

