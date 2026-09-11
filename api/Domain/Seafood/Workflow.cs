using System.ComponentModel.DataAnnotations;

namespace Domain.Seafood;

public enum BusinessPartnerRole
{
    Buyer = 1,
    Supplier = 2,
    VesselOwner = 3,
    Forwarder = 4,
    Carrier = 5,
    Transporter = 6,
    Warehouse = 7
}

public enum StockLotKind
{
    RawMaterial = 1,
    FinishedGoods = 2,
    Byproduct = 3
}

public enum InventoryMovementType
{
    Receipt = 1,
    ProductionConsumption = 2,
    ProductionOutput = 3,
    Reservation = 4,
    ReservationRelease = 5,
    Shipment = 6,
    TransferIn = 7,
    TransferOut = 8,
    Adjustment = 9
}

public enum DocumentStatus
{
    Pending = 1,
    Verified = 2,
    Rejected = 3,
    Expired = 4
}

public enum ReservationStatus
{
    Active = 1,
    Released = 2,
    Fulfilled = 3,
    Cancelled = 4
}

public enum ShipmentStatus
{
    Draft = 0,
    Ready = 1,
    Confirmed = 2,
    Shipped = 3,
    Cancelled = 9
}

public enum InvoiceStatus
{
    Draft = 0,
    Issued = 1,
    PartiallyPaid = 2,
    Paid = 3,
    Cancelled = 9
}

public enum ImportBatchStatus
{
    Preview = 1,
    Confirmed = 2,
    Rejected = 3
}

public class BusinessPartner
{
    public int Id { get; set; }
    [MaxLength(64)] public string Code { get; set; } = string.Empty;
    [MaxLength(256)] public string Name { get; set; } = string.Empty;
    [MaxLength(256)] public string? ContactName { get; set; }
    [MaxLength(64)] public string? TaxCode { get; set; }
    [MaxLength(512)] public string? Note { get; set; }
    public bool IsActive { get; set; } = true;
    public List<BusinessPartnerRoleLink> Roles { get; set; } = new();
}

public class BusinessPartnerRoleLink
{
    public int Id { get; set; }
    public int BusinessPartnerId { get; set; }
    public BusinessPartner? BusinessPartner { get; set; }
    public BusinessPartnerRole Role { get; set; }
}

public class Vessel
{
    public int Id { get; set; }
    [MaxLength(64)] public string Code { get; set; } = string.Empty;
    [MaxLength(256)] public string Name { get; set; } = string.Empty;
    [MaxLength(64)] public string? Flag { get; set; }
    public int? OwnerPartnerId { get; set; }
    public BusinessPartner? OwnerPartner { get; set; }
    [MaxLength(512)] public string? Note { get; set; }
    public bool IsActive { get; set; } = true;
}

public class RawMaterialLot
{
    public int Id { get; set; }
    [MaxLength(64)] public string LotNumber { get; set; } = string.Empty;
    public int InboundPurchaseId { get; set; }
    public InboundPurchase? InboundPurchase { get; set; }
    public int InboundLineId { get; set; }
    public InboundLine? InboundLine { get; set; }
    public int? SupplierPartnerId { get; set; }
    public BusinessPartner? SupplierPartner { get; set; }
    public int? VesselId { get; set; }
    public Vessel? Vessel { get; set; }
    public DateTime ReceivedDate { get; set; } = DateTime.UtcNow;
    public int? WarehouseId { get; set; }
    public int? TargetMarketId { get; set; }
    [MaxLength(64)] public string? FishSpeciesCode { get; set; }
    [MaxLength(64)] public string? FishFormCode { get; set; }
    [MaxLength(64)] public string? SizeCode { get; set; }
    [MaxLength(64)] public string? SensoryCode { get; set; }
    [MaxLength(64)] public string? CatchMethodCode { get; set; }
    [MaxLength(64)] public string? FreezeMethodCode { get; set; }
    [MaxLength(64)] public string? OriginCode { get; set; }
    public decimal DeclaredKg { get; set; }
    public decimal ActualKg { get; set; }
    [MaxLength(512)] public string? Note { get; set; }
    public List<ProductionInput> ProductionInputs { get; set; } = new();
    public List<DocumentAttachment> Documents { get; set; } = new();
}

