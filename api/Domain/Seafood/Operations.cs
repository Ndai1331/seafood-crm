using System.ComponentModel.DataAnnotations;

namespace Domain.Seafood
{
    public class InboundPurchase
    {
        public int Id { get; set; }
        public int? CustomerId { get; set; }
        public Customer? Customer { get; set; }
        public int? SupplierPartnerId { get; set; }
        public BusinessPartner? SupplierPartner { get; set; }
        public int? VesselId { get; set; }
        public Vessel? Vessel { get; set; }
        [MaxLength(64)] public string? CustomerCode { get; set; }
        [MaxLength(128)] public string? ContractNo { get; set; }
        [MaxLength(128)] public string? Purchaser { get; set; }
        [MaxLength(256)] public string? MissingDocuments { get; set; }
        public DateTime? Eta { get; set; }
        public DateTime? WarehouseDate { get; set; }
        public int? WarehouseId { get; set; }
        public int? TargetMarketId { get; set; }
        public int? PaymentTermId { get; set; }
        public DateTime? EstimatePaymentDate { get; set; }
        [MaxLength(128)] public string? BlNumber { get; set; }
        [MaxLength(128)] public string? PurchaseInvoiceNo { get; set; }
        public decimal? ActualWeightKg { get; set; }
        public decimal? ReceivedWeightKg { get; set; }
        public decimal ReleaseOrderFeeUsd { get; set; }
        public decimal ColdStorageFeeUsd { get; set; }
        public decimal HandlingFeeUsd { get; set; }
        public decimal CustomsFeeUsd { get; set; }
        public decimal InfrastructureFeeUsd { get; set; }
        [MaxLength(64)] public string? ContainerNo { get; set; }
        [MaxLength(32)] public string? ContainerType { get; set; }
        [MaxLength(512)] public string? Note { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public List<InboundLine> Lines { get; set; } = new();
        public List<DocumentAttachment> Documents { get; set; } = new();
    }

    public class InboundLine
    {
        public int Id { get; set; }
        public int PurchaseId { get; set; }
        public InboundPurchase? Purchase { get; set; }
        [MaxLength(256)] public string Commodity { get; set; } = string.Empty;
        [MaxLength(64)] public string? FishSpeciesCode { get; set; }
        [MaxLength(64)] public string? FishFormCode { get; set; }
        [MaxLength(64)] public string? SizeCode { get; set; }
        [MaxLength(64)] public string? SensoryCode { get; set; }
        [MaxLength(64)] public string? CatchMethodCode { get; set; }
        [MaxLength(64)] public string? FreezeMethodCode { get; set; }
        [MaxLength(64)] public string? OriginCode { get; set; }
        public decimal QtyKg { get; set; }
        public decimal ActualWeightKg { get; set; }
        public decimal QtyKgLongLine { get; set; }
        public decimal QtyKgHandline { get; set; }
        public decimal QtyKgPs { get; set; }
        public decimal QtyKgLand { get; set; }
        public decimal MahiKg { get; set; }
        public decimal FinishedLbs { get; set; }
        public decimal PriceUsd { get; set; }
        public decimal InvoiceAmountUsd { get; set; }
        [MaxLength(32)] public string? ContainerQty { get; set; }
        public RawMaterialLot? RawMaterialLot { get; set; }
    }

    public class ProductionLot
    {
        public int Id { get; set; }
        [MaxLength(64)] public string LotNumber { get; set; } = string.Empty;
        public int? InboundPurchaseId { get; set; }
        public InboundPurchase? InboundPurchase { get; set; }
        public DateTime ReceivedDate { get; set; } = DateTime.UtcNow;
        public decimal RawMaterialKg { get; set; }
        public int? TargetMarketId { get; set; }
        [MaxLength(512)] public string? Note { get; set; }
        public List<ProductionOutput> Outputs { get; set; } = new();
        public List<LotCertificate> Certificates { get; set; } = new();
        public List<ProductionInput> Inputs { get; set; } = new();
        public List<DocumentAttachment> Documents { get; set; } = new();
    }

    public class ProductionOutput
    {
        public int Id { get; set; }
        public int LotId { get; set; }
        public ProductionLot? Lot { get; set; }
        public int SkuId { get; set; }
        public ProductSku? Sku { get; set; }
        public decimal RecoveredKg { get; set; }
        public decimal RawUsedKg { get; set; }
        public decimal YieldRatio { get; set; }
        [MaxLength(64)] public string? ExportMarket { get; set; }
        public decimal? CostUsd { get; set; }
    }

    public class LotCertificate
    {
        public int Id { get; set; }
        public int LotId { get; set; }
        public ProductionLot? Lot { get; set; }
        [MaxLength(64)] public string CertificateCode { get; set; } = string.Empty;
    }

