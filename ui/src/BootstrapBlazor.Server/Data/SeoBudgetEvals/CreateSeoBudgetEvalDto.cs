using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BootstrapBlazor.Server.Data.SeoBudgetEvals
{
    public class CreateSeoBudgetEvalDto
    {
        [Required(ErrorMessage = "Thương hiệu là bắt buộc")]
        public string Brand { get; set; } = null!;
        [Required(ErrorMessage = "Tháng là bắt buộc")]
        public string Month { get; set; } = null!;
        [Required]
        public string PicUserId { get; set; } = null!;
        public decimal TotalCost { get; set; }
        public string Currency { get; set; } = "VNĐ";
        public string Notes { get; set; } = string.Empty;
        public int? FrameworkVersionId { get; set; }
        public string? FrameworkSnapshotJson { get; set; }
        
        // This validates if there are items, but we can do manual validation too
        public List<CreateSeoBudgetEvalItemDto> Items { get; set; } = new List<CreateSeoBudgetEvalItemDto>();
    }

    public class CreateSeoBudgetEvalItemDto
    {
        [Required(ErrorMessage = "Tên miền không được để trống")]
        public string Domain { get; set; } = null!;
        public string AdType { get; set; } = "Guest Post";
        public string NicheType { get; set; } = "direct";
        [Range(1, int.MaxValue, ErrorMessage = "Chi phí phải lớn hơn 0")]
        public decimal Price { get; set; }
        public int AhrefsDr { get; set; }
        public int AhrefsTraffic { get; set; }
        public int AhrefsRefDomains { get; set; }
        public int AhrefsKeywords { get; set; }
        public decimal EvalHi { get; set; }
        public decimal EvalLo { get; set; }
        public string EvalLv { get; set; } = "low";
        public int RiskScore { get; set; }
    }

    public class UpdateSeoBudgetEvalDto : CreateSeoBudgetEvalDto
    {
    }

    public class SeoBudgetEvalFilterDto : BaseFilterPagingDto
    {
        public string? Status { get; set; }
        public string? BrandC { get; set; }
        public string? Month { get; set; }
        public string? PicUserId { get; set; }
    }
}
