namespace BootstrapBlazor.Server.Data
{
    /// <summary>
    /// SEO Payment Ticket Comment DTO
    /// </summary>
    public class SeoPaymentTicketCommentDto
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
        /// Comment content
        /// </summary>
        public string Comment { get; set; } = string.Empty;

        /// <summary>
        /// User ID who created the comment
        /// </summary>
        public int CreatedById { get; set; }

        /// <summary>
        /// User name who created the comment
        /// </summary>
        public string? CreatedByName { get; set; }

        /// <summary>
        /// Created timestamp
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Updated timestamp
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Is comment deleted
        /// </summary>
        public bool IsDeleted { get; set; }
    }

    /// <summary>
    /// DTO for creating a new SEO Payment Ticket Comment
    /// </summary>
    public class CreateSeoPaymentTicketCommentDto
    {
        /// <summary>
        /// SEO Payment Ticket Total ID (required)
        /// </summary>
        public long TicketTotalId { get; set; }

        /// <summary>
        /// Comment content (required)
        /// </summary>
        public string Comment { get; set; } = string.Empty;

        /// <summary>
        /// Created by user ID (required)
        /// </summary>
        public int CreatedById { get; set; }
    }

    /// <summary>
    /// DTO for updating an existing SEO Payment Ticket Comment
    /// </summary>
    public class UpdateSeoPaymentTicketCommentDto
    {
        /// <summary>
        /// Comment content (required)
        /// </summary>
        public string Comment { get; set; } = string.Empty;
    }

    /// <summary>
    /// Filter DTO for SEO Payment Ticket Comment list query
    /// </summary>
    public class SeoPaymentTicketCommentFilterDto
    {
        /// <summary>
        /// Filter by SEO Payment Ticket Total ID
        /// </summary>
        public long? TicketTotalId { get; set; }

        /// <summary>
        /// Filter by created by user ID
        /// </summary>
        public int? CreatedById { get; set; }

        /// <summary>
        /// Include deleted comments
        /// </summary>
        public bool? IncludeDeleted { get; set; } = false;

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

