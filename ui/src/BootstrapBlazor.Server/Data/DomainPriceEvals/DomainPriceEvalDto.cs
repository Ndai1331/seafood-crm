using System;
using System.Collections.Generic;

namespace BootstrapBlazor.Server.Data.DomainPriceEvals
{
    public class DomainPriceEvalSessionDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string CreatedBy { get; set; } = "";
        public string Status { get; set; } = "draft";
        public int? FrameworkVersionId { get; set; }
        public string? FrameworkSnapshotJson { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<DomainPriceEvalItemDto> Items { get; set; } = new();
    }

    public class DomainPriceEvalItemDto
    {
        public int Id { get; set; }
        public int SessionId { get; set; }
        public string Domain { get; set; } = "";
        public int Dr { get; set; }
        public int Traffic { get; set; }
        public int TrafficSnapshot { get; set; }
        public DateTime? AhrefsSnapshotAt { get; set; }
        public int Keywords { get; set; }
        public int RefDomains { get; set; }
        public int UrlRating { get; set; }
        public double OrgCost { get; set; }
        public double DofollowPct { get; set; }
        public decimal Price { get; set; }
        public string Niche { get; set; } = "direct";
        public string Note { get; set; } = "";
        public decimal EvalLo { get; set; }
        public decimal EvalHi { get; set; }
        public int RiskScore { get; set; }
        public string EvalLv { get; set; } = "low";

        // Peer Benchmark
        public decimal? PeerP50 { get; set; }
        public int PeerSampleCount { get; set; }
        public decimal? NccCatalogPrice { get; set; }
        public decimal? RecMax { get; set; }
        public string BuyingSignal { get; set; } = "";
    }

    public class CreateDomainPriceEvalSessionDto
    {
        public string Name { get; set; } = "";
        public string Status { get; set; } = "draft";
        public int? FrameworkVersionId { get; set; }
        public string? FrameworkSnapshotJson { get; set; }
        public List<DomainPriceEvalItemDto> Items { get; set; } = new();
    }

    public class UpdateDomainPriceEvalSessionDto
    {
        public string Name { get; set; } = "";
        public string Status { get; set; } = "draft";
        public int? FrameworkVersionId { get; set; }
        public string? FrameworkSnapshotJson { get; set; }
        public List<DomainPriceEvalItemDto> Items { get; set; } = new();
    }
}
