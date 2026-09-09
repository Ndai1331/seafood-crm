namespace BootstrapBlazor.Server.Data
{
    /// <summary>
    /// Represents a Data Transfer Object for a cost type in the UI project.
    /// </summary>
    public class CostTypeDto
    {
        /// <summary>
        /// Unique identifier.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Code for the cost type.
        /// </summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Human-readable name.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Optional notes.
        /// </summary>
        public string Note { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO for creating a new cost type - matches API specification
    /// </summary>
    public class CreateCostTypeDto
    {
        /// <summary>
        /// Code for the cost type (required).
        /// </summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Human-readable name (optional).
        /// </summary>
        public string? Name { get; set; }
    }

    /// <summary>
    /// DTO for updating an existing cost type - matches API specification
    /// </summary>
    public class UpdateCostTypeDto
    {
        /// <summary>
        /// Unique identifier of the cost type to update (required).
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Code for the cost type (required).
        /// </summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Human-readable name (optional).
        /// </summary>
        public string? Name { get; set; }
    }
}