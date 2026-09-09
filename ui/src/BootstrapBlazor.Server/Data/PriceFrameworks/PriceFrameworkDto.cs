using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BootstrapBlazor.Server.Data.PriceFrameworks
{
    public class PriceFrameworkVersionDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public string TrafficBands { get; set; } = "[]";
        public string NicheMultipliers { get; set; } = "[]";
        public string DrMultipliers { get; set; } = "[]";
        public string CreatedBy { get; set; } = "";
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class CreatePriceFrameworkVersionDto
    {
        public string Name { get; set; } = "";
        public string? Description { get; set; }
        public string TrafficBands { get; set; } = "[]";
        public string NicheMultipliers { get; set; } = "[]";
        public string DrMultipliers { get; set; } = "[]";
    }

    // Helper classes for deserializing JSON columns
    public class TrafficBand
    {
        [JsonPropertyName("traffic_max")]
        public int? TrafficMax { get; set; }

        [JsonPropertyName("lo")]
        public decimal Lo { get; set; }

        [JsonPropertyName("hi")]
        public decimal Hi { get; set; }

        [JsonPropertyName("quality_label")]
        public string QualityLabel { get; set; } = "";

        [JsonPropertyName("quality_desc")]
        public string QualityDesc { get; set; } = "";
    }

    public class NicheMultiplier
    {
        [JsonPropertyName("niche")]
        public string Niche { get; set; } = "";

        [JsonPropertyName("label")]
        public string Label { get; set; } = "";

        [JsonPropertyName("mul")]
        public double Mul { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; } = "";
    }

    public class DrMultiplier
    {
        [JsonPropertyName("dr_max")]
        public int? DrMax { get; set; }

        [JsonPropertyName("mul")]
        public double Mul { get; set; }

        [JsonPropertyName("note")]
        public string Note { get; set; } = "";
    }
}
