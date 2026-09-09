namespace BootstrapBlazor.Server.Data
{
    /// <summary>
    /// Represents a Data Transfer Object for a brand code in the UI project.
    /// </summary>
    public class BrandCodeDto
    {
        /// <summary>
        /// Unique identifier.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Code for the brand code.
        /// </summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Human-readable name.
        /// </summary>
        public string Name { get; set; } = string.Empty;
        public DateTime? DeploymentDate { get; set; }
        public DateTime? DeploymentStopDate { get; set; }
    }

    /// <summary>
    /// DTO for creating a new brand code - matches API specification
    /// </summary>
    public class CreateBrandCodeDto
    {
        /// <summary>
        /// Code for the brand code (required).
        /// </summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Human-readable name (optional).
        /// </summary>
        public string? Name { get; set; }
        public DateTime? DeploymentDate { get; set; }
    }

    /// <summary>
    /// DTO for updating an existing brand code - matches API specification
    /// </summary>
    public class UpdateBrandCodeDto
    {
        /// <summary>
        /// Unique identifier of the brand code to update (required).
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Code for the brand code (required).
        /// </summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Human-readable name (optional).
        /// </summary>
        public string? Name { get; set; }
        public DateTime? DeploymentDate { get; set; }
        public DateTime? DeploymentStopDate { get; set; }
    }
}
