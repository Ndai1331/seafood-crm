using Domain.Seafood;

namespace Contract.Seafood
{
    public class CatalogLookupDto
    {
        public int Id { get; set; }
        public LookupCategory Category { get; set; }
        public string Code { get; set; } = "";
        public string Name { get; set; } = "";
        public string? Description { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
    }

    public class ProductGroupDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = "";
        public string Name { get; set; } = "";
        public string? DefaultMarket { get; set; }
        public List<ProductSkuDto> Skus { get; set; } = new();
    }

    public class ProductSkuDto
    {
        public int Id { get; set; }
        public int GroupId { get; set; }
        public string? GroupName { get; set; }
        public string Code { get; set; } = "";
        public string Name { get; set; } = "";
        public string? ExportMarket { get; set; }
        public decimal DefaultUnitPriceUsd { get; set; }
        public bool IsByproduct { get; set; }
        public bool IsActive { get; set; }
    }

    public class CustomerDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = "";
        public string Name { get; set; } = "";
        public string? ContactName { get; set; }
        public CustomerKind Kind { get; set; }
        public string? Purchaser { get; set; }
        public string? Note { get; set; }
        public bool IsActive { get; set; }
    }

    public class PaymentTermDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = "";
        public string Name { get; set; } = "";
        public string RatiosJson { get; set; } = "[]";
        public int? DueDays { get; set; }
        public bool IsLc { get; set; }
        public bool IsActive { get; set; }
    }

    public class InboundPurchaseDto
    {
        public int Id { get; set; }
        public int? CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerCode { get; set; }
        public string? ContractNo { get; set; }
        public string? Purchaser { get; set; }
        public string? MissingDocuments { get; set; }
        public DateTime? Eta { get; set; }
        public DateTime? WarehouseDate { get; set; }
        public string? BlNumber { get; set; }
        public string? ContainerNo { get; set; }
        public string? ContainerType { get; set; }
        public string? Note { get; set; }
        public decimal TotalKg { get; set; }
        public decimal TotalAmountUsd { get; set; }
        public List<InboundLineDto> Lines { get; set; } = new();
    }

    public class InboundLineDto
    {
        public int Id { get; set; }
        public string Commodity { get; set; } = "";
        public decimal QtyKgLongLine { get; set; }
        public decimal QtyKgHandline { get; set; }
        public decimal QtyKgPs { get; set; }
        public decimal QtyKgLand { get; set; }
        public decimal PriceUsd { get; set; }
        public decimal InvoiceAmountUsd { get; set; }
        public string? ContainerQty { get; set; }
    }

    public class ProductionLotDto
    {
        public int Id { get; set; }
        public string LotNumber { get; set; } = "";
        public int? InboundPurchaseId { get; set; }
        public DateTime ReceivedDate { get; set; }
        public decimal RawMaterialKg { get; set; }
        public decimal RecoveryRatio { get; set; }
        public string? Note { get; set; }
        public List<string> CertificateCodes { get; set; } = new();
        public List<ProductionOutputDto> Outputs { get; set; } = new();
    }

    public class ProductionOutputDto
    {
        public int Id { get; set; }
        public int SkuId { get; set; }
        public string? SkuName { get; set; }
        public decimal RecoveredKg { get; set; }
        public decimal RawUsedKg { get; set; }
        public decimal YieldRatio { get; set; }
        public string? ExportMarket { get; set; }
    }

    public class InventoryRowDto
    {
        public int Id { get; set; }
        public int LotId { get; set; }
        public string LotNumber { get; set; } = "";
        public int SkuId { get; set; }
        public string SkuName { get; set; } = "";
        public decimal OnHandKg { get; set; }
        public decimal AllocatedKg { get; set; }
        public decimal AvailableKg { get; set; }
        public List<string> Certificates { get; set; } = new();
        public string? Warning { get; set; }
    }

    public class SalesContractDto
    {
        public int Id { get; set; }
        public string ContractNo { get; set; } = "";
        public string? CustomerContractNo { get; set; }
        public int CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public int? MarketId { get; set; }
        public ContractStatus Status { get; set; }
        public decimal SuggestedUnitPriceUsd { get; set; }
        public decimal VarianceVsLastPct { get; set; }
        public decimal VarianceVsPeersPct { get; set; }
        public string? ApprovalNote { get; set; }
        public List<SalesContractLineDto> Lines { get; set; } = new();
    }

    public class SalesContractLineDto
    {
        public int SkuId { get; set; }
        public string? SkuName { get; set; }
        public decimal QtyKg { get; set; }
        public decimal QtyLbs { get; set; }
        public decimal UnitPriceUsd { get; set; }
        public decimal AmountUsd { get; set; }
    }

    public class PriceSuggestionDto
    {
        public decimal? LastPriceForCustomer { get; set; }
        public decimal? PeerAvgLastTwoMonths { get; set; }
        public string? LastContractNo { get; set; }
    }

    public class ExportShipmentDto
    {
        public int Id { get; set; }
        public int? SalesContractId { get; set; }
        public int? CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public string? ProductionNoticeNo { get; set; }
        public DateTime? PackingDate { get; set; }
        public DateTime? Etd { get; set; }
        public DateTime? Eta { get; set; }
        public string? InvoiceNo { get; set; }
        public int? PaymentTermId { get; set; }
        public string? PaymentTermName { get; set; }
        public int Cartons { get; set; }
        public decimal QtyLbs { get; set; }
        public decimal QtyKg { get; set; }
        public decimal AmountUsd { get; set; }
        public string? ContainerNo { get; set; }
        public string? ContainerType { get; set; }
        public string? Route { get; set; }
    }

    public class PaymentInstallmentDto
    {
        public int Id { get; set; }
        public int ExportShipmentId { get; set; }
        public string? InvoiceNo { get; set; }
        public string? CustomerName { get; set; }
        public int Sequence { get; set; }
        public decimal Ratio { get; set; }
        public decimal AmountUsd { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? ReceivedDate { get; set; }
        public decimal ReceivedAmountUsd { get; set; }
    }

    public class CustomerDepositDto
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public decimal AmountUsd { get; set; }
        public DateTime ReceivedDate { get; set; }
        public string? Note { get; set; }
        public decimal AllocatedUsd { get; set; }
        public List<DepositAllocationDto> Allocations { get; set; } = new();
    }

    public class DepositAllocationDto
    {
        public int Id { get; set; }
        public int? SalesContractId { get; set; }
        public string? ContractNo { get; set; }
        public decimal AmountUsd { get; set; }
        public bool IsExported { get; set; }
    }

    public class DashboardDto
    {
        public decimal InboundKgThisMonth { get; set; }
        public decimal ProducedKgThisMonth { get; set; }
        public decimal ExportKgThisMonth { get; set; }
        public decimal OnHandKg { get; set; }
        public decimal OpenContractUsd { get; set; }
        public decimal OutstandingPaymentUsd { get; set; }
        public List<PieSliceDto> StockBySku { get; set; } = new();
        public List<PieSliceDto> ExportByMarket { get; set; } = new();
    }

    public class PieSliceDto
    {
        public string Label { get; set; } = "";
        public decimal Value { get; set; }
    }
}
