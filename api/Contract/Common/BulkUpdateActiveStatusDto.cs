namespace Contract.Common
{
    /// <summary>
    /// DTO for bulk updating active status of IC SEO resources
    /// </summary>
    public class BulkUpdateActiveStatusDto
    {
        public List<long> Ids { get; set; } = new();
        public bool IsActive { get; set; }
    }
}
