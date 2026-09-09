namespace BootstrapBlazor.Server.Data
{
    /// <summary>
    /// SEO Payment Ticket History DTO
    /// </summary>
    public class SeoPaymentTicketHistoryDto
    {
        /// <summary>
        /// Unique identifier
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// SEO Payment Ticket Total ID
        /// </summary>
        public long TicketTotalId { get; set; }

        /// <summary>
        /// Action type (enum value)
        /// </summary>
        public int ActionType { get; set; }

        /// <summary>
        /// Action type name
        /// </summary>
        public string ActionTypeName { get; set; } = string.Empty;

        /// <summary>
        /// User ID who performed the action
        /// </summary>
        public int ActionById { get; set; }

        /// <summary>
        /// User name who performed the action
        /// </summary>
        public string? ActionByName { get; set; }

        /// <summary>
        /// Action timestamp
        /// </summary>
        public DateTime ActionAt { get; set; }

        /// <summary>
        /// Reason for the action
        /// </summary>
        public string? Reason { get; set; }

        /// <summary>
        /// Old status
        /// </summary>
        public int? OldStatus { get; set; }

        /// <summary>
        /// Old status name
        /// </summary>
        public string? OldStatusName { get; set; }

        /// <summary>
        /// Old approval step
        /// </summary>
        public int? OldStep { get; set; }

        /// <summary>
        /// Old approval step name
        /// </summary>
        public string? OldStepName { get; set; }

        /// <summary>
        /// New status
        /// </summary>
        public int? NewStatus { get; set; }

        /// <summary>
        /// New status name
        /// </summary>
        public string? NewStatusName { get; set; }

        /// <summary>
        /// New approval step
        /// </summary>
        public int? NewStep { get; set; }

        /// <summary>
        /// New approval step name
        /// </summary>
        public string? NewStepName { get; set; }

        /// <summary>
        /// Metadata (JSON string)
        /// </summary>
        public string? Metadata { get; set; }
    }

    /// <summary>
    /// Filter DTO for SEO Payment Ticket History list query
    /// </summary>
    public class SeoPaymentTicketHistoryFilterDto
    {
        /// <summary>
        /// Filter by SEO Payment Ticket Total ID
        /// </summary>
        public long? TicketTotalId { get; set; }

        /// <summary>
        /// Filter by action type
        /// </summary>
        public int? ActionType { get; set; }

        /// <summary>
        /// Filter by action by user ID
        /// </summary>
        public int? ActionById { get; set; }

        /// <summary>
        /// Filter by from date
        /// </summary>
        public DateTime? FromDate { get; set; }

        /// <summary>
        /// Filter by to date
        /// </summary>
        public DateTime? ToDate { get; set; }

        /// <summary>
        /// Page number (1-based)
        /// </summary>
        public int Page { get; set; } = 1;

        /// <summary>
        /// Page size
        /// </summary>
        public int PageSize { get; set; } = 20;
    }
}