    public class InventoryBalance
    {
        public int Id { get; set; }
        public int LotId { get; set; }
        public ProductionLot? Lot { get; set; }
        public int SkuId { get; set; }
        public ProductSku? Sku { get; set; }
        public decimal OnHandKg { get; set; }
        public decimal AllocatedKg { get; set; }
        public int? WarehouseId { get; set; }
        public decimal AvailableKg => OnHandKg - AllocatedKg;
        public List<InventoryMovement> Movements { get; set; } = new();
        public List<StockReservation> Reservations { get; set; } = new();
    }

    public class SalesContract
    {
        public int Id { get; set; }
        [MaxLength(64)] public string ContractNo { get; set; } = string.Empty;
        [MaxLength(64)] public string? CustomerContractNo { get; set; }
        public int CustomerId { get; set; }
        public Customer? Customer { get; set; }
        public int? MarketId { get; set; }
        public ContractStatus Status { get; set; } = ContractStatus.Draft;
        public decimal SuggestedUnitPriceUsd { get; set; }
        public decimal VarianceVsLastPct { get; set; }
        public decimal VarianceVsPeersPct { get; set; }
        [MaxLength(1024)] public string? ApprovalNote { get; set; }
        public int? ApprovedBy { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public List<SalesContractLine> Lines { get; set; } = new();
        public List<SalesInvoice> Invoices { get; set; } = new();
    }

    public class SalesContractLine
    {
        public int Id { get; set; }
        public int ContractId { get; set; }
        public SalesContract? Contract { get; set; }
        public int SkuId { get; set; }
        public ProductSku? Sku { get; set; }
        public decimal QtyKg { get; set; }
        public decimal QtyLbs { get; set; }
        public decimal UnitPriceUsd { get; set; }
        public decimal AmountUsd { get; set; }
        public decimal AllocatedKg { get; set; }
    }

    public class ExportShipment
    {
        public int Id { get; set; }
        public int? SalesContractId { get; set; }
        public SalesContract? SalesContract { get; set; }
        public int? CustomerId { get; set; }
        public Customer? Customer { get; set; }
        [MaxLength(64)] public string? ProductionNoticeNo { get; set; }
        public DateTime? PackingDate { get; set; }
        public DateTime? Etd { get; set; }
        public DateTime? Eta { get; set; }
        [MaxLength(64)] public string? InvoiceNo { get; set; }
        public int? PaymentTermId { get; set; }
        public PaymentTerm? PaymentTerm { get; set; }
        public int Cartons { get; set; }
        public decimal QtyLbs { get; set; }
        public decimal QtyKg { get; set; }
        public decimal AmountUsd { get; set; }
        [MaxLength(64)] public string? ContainerNo { get; set; }
        [MaxLength(32)] public string? ContainerType { get; set; }
        public int? CarrierId { get; set; }
        [MaxLength(256)] public string? Route { get; set; }
        [MaxLength(512)] public string? Note { get; set; }
        public ShipmentStatus Status { get; set; } = ShipmentStatus.Draft;
        public List<ShipmentContainer> Containers { get; set; } = new();
        public List<StockReservation> Reservations { get; set; } = new();
        public List<SalesAllocation> Allocations { get; set; } = new();
        public List<ShipmentDocument> Documents { get; set; } = new();
        public List<SalesInvoice> Invoices { get; set; } = new();
    }

    public class PaymentInstallment
    {
        public int Id { get; set; }
        public int ExportShipmentId { get; set; }
        public ExportShipment? ExportShipment { get; set; }
        public int Sequence { get; set; }
        public decimal Ratio { get; set; }
        public decimal AmountUsd { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? ReceivedDate { get; set; }
        public decimal ReceivedAmountUsd { get; set; }
        public int? SalesInvoiceId { get; set; }
        public SalesInvoice? SalesInvoice { get; set; }
    }

    public class CustomerDeposit
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public Customer? Customer { get; set; }
        public decimal AmountUsd { get; set; }
        public DateTime ReceivedDate { get; set; } = DateTime.UtcNow;
        [MaxLength(1024)] public string? Note { get; set; }
        public List<DepositAllocation> Allocations { get; set; } = new();
    }

    public class DepositAllocation
    {
        public int Id { get; set; }
        public int DepositId { get; set; }
        public CustomerDeposit? Deposit { get; set; }
        public int? SalesContractId { get; set; }
        public SalesContract? SalesContract { get; set; }
        public decimal AmountUsd { get; set; }
        public bool IsExported { get; set; }
        public DateTime? ExportedAt { get; set; }
    }
}
