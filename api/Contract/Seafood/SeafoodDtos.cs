using Domain.Seafood;

namespace Contract.Seafood
{
    public class SeafoodPagedResult<T>
    {
        public List<T> Items { get; set; } = new();
        public int TotalCount { get; set; }
    }

    public class SeafoodSelectOptionDto
    {
        public int Id { get; set; }
        public string Text { get; set; } = "";
        public string? Description { get; set; }
    }

    public class SeafoodSelect2SearchResponseDto
    {
        public List<SeafoodSelectOptionDto> Results { get; set; } = new();
        public bool More { get; set; }
    }

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
        public int SortOrder { get; set; }
        public bool IsActive { get; set; } = true;
        public int SkuCount { get; set; }
        public List<ProductSkuDto> Skus { get; set; } = new();
    }

    public class MarketCertificateRuleDto
    {
        public int Id { get; set; }
        public string MarketCode { get; set; } = "";
        public int? RequiredDocumentTypeId { get; set; }
        public string? RequiredDocumentTypeCode { get; set; }
        public string? RequiredDocumentTypeName { get; set; }
        public int? SupplementalDocumentTypeId { get; set; }
        public string? SupplementalDocumentTypeCode { get; set; }
        public string? SupplementalDocumentTypeName { get; set; }
        public string? Note { get; set; }
        public bool IsActive { get; set; } = true;
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
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public string? CountryCode { get; set; }
        public string? TaxCode { get; set; }
        public string? BusinessRegistrationNo { get; set; }
        public CustomerKind Kind { get; set; }
        public string? Purchaser { get; set; }
        public string? Note { get; set; }
        public bool IsActive { get; set; }
    }

    public class BusinessPartnerDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = "";
        public string Name { get; set; } = "";
        public string? ContactName { get; set; }
        public string? TaxCode { get; set; }
        public string? Note { get; set; }
        public bool IsActive { get; set; } = true;
        public List<BusinessPartnerRole> Roles { get; set; } = new();
    }

    public class VesselDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = "";
        public string Name { get; set; } = "";
        public string? Flag { get; set; }
        public int? OwnerPartnerId { get; set; }
        public string? OwnerPartnerName { get; set; }
        public string? Note { get; set; }
        public bool IsActive { get; set; } = true;
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
        public int? SupplierPartnerId { get; set; }
        public string? SupplierPartnerName { get; set; }
        public int? VesselId { get; set; }
        public string? VesselName { get; set; }
        public string? CustomerCode { get; set; }
        public string? ContractNo { get; set; }
        public string? Purchaser { get; set; }
        public string? MissingDocuments { get; set; }
        public DateTime? Eta { get; set; }
        public DateTime? WarehouseDate { get; set; }
        public int? WarehouseId { get; set; }
        public int? TargetMarketId { get; set; }
        public int? PaymentTermId { get; set; }
        public DateTime? EstimatePaymentDate { get; set; }
        public string? BlNumber { get; set; }
        public string? PurchaseInvoiceNo { get; set; }
        public decimal? ActualWeightKg { get; set; }
        public decimal? ReceivedWeightKg { get; set; }
        public decimal ReleaseOrderFeeUsd { get; set; }
        public decimal ColdStorageFeeUsd { get; set; }
        public decimal HandlingFeeUsd { get; set; }
        public decimal CustomsFeeUsd { get; set; }
        public decimal InfrastructureFeeUsd { get; set; }
        public string? ContainerNo { get; set; }
        public string? ContainerType { get; set; }
        public string? Note { get; set; }
        public decimal TotalKg { get; set; }
        public decimal TotalAmountUsd { get; set; }
        public List<DocumentAttachmentDto> Documents { get; set; } = new();
        public List<InboundLineDto> Lines { get; set; } = new();
    }

    public class InboundLineDto
    {
        public int Id { get; set; }
        public string Commodity { get; set; } = "";
        public string? FishSpeciesCode { get; set; }
        public string? FishFormCode { get; set; }
        public string? SizeCode { get; set; }
        public string? SensoryCode { get; set; }
        public string? CatchMethodCode { get; set; }
        public string? FreezeMethodCode { get; set; }
        public string? OriginCode { get; set; }
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
        public string? ContainerQty { get; set; }
        public int? RawMaterialLotId { get; set; }
        public string? RawMaterialLotNumber { get; set; }
    }

    public class ProductionLotDto
    {
        public int Id { get; set; }
        public string LotNumber { get; set; } = "";
        public int? InboundPurchaseId { get; set; }
        public DateTime ReceivedDate { get; set; }
        public decimal RawMaterialKg { get; set; }
        public int? TargetMarketId { get; set; }
        public decimal RecoveryRatio { get; set; }
        public string? Note { get; set; }
        public List<string> CertificateCodes { get; set; } = new();
        public List<ProductionInputDto> Inputs { get; set; } = new();
        public List<DocumentAttachmentDto> Documents { get; set; } = new();
        public List<ProductionOutputDto> Outputs { get; set; } = new();
    }

    public class ProductionInputDto
    {
        public int RawMaterialLotId { get; set; }
        public string? RawMaterialLotNumber { get; set; }
        public decimal QuantityKg { get; set; }
        public string? Reason { get; set; }
    }

    public class RawMaterialLotDto
    {
        public int Id { get; set; }
        public string LotNumber { get; set; } = "";
        public int InboundPurchaseId { get; set; }
        public int InboundLineId { get; set; }
        public string? SupplierName { get; set; }
        public string? VesselName { get; set; }
        public DateTime ReceivedDate { get; set; }
        public decimal DeclaredKg { get; set; }
        public decimal ActualKg { get; set; }
        public decimal ConsumedKg { get; set; }
        public decimal AvailableKg { get; set; }
        public string? FishFormCode { get; set; }
        public string? SizeCode { get; set; }
        public string? CatchMethodCode { get; set; }
        public string? FreezeMethodCode { get; set; }
        public string? OriginCode { get; set; }
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
        public decimal? CostUsd { get; set; }
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
        public int? WarehouseId { get; set; }
        public List<string> Certificates { get; set; } = new();
        public string? Warning { get; set; }
        public List<InventoryMovementDto> Movements { get; set; } = new();
    }

    public class InventoryMovementDto
    {
        public long Id { get; set; }
        public InventoryMovementType MovementType { get; set; }
        public decimal QuantityKg { get; set; }
        public string ReferenceType { get; set; } = "";
        public int? ReferenceId { get; set; }
        public DateTime OccurredAt { get; set; }
        public string? Reason { get; set; }
    }

    public class InventoryAdjustmentDto
    {
        public int InventoryBalanceId { get; set; }
        public decimal QuantityKg { get; set; }
        public string Reason { get; set; } = "";
    }

    public class AllocationPreviewRequestDto
    {
        public int SalesContractId { get; set; }
        public DateTime? Etd { get; set; }
    }

    public class AllocationPreviewDto
    {
        public int SalesContractId { get; set; }
        public decimal RequiredKg { get; set; }
        public decimal AllocatableKg { get; set; }
        public decimal MissingKg { get; set; }
        public bool HasRequiredDocuments { get; set; }
        public List<AllocationCandidateDto> Candidates { get; set; } = new();
    }

    public class AllocationCandidateDto
    {
        public int ContractLineId { get; set; }
        public int InventoryBalanceId { get; set; }
        public int LotId { get; set; }
        public string LotNumber { get; set; } = "";
        public int SkuId { get; set; }
        public string SkuName { get; set; } = "";
        public decimal AvailableKg { get; set; }
        public decimal SuggestedKg { get; set; }
        public bool IsDocumentReady { get; set; }
        public List<string> RequiredDocuments { get; set; } = new();
        public List<string> SupplementalDocuments { get; set; } = new();
        public List<string> MissingDocuments { get; set; } = new();
    }

    public class AllocationConfirmDto
    {
        public int SalesContractId { get; set; }
        public DateTime? Etd { get; set; }
        public string? Reason { get; set; }
        public bool OverrideDocumentCheck { get; set; }
        public string? OverrideReason { get; set; }
        public List<AllocationItemDto> Items { get; set; } = new();
    }

    public class AllocationReleaseDto
    {
        public int SalesContractId { get; set; }
        public string? Reason { get; set; }
    }

    public class AllocationItemDto
    {
        public int SalesContractLineId { get; set; }
        public int InventoryBalanceId { get; set; }
        public decimal QuantityKg { get; set; }
    }

    public class AllocationResultDto
    {
        public int SalesContractId { get; set; }
        public decimal AllocatedKg { get; set; }
        public decimal MissingKg { get; set; }
        public List<AllocationItemDto> Items { get; set; } = new();
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
        public decimal AllocatedKg { get; set; }
    }

    public class SalesContractLineDto
    {
        public int SkuId { get; set; }
        public string? SkuName { get; set; }
        public decimal QtyKg { get; set; }
        public decimal QtyLbs { get; set; }
        public decimal UnitPriceUsd { get; set; }
        public decimal AmountUsd { get; set; }
        public decimal AllocatedKg { get; set; }
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
        public ShipmentStatus Status { get; set; }
        public List<ShipmentContainerDto> Containers { get; set; } = new();
        public List<DocumentAttachmentDto> Documents { get; set; } = new();
    }

    public class ShipmentContainerDto
    {
        public int Id { get; set; }
        public string ContainerNo { get; set; } = "";
        public string? ContainerType { get; set; }
        public string? SealNo { get; set; }
        public int Cartons { get; set; }
        public decimal QtyKg { get; set; }
        public decimal QtyLbs { get; set; }
        public DateTime? LoadingDate { get; set; }
        public bool IsConfirmed { get; set; }
    }

    public class DocumentTypeDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = "";
        public string Name { get; set; } = "";
        public string? Description { get; set; }
        public bool IsActive { get; set; }
    }

    public class DocumentAttachmentDto
    {
        public long Id { get; set; }
        public string OwnerType { get; set; } = "";
        public int OwnerId { get; set; }
        public int? DocumentTypeId { get; set; }
        public string? DocumentTypeCode { get; set; }
        public string? DocumentTypeName { get; set; }
        public string? DocumentNo { get; set; }
        public DateTime? IssueDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public DocumentStatus Status { get; set; }
        public string FileName { get; set; } = "";
        public long FileSize { get; set; }
        public DateTime UploadedAt { get; set; }
        public string? DownloadUrl { get; set; }
        public string? Note { get; set; }
    }

    public class DocumentChecklistDto
    {
        public string MarketCode { get; set; } = "";
        public DateTime EffectiveDate { get; set; }
        public List<DocumentChecklistItemDto> Items { get; set; } = new();
    }

    public class DocumentChecklistItemDto
    {
        public string DocumentCode { get; set; } = "";
        public string DocumentName { get; set; } = "";
        public bool IsRequired { get; set; }
        public bool IsValid { get; set; }
        public string? Reason { get; set; }
    }

    public class ImportPreviewDto
    {
        public int Id { get; set; }
        public string FileName { get; set; } = "";
        public ImportBatchStatus Status { get; set; }
        public int TotalRows { get; set; }
        public int ValidRows { get; set; }
        public int ErrorRows { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<ImportStagingRowDto> Rows { get; set; } = new();
    }

    public class ImportStagingRowDto
    {
        public long Id { get; set; }
        public string SheetName { get; set; } = "";
        public int RowNumber { get; set; }
        public string PayloadJson { get; set; } = "{}";
        public bool IsValid { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class ImportConfirmDto
    {
        public bool Confirm { get; set; }
        public string? Note { get; set; }
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

    public class TraceabilityDto
    {
        public string RootType { get; set; } = "";
        public long RootId { get; set; }
        public List<TraceabilityNodeDto> Nodes { get; set; } = new();
        public List<TraceabilityEdgeDto> Edges { get; set; } = new();
    }

    public class TraceabilityNodeDto
    {
        public string Type { get; set; } = "";
        public long Id { get; set; }
        public string Label { get; set; } = "";
    }

    public class TraceabilityEdgeDto
    {
        public string FromType { get; set; } = "";
        public long FromId { get; set; }
        public string ToType { get; set; } = "";
        public long ToId { get; set; }
        public decimal QuantityKg { get; set; }
        public string Relation { get; set; } = "";
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
