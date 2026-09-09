using System;
using System.Collections.Generic;

namespace BootstrapBlazor.Server.Data.DomainPriceEvals
{
    public class PeerBenchmarkDto
    {
        public int SampleCount { get; set; }
        public List<PeerBenchmarkItemDto> Peers { get; set; } = new();
        public decimal? P25 { get; set; }
        public decimal? P50 { get; set; }
        public decimal? P75 { get; set; }
    }

    public class PeerBenchmarkItemDto
    {
        public string Domain { get; set; } = "";
        public decimal? PriceGp { get; set; }
        public decimal? PriceText { get; set; }
        public string? NccSource { get; set; }
        public string? Topic { get; set; }
        public int? Traffic { get; set; }
        public int? Dr { get; set; }
        public int? PurchaseYear { get; set; }
    }

    public class NccCatalogLookupDto
    {
        public bool Found { get; set; }
        public string Domain { get; set; } = "";
        public string? NccType { get; set; }
        public string? NccSource { get; set; }
        public int? NccDr { get; set; }
        public int? NccTraffic { get; set; }
        public decimal? NccPriceGp { get; set; }
        public decimal? NccPriceText { get; set; }
        public double? NegotiationScore { get; set; }
        public decimal? NegotiationTarget { get; set; }
        public decimal? NegotiationMarket { get; set; }
        public decimal? NegotiationReject { get; set; }
    }
}
