namespace Contract.Common
{
    /// <summary>
    /// DTO for bulk updating dates, OS, and PIC of IC SEO resources.
    /// Null fields are not updated (partial update semantics).
    /// </summary>
    public class BulkUpdateDatesDto
    {
        public List<long> Ids { get; set; } = new();
        public DateTime? OrderDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string? Pic { get; set; }
        public string? Os { get; set; }
    }
}
