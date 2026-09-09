using System;
using System.Collections.Generic;

namespace BootstrapBlazor.Server.Data.SeoBudgetEvals
{
    public class SeoBudgetEvalDto
    {
        public int Id { get; set; }
        public string DisplayId { get; set; } = null!;
        public string Brand { get; set; } = null!;
        public string PicUserId { get; set; } = null!;
        public string Month { get; set; } = null!;
        public decimal TotalCost { get; set; }
        public string Currency { get; set; } = "VNĐ";
        public string Notes { get; set; } = null!;
        public string Status { get; set; } = "draft";
        public int? FrameworkVersionId { get; set; }
        public string? FrameworkSnapshotJson { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public List<SeoBudgetEvalItemDto> Items { get; set; } = new List<SeoBudgetEvalItemDto>();
        public List<SeoBudgetEvalLogDto> Logs { get; set; } = new List<SeoBudgetEvalLogDto>();
    }

    public class SeoBudgetEvalItemDto
    {
        public int Id { get; set; }
        public int EvalId { get; set; }
        public string Domain { get; set; } = null!;
        public string AdType { get; set; } = "Guest Post";
        public string NicheType { get; set; } = "direct";
        public decimal Price { get; set; }
        public int AhrefsDr { get; set; }
        public int AhrefsTraffic { get; set; }
        public int AhrefsRefDomains { get; set; }
        public int AhrefsKeywords { get; set; }
        public decimal EvalHi { get; set; }
        public decimal EvalLo { get; set; }
        public string EvalLv { get; set; } = "low";
        public int RiskScore { get; set; }
        public string Status { get; set; } = null!;
        public string Note { get; set; } = "";
        /// <summary>UI-only: formatted display text for price input</summary>
        public string PriceText { get; set; } = "";
    }

    public class SeoBudgetEvalLogDto
    {
        public int Id { get; set; }
        public int EvalId { get; set; }
        public string Action { get; set; } = null!;
        public string UserId { get; set; } = null!;
        public string Note { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
    }
}
