using System.ComponentModel.DataAnnotations;

namespace Domain.Seafood
{
    public enum LookupCategory
    {
        FishSpecies = 1,
        FishForm = 2,
        SizeGrade = 3,
        Sensory = 4,
        CatchMethod = 5,
        FreezeMethod = 6,
        Certificate = 7,
        Origin = 8,
        Market = 9,
        Warehouse = 10,
        Carrier = 11,
        ContainerType = 12
    }

    public enum CustomerKind
    {
        Buyer = 1,
        Supplier = 2,
        Both = 3
    }

    public enum ContractStatus
    {
        Draft = 0,
        PendingApproval = 1,
        Signed = 2,
        ReadyStock = 3,
        ReadyDocs = 4,
        Shipped = 5,
        Paid = 6,
        Cancelled = 9
    }

    public class CatalogLookup
    {
        public int Id { get; set; }
        public LookupCategory Category { get; set; }
        [MaxLength(64)] public string Code { get; set; } = string.Empty;
        [MaxLength(256)] public string Name { get; set; } = string.Empty;
        [MaxLength(512)] public string? Description { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class MarketCertificateRule
    {
        public int Id { get; set; }
        [MaxLength(64)] public string MarketCode { get; set; } = string.Empty;
        [MaxLength(64)] public string? MustHaveCertificateCode { get; set; }
        [MaxLength(64)] public string? AddonCertificateCode { get; set; }
        [MaxLength(256)] public string? Note { get; set; }
    }

    public class ProductGroup
    {
        public int Id { get; set; }
        [MaxLength(64)] public string Code { get; set; } = string.Empty;
        [MaxLength(256)] public string Name { get; set; } = string.Empty;
        [MaxLength(64)] public string? DefaultMarket { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; } = true;
        public List<ProductSku> Skus { get; set; } = new();
    }

    public class ProductSku
    {
        public int Id { get; set; }
        public int GroupId { get; set; }
        public ProductGroup? Group { get; set; }
        [MaxLength(128)] public string Code { get; set; } = string.Empty;
        [MaxLength(256)] public string Name { get; set; } = string.Empty;
        [MaxLength(64)] public string? ExportMarket { get; set; }
        public decimal DefaultUnitPriceUsd { get; set; }
        public bool IsByproduct { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class PaymentTerm
    {
        public int Id { get; set; }
        [MaxLength(64)] public string Code { get; set; } = string.Empty;
        [MaxLength(256)] public string Name { get; set; } = string.Empty;
        /// <summary>JSON array of ratios, e.g. [0.2, 0.8] for TT 20+80.</summary>
        [MaxLength(256)] public string RatiosJson { get; set; } = "[]";
        public int? DueDays { get; set; }
        public bool IsLc { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class Customer
    {
        public int Id { get; set; }
        [MaxLength(64)] public string Code { get; set; } = string.Empty;
        [MaxLength(256)] public string Name { get; set; } = string.Empty;
        [MaxLength(256)] public string? ContactName { get; set; }
        public CustomerKind Kind { get; set; } = CustomerKind.Buyer;
        [MaxLength(128)] public string? Purchaser { get; set; }
        [MaxLength(512)] public string? Note { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