public class ProductionInput
{
    public int Id { get; set; }
    public int ProductionLotId { get; set; }
    public ProductionLot? ProductionLot { get; set; }
    public int RawMaterialLotId { get; set; }
    public RawMaterialLot? RawMaterialLot { get; set; }
    public decimal QuantityKg { get; set; }
    [MaxLength(512)] public string? Reason { get; set; }
}

public class InventoryMovement
{
    public long Id { get; set; }
    public int InventoryBalanceId { get; set; }
    public InventoryBalance? InventoryBalance { get; set; }
    public InventoryMovementType MovementType { get; set; }
    public decimal QuantityKg { get; set; }
    [MaxLength(64)] public string ReferenceType { get; set; } = string.Empty;
    public int? ReferenceId { get; set; }
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    public int? CreatedByUserId { get; set; }
    [MaxLength(512)] public string? Reason { get; set; }
}

public class StockReservation
{
    public long Id { get; set; }
    public int SalesContractLineId { get; set; }
    public SalesContractLine? SalesContractLine { get; set; }
    public int InventoryBalanceId { get; set; }
    public InventoryBalance? InventoryBalance { get; set; }
    public decimal QuantityKg { get; set; }
    public ReservationStatus Status { get; set; } = ReservationStatus.Active;
    public int? ShipmentId { get; set; }
    public ExportShipment? Shipment { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int? CreatedByUserId { get; set; }
    public DateTime? ReleasedAt { get; set; }
    [MaxLength(512)] public string? Reason { get; set; }
}

public class SalesAllocation
{
    public long Id { get; set; }
    public int SalesContractLineId { get; set; }
    public SalesContractLine? SalesContractLine { get; set; }
    public int InventoryBalanceId { get; set; }
    public InventoryBalance? InventoryBalance { get; set; }
    public int? ShipmentId { get; set; }
    public ExportShipment? Shipment { get; set; }
    public decimal QuantityKg { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? CancelledAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class TraceabilityLink
{
    public long Id { get; set; }
    [MaxLength(64)] public string FromType { get; set; } = string.Empty;
    public int FromId { get; set; }
    [MaxLength(64)] public string ToType { get; set; } = string.Empty;
    public int ToId { get; set; }
    public decimal QuantityKg { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class DocumentType
{
    public int Id { get; set; }
    [MaxLength(64)] public string Code { get; set; } = string.Empty;
    [MaxLength(256)] public string Name { get; set; } = string.Empty;
    [MaxLength(512)] public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}

public class DocumentAttachment
{
    public long Id { get; set; }
    [MaxLength(64)] public string OwnerType { get; set; } = string.Empty;
    public int OwnerId { get; set; }
    public int? DocumentTypeId { get; set; }
    public DocumentType? DocumentType { get; set; }
    [MaxLength(128)] public string? DocumentNo { get; set; }
    public DateTime? IssueDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public DocumentStatus Status { get; set; } = DocumentStatus.Pending;
    [MaxLength(512)] public string ObjectKey { get; set; } = string.Empty;
    [MaxLength(256)] public string FileName { get; set; } = string.Empty;
    [MaxLength(128)] public string? ContentType { get; set; }
    public long FileSize { get; set; }
    public int? UploadedByUserId { get; set; }
    public int? VerifiedByUserId { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    public DateTime? VerifiedAt { get; set; }
    [MaxLength(512)] public string? Note { get; set; }
}

public class DocumentRequirement
{
    public int Id { get; set; }
    [MaxLength(64)] public string MarketCode { get; set; } = string.Empty;
    public int DocumentTypeId { get; set; }
    public DocumentType? DocumentType { get; set; }
    public bool IsRequired { get; set; }
    public int SortOrder { get; set; }
    [MaxLength(256)] public string? Note { get; set; }
}

public class ShipmentContainer
{
    public int Id { get; set; }
    public int ShipmentId { get; set; }
    public ExportShipment? Shipment { get; set; }
    [MaxLength(64)] public string ContainerNo { get; set; } = string.Empty;
    [MaxLength(32)] public string? ContainerType { get; set; }
    [MaxLength(64)] public string? SealNo { get; set; }
    public int Cartons { get; set; }
    public decimal QtyKg { get; set; }
    public decimal QtyLbs { get; set; }
    public DateTime? LoadingDate { get; set; }
    public bool IsConfirmed { get; set; }
}

public class ShipmentDocument
{
    public int Id { get; set; }
    public int ShipmentId { get; set; }
    public ExportShipment? Shipment { get; set; }
    public long DocumentAttachmentId { get; set; }
    public DocumentAttachment? DocumentAttachment { get; set; }
    public bool IsRequired { get; set; }
}

public class SalesInvoice
{
    public int Id { get; set; }
    [MaxLength(64)] public string InvoiceNo { get; set; } = string.Empty;
    public int? SalesContractId { get; set; }
    public SalesContract? SalesContract { get; set; }
    public int? ShipmentId { get; set; }
    public ExportShipment? Shipment { get; set; }
    public int? CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public decimal AmountUsd { get; set; }
    public DateTime IssueDate { get; set; } = DateTime.UtcNow;
    public DateTime? DueDate { get; set; }
    public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft;
}

public class PaymentTransaction
{
    public long Id { get; set; }
    public int? CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public int? InvoiceId { get; set; }
    public SalesInvoice? Invoice { get; set; }
    [MaxLength(64)] public string? ReferenceNo { get; set; }
    public decimal AmountUsd { get; set; }
    public DateTime ReceivedDate { get; set; } = DateTime.UtcNow;
    [MaxLength(512)] public string? Note { get; set; }
    public List<PaymentAllocation> Allocations { get; set; } = new();
}

public class PaymentAllocation
{
    public long Id { get; set; }
    public long PaymentTransactionId { get; set; }
    public PaymentTransaction? PaymentTransaction { get; set; }
    public int? PaymentInstallmentId { get; set; }
    public PaymentInstallment? PaymentInstallment { get; set; }
    public int? InvoiceId { get; set; }
    public SalesInvoice? Invoice { get; set; }
    public decimal AmountUsd { get; set; }
}

public class ExchangeRateSnapshot
{
    public int Id { get; set; }
    [MaxLength(16)] public string FromCurrency { get; set; } = "USD";
    [MaxLength(16)] public string ToCurrency { get; set; } = "VND";
    public decimal Rate { get; set; }
    public DateTime EffectiveDate { get; set; } = DateTime.UtcNow;
    [MaxLength(256)] public string? Source { get; set; }
}

public class DomainAuditLog
{
    public long Id { get; set; }
    [MaxLength(64)] public string EntityType { get; set; } = string.Empty;
    public int EntityId { get; set; }
    [MaxLength(32)] public string Action { get; set; } = string.Empty;
    public int? UserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? ChangesJson { get; set; }
    [MaxLength(512)] public string? Reason { get; set; }
}

public class ImportBatch
{
    public int Id { get; set; }
    [MaxLength(256)] public string FileName { get; set; } = string.Empty;
    [MaxLength(512)] public string? SourceObjectKey { get; set; }
    public ImportBatchStatus Status { get; set; } = ImportBatchStatus.Preview;
    public int TotalRows { get; set; }
    public int ValidRows { get; set; }
    public int ErrorRows { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int? CreatedByUserId { get; set; }
    public List<ImportStagingRow> Rows { get; set; } = new();
}

public class ImportStagingRow
{
    public long Id { get; set; }
    public int ImportBatchId { get; set; }
    public ImportBatch? ImportBatch { get; set; }
    [MaxLength(128)] public string SheetName { get; set; } = string.Empty;
    public int RowNumber { get; set; }
    public string PayloadJson { get; set; } = "{}";
    public bool IsValid { get; set; }
    [MaxLength(1024)] public string? ErrorMessage { get; set; }
}
