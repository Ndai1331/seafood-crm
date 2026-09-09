namespace BootstrapBlazor.Server.Data
{
    /// <summary>
    /// Represents a Data Transfer Object for a cost type step in the UI project.
    /// </summary>
    public class CostTypeStepDto
    {
        /// <summary>
        /// Unique identifier.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Cost type ID.
        /// </summary>
        public int CostTypeId { get; set; }

        /// <summary>
        /// Step name.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Step code.
        /// </summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Step unit.
        /// </summary>
        public string? Unit { get; set; }


        /// <summary>
        /// Step order/sequence.
        /// </summary>
        public int StepOrder { get; set; }

        /// <summary>
        /// Optional description.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Step type (BASE, MULTIPLIER, FIXED).
        /// </summary>
        public CostTypeStepType? StepType { get; set; }

        /// <summary>
        /// Value data type (INT, DECIMAL, ENUM, TEXT).
        /// </summary>
        public CostTypeStepValueDataType? ValueDataType { get; set; }

        /// <summary>
        /// Whether this step is required.
        /// </summary>
        public bool IsRequired { get; set; }

        /// <summary>
        /// Whether this step is active.
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Created date time.
        /// </summary>
        public DateTime? CreatedAt { get; set; }

        /// <summary>
        /// Updated date time.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Created by user ID.
        /// </summary>
        public int? CreatedBy { get; set; }

        /// <summary>
        /// Updated by user ID.
        /// </summary>
        public int? UpdatedBy { get; set; }

        /// <summary>
        /// List of options for this step.
        /// </summary>
        public List<CostTypeStepOptionDto>? Options { get; set; }
    }

    /// <summary>
    /// Filter DTO for CostTypeStep list query
    /// </summary>
    public class CostTypeStepFilterDto : BaseFilterPagingDto
    {
        /// <summary>
        /// Filter by cost type ID.
        /// </summary>
        public int? CostTypeId { get; set; }
    }

    /// <summary>
    /// DTO for creating a new cost type step - matches API specification
    /// </summary>
    public class CreateCostTypeStepDto
    {
        /// <summary>
        /// Cost type ID (required).
        /// </summary>
        public int CostTypeId { get; set; }

        /// <summary>
        /// Step name (required).
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Step order/sequence (optional).
        /// </summary>
        public int? StepOrder { get; set; }

        /// <summary>
        /// Optional description.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Step type (BASE, MULTIPLIER, FIXED).
        /// </summary>
        public CostTypeStepType? StepType { get; set; }

        /// <summary>
        /// Value data type (INT, DECIMAL, ENUM, TEXT).
        /// </summary>
        public CostTypeStepValueDataType? ValueDataType { get; set; }

        /// <summary>
        /// Whether this step is required.
        /// </summary>
        public bool IsRequired { get; set; }

        /// <summary>
        /// Whether this step is active.
        /// </summary>
        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// DTO for updating an existing cost type step - matches API specification
    /// </summary>
    public class UpdateCostTypeStepDto
    {
        /// <summary>
        /// Unique identifier of the cost type step to update (required).
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Cost type ID (required).
        /// </summary>
        public int CostTypeId { get; set; }

        /// <summary>
        /// Step name (required).
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Step order/sequence (optional).
        /// </summary>
        public int? StepOrder { get; set; }

        /// <summary>
        /// Optional description.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Step type (BASE, MULTIPLIER, FIXED).
        /// </summary>
        public CostTypeStepType? StepType { get; set; }

        /// <summary>
        /// Value data type (INT, DECIMAL, ENUM, TEXT).
        /// </summary>
        public CostTypeStepValueDataType? ValueDataType { get; set; }

        /// <summary>
        /// Whether this step is required.
        /// </summary>
        public bool IsRequired { get; set; }

        /// <summary>
        /// Whether this step is active.
        /// </summary>
        public bool IsActive { get; set; } = true;
    }
}

