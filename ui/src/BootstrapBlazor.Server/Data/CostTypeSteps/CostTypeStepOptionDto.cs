using System.ComponentModel.DataAnnotations;

namespace BootstrapBlazor.Server.Data
{
    /// <summary>
    /// Represents a Data Transfer Object for a cost type step option in the UI project.
    /// </summary>
    public class CostTypeStepOptionDto
    {
        public long Id { get; set; }
        public long StepId { get; set; }
        public string? StepName { get; set; }
        public string Label { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal? ConditionMin { get; set; }
        public decimal? ConditionMax { get; set; }
        public decimal? BasePriceMin { get; set; }
        public decimal? BasePriceMax { get; set; }
        public decimal? MultiplierMin { get; set; }
        public decimal? MultiplierMax { get; set; }
        public decimal? FixedAmount { get; set; }
        public int SortOrder { get; set; }
        public bool IsDefault { get; set; }

        /// <summary>
        /// Whether this option is active.
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
    }

    /// <summary>
    /// Filter DTO for CostTypeStepOption list query
    /// </summary>
    public class CostTypeStepOptionFilterDto : BaseFilterPagingDto
    {
        /// <summary>
        /// Filter by step ID.
        /// </summary>
        public long? StepId { get; set; }
    }

    /// <summary>
    /// DTO for creating a new cost type step option - matches API specification
    /// </summary>
    public class CreateCostTypeStepOptionDto
    {
        [Required(ErrorMessage = "StepId is required")]
        public long StepId { get; set; }

        [Required(ErrorMessage = "Label is required")]
        [StringLength(255, ErrorMessage = "Label must not exceed 255 characters")]
        public string Label { get; set; } = string.Empty;

        public string? Description { get; set; }

        public decimal? ConditionMin { get; set; }

        public decimal? ConditionMax { get; set; }

        public decimal? BasePriceMin { get; set; }

        public decimal? BasePriceMax { get; set; }

        public decimal? MultiplierMin { get; set; }

        public decimal? MultiplierMax { get; set; }

        public decimal? FixedAmount { get; set; }

        public int SortOrder { get; set; } = 0;

        public bool IsDefault { get; set; } = false;

        /// <summary>
        /// Whether this option is active.
        /// </summary>
        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// DTO for updating an existing cost type step option - matches API specification
    /// </summary>
   public class UpdateCostTypeStepOptionDto
    {
        [Required(ErrorMessage = "Id is required")]
        public long Id { get; set; }

        [Required(ErrorMessage = "StepId is required")]
        public long StepId { get; set; }

        [Required(ErrorMessage = "Label is required")]
        [StringLength(255, ErrorMessage = "Label must not exceed 255 characters")]
        public string Label { get; set; } = string.Empty;

        public string? Description { get; set; }

        public decimal? ConditionMin { get; set; }

        public decimal? ConditionMax { get; set; }

        public decimal? BasePriceMin { get; set; }

        public decimal? BasePriceMax { get; set; }

        public decimal? MultiplierMin { get; set; }

        public decimal? MultiplierMax { get; set; }

        public decimal? FixedAmount { get; set; }

        public int SortOrder { get; set; } = 0;

        public bool IsDefault { get; set; } = false;

        /// <summary>
        /// Whether this option is active.
        /// </summary>
        public bool IsActive { get; set; } = true;
    }
}

