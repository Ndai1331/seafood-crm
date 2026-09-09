namespace Core.Enum
{
    /// <summary>
    /// Approval workflow steps for SEO Payment Ticket
    /// </summary>
    public enum SeoPaymentTicketApprovalStep
    {
        Pending = 0,
        AIAutoCheck = 1,
        IC_Check = 2,
        Assistant_Review = 3,
        Manager_Approve = 4,
        Approved = 5,
        Rejected = 6,
        Processing = 7,
        Processed = 8
    }
}

