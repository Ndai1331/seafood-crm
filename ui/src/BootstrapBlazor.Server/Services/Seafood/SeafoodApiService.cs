using BootstrapBlazor.Components;
using BootstrapBlazor.Server.Http;
using Microsoft.AspNetCore.Components.Forms;

namespace BootstrapBlazor.Server.Services.Seafood;

public class SeafoodApiService
{
    public Task<DashboardDto?> DashboardAsync() => RequestClient.GetAPIAsync<DashboardDto>("seafood/dashboard");
    public Task<List<CatalogLookupDto>?> CatalogAsync(int? category = null)
        => RequestClient.GetAPIAsync<List<CatalogLookupDto>>(category == null ? "seafood/catalog" : $"seafood/catalog?category={category}");
    public Task<SeafoodPagedResult<CatalogLookupDto>?> CatalogPageAsync(int? category, string? search, int skip, int take)
        => RequestClient.GetAPIAsync<SeafoodPagedResult<CatalogLookupDto>>(PageEndpoint("seafood/catalog/page", search, skip, take, category));
    public Task<SeafoodSelect2SearchResponseDto?> SearchLookupOptionsAsync(int category, string? search, int page = 1, int pageSize = 20)
        => SearchOptionsAsync($"seafood/select-options/lookups/{category}", search, page, pageSize);
    public Task<CatalogLookupDto?> SaveCatalogAsync(CatalogLookupDto dto) => RequestClient.PostAPIAsync<CatalogLookupDto>("seafood/catalog", dto);
    public Task<bool> DeleteCatalogAsync(int id) => RequestClient.DeleteAPIAsync<bool>($"seafood/catalog/{id}");
    public Task<List<MarketCertificateRuleDto>?> MarketDocumentRulesAsync() => RequestClient.GetAPIAsync<List<MarketCertificateRuleDto>>("seafood/market-document-rules");
    public Task<SeafoodPagedResult<MarketCertificateRuleDto>?> MarketDocumentRulesPageAsync(string? search, int skip, int take)
        => RequestClient.GetAPIAsync<SeafoodPagedResult<MarketCertificateRuleDto>>(PageEndpoint("seafood/market-document-rules/page", search, skip, take));
    public Task<MarketCertificateRuleDto?> SaveMarketDocumentRuleAsync(MarketCertificateRuleDto dto) => RequestClient.PostAPIAsync<MarketCertificateRuleDto>("seafood/market-document-rules", dto);
    public Task<bool> DeleteMarketDocumentRuleAsync(int id) => RequestClient.DeleteAPIAsync<bool>($"seafood/market-document-rules/{id}");
    public Task<List<ProductGroupDto>?> ProductsAsync() => RequestClient.GetAPIAsync<List<ProductGroupDto>>("seafood/products");
    public Task<SeafoodPagedResult<ProductGroupDto>?> ProductsPageAsync(string? search, int skip, int take)
        => RequestClient.GetAPIAsync<SeafoodPagedResult<ProductGroupDto>>(PageEndpoint("seafood/products/page", search, skip, take));
    public Task<SeafoodPagedResult<ProductSkuDto>?> ProductSkusPageAsync(int groupId, string? search, int skip, int take)
        => RequestClient.GetAPIAsync<SeafoodPagedResult<ProductSkuDto>>(PageEndpoint($"seafood/products/{groupId}/skus/page", search, skip, take));
    public Task<ProductGroupDto?> SaveProductGroupAsync(ProductGroupDto dto) => RequestClient.PostAPIAsync<ProductGroupDto>("seafood/products/groups", dto);
    public Task<bool> DeleteProductGroupAsync(int id) => RequestClient.DeleteAPIAsync<bool>($"seafood/products/groups/{id}");
    public Task<SeafoodSelect2SearchResponseDto?> SearchProductGroupOptionsAsync(string? search, int page = 1, int pageSize = 20)
        => SearchOptionsAsync("seafood/select-options/product-groups", search, page, pageSize);
    public Task<SeafoodSelect2SearchResponseDto?> SearchSkusAsync(string? search, int page = 1, int pageSize = 20)
        => SearchOptionsAsync("seafood/select-options/skus", search, page, pageSize);
    public Task<ProductSkuDto?> SaveSkuAsync(ProductSkuDto dto) => RequestClient.PostAPIAsync<ProductSkuDto>("seafood/products/sku", dto);
    public Task<bool> DeleteSkuAsync(int id) => RequestClient.DeleteAPIAsync<bool>($"seafood/products/sku/{id}");
    public Task<List<CustomerDto>?> CustomersAsync() => RequestClient.GetAPIAsync<List<CustomerDto>>("seafood/customers");
    public Task<SeafoodPagedResult<CustomerDto>?> CustomersPageAsync(string? search, int skip, int take)
        => RequestClient.GetAPIAsync<SeafoodPagedResult<CustomerDto>>(PageEndpoint("seafood/customers/page", search, skip, take));
    public Task<SeafoodSelect2SearchResponseDto?> SearchCustomersAsync(string? search, int page = 1, int pageSize = 20)
        => SearchOptionsAsync("seafood/select-options/customers", search, page, pageSize);
    public Task<CustomerDto?> SaveCustomerAsync(CustomerDto dto) => RequestClient.PostAPIAsync<CustomerDto>("seafood/customers", dto);
    public Task<bool> DeleteCustomerAsync(int id) => RequestClient.DeleteAPIAsync<bool>($"seafood/customers/{id}");
    public Task<List<BusinessPartnerDto>?> PartnersAsync(int? role = null)
        => RequestClient.GetAPIAsync<List<BusinessPartnerDto>>(role.HasValue ? $"seafood/partners?role={role}" : "seafood/partners");
    public Task<SeafoodPagedResult<BusinessPartnerDto>?> PartnersPageAsync(int? role, string? search, int skip, int take)
        => RequestClient.GetAPIAsync<SeafoodPagedResult<BusinessPartnerDto>>(PageEndpoint("seafood/partners/page", search, skip, take, role));
    public Task<SeafoodSelect2SearchResponseDto?> SearchPartnerOptionsAsync(string? search, int? role = null, int page = 1, int pageSize = 20)
        => RequestClient.GetAPIAsync<SeafoodSelect2SearchResponseDto>($"seafood/select-options/partners{(role.HasValue ? $"?role={role}&" : "?")}search={Uri.EscapeDataString(search?.Trim() ?? "")}&page={Math.Max(1, page)}&pageSize={Math.Clamp(pageSize, 1, 100)}");
    public Task<BusinessPartnerDto?> SavePartnerAsync(BusinessPartnerDto dto) => RequestClient.PostAPIAsync<BusinessPartnerDto>("seafood/partners", dto);
    public Task<bool> DeletePartnerAsync(int id) => RequestClient.DeleteAPIAsync<bool>($"seafood/partners/{id}");
    public Task<List<VesselDto>?> VesselsAsync() => RequestClient.GetAPIAsync<List<VesselDto>>("seafood/vessels");
    public Task<SeafoodPagedResult<VesselDto>?> VesselsPageAsync(string? search, int skip, int take)
        => RequestClient.GetAPIAsync<SeafoodPagedResult<VesselDto>>(PageEndpoint("seafood/vessels/page", search, skip, take));
    public Task<VesselDto?> SaveVesselAsync(VesselDto dto) => RequestClient.PostAPIAsync<VesselDto>("seafood/vessels", dto);
    public Task<bool> DeleteVesselAsync(int id) => RequestClient.DeleteAPIAsync<bool>($"seafood/vessels/{id}");
    public Task<List<PaymentTermDto>?> PaymentTermsAsync() => RequestClient.GetAPIAsync<List<PaymentTermDto>>("seafood/payment-terms");
    public Task<SeafoodPagedResult<PaymentTermDto>?> PaymentTermsPageAsync(string? search, int skip, int take)
        => RequestClient.GetAPIAsync<SeafoodPagedResult<PaymentTermDto>>(PageEndpoint("seafood/payment-terms/page", search, skip, take));
    public Task<SeafoodSelect2SearchResponseDto?> SearchPaymentTermsAsync(string? search, int page = 1, int pageSize = 20)
        => SearchOptionsAsync("seafood/select-options/payment-terms", search, page, pageSize);
    public Task<PaymentTermDto?> SavePaymentTermAsync(PaymentTermDto dto) => RequestClient.PostAPIAsync<PaymentTermDto>("seafood/payment-terms", dto);
    public Task<bool> DeletePaymentTermAsync(int id) => RequestClient.DeleteAPIAsync<bool>($"seafood/payment-terms/{id}");
    public Task<List<InboundPurchaseDto>?> InboundAsync(string? search = null, int skip = 0, int take = 100)
        => RequestClient.GetAPIAsync<List<InboundPurchaseDto>>($"seafood/inbound?skip={skip}&take={take}{(string.IsNullOrWhiteSpace(search) ? "" : $"&search={Uri.EscapeDataString(search)}")}");
    public Task<SeafoodPagedResult<InboundPurchaseDto>?> InboundPageAsync(string? search, int skip, int take)
        => RequestClient.GetAPIAsync<SeafoodPagedResult<InboundPurchaseDto>>(PageEndpoint("seafood/inbound/page", search, skip, take));
    public Task<InboundPurchaseDto?> SaveInboundAsync(InboundPurchaseDto dto) => RequestClient.PostAPIAsync<InboundPurchaseDto>("seafood/inbound", dto);
    public Task<List<ProductionLotDto>?> ProductionAsync() => RequestClient.GetAPIAsync<List<ProductionLotDto>>("seafood/production");
    public Task<SeafoodPagedResult<ProductionLotDto>?> ProductionPageAsync(string? search, int skip, int take)
        => RequestClient.GetAPIAsync<SeafoodPagedResult<ProductionLotDto>>(PageEndpoint("seafood/production/page", search, skip, take));
    public Task<ProductionLotDto?> SaveProductionAsync(ProductionLotDto dto) => RequestClient.PostAPIAsync<ProductionLotDto>("seafood/production", dto);
    public Task<List<RawMaterialLotDto>?> RawLotsAsync(string? search = null)
        => RequestClient.GetAPIAsync<List<RawMaterialLotDto>>($"seafood/raw-lots{(string.IsNullOrWhiteSpace(search) ? "" : $"?search={Uri.EscapeDataString(search)}")}");
    public Task<SeafoodSelect2SearchResponseDto?> SearchRawLotOptionsAsync(string? search, int page = 1, int pageSize = 20)
        => SearchOptionsAsync("seafood/select-options/raw-lots", search, page, pageSize);
    public Task<List<InventoryRowDto>?> InventoryAsync(string? search = null, int skip = 0, int take = 100)
        => RequestClient.GetAPIAsync<List<InventoryRowDto>>($"seafood/inventory?skip={skip}&take={take}{(string.IsNullOrWhiteSpace(search) ? "" : $"&search={Uri.EscapeDataString(search)}")}");
    public Task<SeafoodPagedResult<InventoryRowDto>?> InventoryPageAsync(string? search, int skip, int take)
        => RequestClient.GetAPIAsync<SeafoodPagedResult<InventoryRowDto>>(PageEndpoint("seafood/inventory/page", search, skip, take));
    public Task<InventoryRowDto?> AdjustInventoryAsync(InventoryAdjustmentDto request)
        => RequestClient.PostAPIAsync<InventoryRowDto>("seafood/inventory/adjust", request);
    public Task<AllocationPreviewDto?> PreviewAllocationAsync(AllocationPreviewRequestDto request)
        => RequestClient.PostAPIAsync<AllocationPreviewDto>("seafood/allocation/preview", request);
    public Task<AllocationResultDto?> ConfirmAllocationAsync(AllocationConfirmDto request)
        => RequestClient.PostAPIAsync<AllocationResultDto>("seafood/allocation/confirm", request);
    public Task<bool> ReleaseAllocationAsync(AllocationReleaseDto request)
        => RequestClient.PostAPIAsync<bool>("seafood/allocation/release", request);
    public Task<List<SalesContractDto>?> QuotesAsync() => RequestClient.GetAPIAsync<List<SalesContractDto>>("seafood/quotes");
    public Task<SeafoodPagedResult<SalesContractDto>?> QuotesPageAsync(string? search, int skip, int take)
        => RequestClient.GetAPIAsync<SeafoodPagedResult<SalesContractDto>>(PageEndpoint("seafood/quotes/page", search, skip, take));
    public Task<SeafoodSelect2SearchResponseDto?> SearchContractOptionsAsync(string? search, int page = 1, int pageSize = 20)
        => SearchOptionsAsync("seafood/select-options/contracts", search, page, pageSize);
    public Task<SalesContractDto?> SaveQuoteAsync(SalesContractDto dto) => RequestClient.PostAPIAsync<SalesContractDto>("seafood/quotes", dto);
    public Task<PriceSuggestionDto?> SuggestPriceAsync(int customerId, int skuId)
        => RequestClient.GetAPIAsync<PriceSuggestionDto>($"seafood/quotes/suggest-price?customerId={customerId}&skuId={skuId}");
    public Task<SalesContractDto?> ApproveAsync(int id) => RequestClient.PostAPIAsync<SalesContractDto>($"seafood/quotes/{id}/approve", "");
    public Task<List<ExportShipmentDto>?> OrdersAsync() => RequestClient.GetAPIAsync<List<ExportShipmentDto>>("seafood/orders");
    public Task<SeafoodPagedResult<ExportShipmentDto>?> OrdersPageAsync(string? search, int skip, int take)
        => RequestClient.GetAPIAsync<SeafoodPagedResult<ExportShipmentDto>>(PageEndpoint("seafood/orders/page", search, skip, take));
    public Task<ExportShipmentDto?> SaveOrderAsync(ExportShipmentDto dto) => RequestClient.PostAPIAsync<ExportShipmentDto>("seafood/orders", dto);
    public Task<ExportShipmentDto?> ConfirmOrderAsync(int id) => RequestClient.PostAPIAsync<ExportShipmentDto>($"seafood/orders/{id}/confirm", "");
    public Task<List<DocumentTypeDto>?> DocumentTypesAsync(bool includeInactive = true) => RequestClient.GetAPIAsync<List<DocumentTypeDto>>($"seafood/documents/types?includeInactive={includeInactive.ToString().ToLowerInvariant()}");
    public Task<SeafoodPagedResult<DocumentTypeDto>?> DocumentTypesPageAsync(bool includeInactive, string? search, int skip, int take)
        => RequestClient.GetAPIAsync<SeafoodPagedResult<DocumentTypeDto>>(PageEndpoint("seafood/documents/types/page", search, skip, take, includeInactive));
    public Task<DocumentTypeDto?> SaveDocumentTypeAsync(DocumentTypeDto dto) => RequestClient.PostAPIAsync<DocumentTypeDto>("seafood/documents/types", dto);
    public Task<bool> DeleteDocumentTypeAsync(int id) => RequestClient.DeleteAPIAsync<bool>($"seafood/documents/types/{id}");
    public Task<SeafoodSelect2SearchResponseDto?> SearchDocumentTypeOptionsAsync(string? search, int page = 1, int pageSize = 20)
        => SearchOptionsAsync("seafood/documents/types/options", search, page, pageSize);
    public Task<List<DocumentAttachmentDto>?> DocumentsAsync(string ownerType, int ownerId)
        => RequestClient.GetAPIAsync<List<DocumentAttachmentDto>>($"seafood/documents?ownerType={Uri.EscapeDataString(ownerType)}&ownerId={ownerId}");
    public Task<DocumentChecklistDto?> DocumentChecklistAsync(string ownerType, int ownerId, string marketCode, DateTime? effectiveDate = null)
        => RequestClient.GetAPIAsync<DocumentChecklistDto>($"seafood/documents/checklist?ownerType={Uri.EscapeDataString(ownerType)}&ownerId={ownerId}&marketCode={Uri.EscapeDataString(marketCode)}{(effectiveDate.HasValue ? $"&effectiveDate={effectiveDate:O}" : "")}");
    public Task<DocumentAttachmentDto?> UploadDocumentAsync(string ownerType, int ownerId, int? documentTypeId, IBrowserFile file)
        => RequestClient.PostAPIWithFileAsync<DocumentAttachmentDto>($"seafood/documents/upload?ownerType={Uri.EscapeDataString(ownerType)}&ownerId={ownerId}{(documentTypeId.HasValue ? $"&documentTypeId={documentTypeId}" : "")}", file);
    public Task<DocumentAttachmentDto?> VerifyDocumentAsync(long id, bool verified, string? note = null)
        => RequestClient.PostAPIAsync<DocumentAttachmentDto>($"seafood/documents/{id}/verify", new { verified, note });
    public Task<SeafoodImportPreviewDto?> PreviewImportAsync(IBrowserFile file)
        => RequestClient.PostAPIWithFileAsync<SeafoodImportPreviewDto>("seafood/import/preview", file);
    public Task<SeafoodImportPreviewDto?> GetImportAsync(int id) => RequestClient.GetAPIAsync<SeafoodImportPreviewDto>($"seafood/import/{id}");
    public Task<SeafoodImportPreviewDto?> ConfirmImportAsync(int id, string? note = null)
        => RequestClient.PostAPIAsync<SeafoodImportPreviewDto>($"seafood/import/{id}/confirm", new ImportConfirmDto { Confirm = true, Note = note });
    public Task<List<PaymentInstallmentDto>?> PaymentsAsync() => RequestClient.GetAPIAsync<List<PaymentInstallmentDto>>("seafood/payments");
    public Task<SeafoodPagedResult<PaymentInstallmentDto>?> PaymentsPageAsync(string? search, int skip, int take)
        => RequestClient.GetAPIAsync<SeafoodPagedResult<PaymentInstallmentDto>>(PageEndpoint("seafood/payments/page", search, skip, take));
    public Task<PaymentInstallmentDto?> ReceivePaymentAsync(int id, decimal amount)
        => RequestClient.PostAPIAsync<PaymentInstallmentDto>($"seafood/payments/{id}/receive?amount={amount}", "");
    public Task<List<CustomerDepositDto>?> DepositsAsync() => RequestClient.GetAPIAsync<List<CustomerDepositDto>>("seafood/deposits");
    public Task<SeafoodPagedResult<CustomerDepositDto>?> DepositsPageAsync(string? search, int skip, int take)
        => RequestClient.GetAPIAsync<SeafoodPagedResult<CustomerDepositDto>>(PageEndpoint("seafood/deposits/page", search, skip, take));
    public Task<CustomerDepositDto?> SaveDepositAsync(CustomerDepositDto dto) => RequestClient.PostAPIAsync<CustomerDepositDto>("seafood/deposits", dto);
    public Task<CustomerDepositDto?> AllocateDepositAsync(int id, int contractId, decimal amount)
        => RequestClient.PostAPIAsync<CustomerDepositDto>($"seafood/deposits/{id}/allocate?contractId={contractId}&amount={amount}", "");
    public Task<TraceabilityDto?> TraceabilityAsync(string ownerType, int ownerId)
        => RequestClient.GetAPIAsync<TraceabilityDto>($"seafood/traceability/{Uri.EscapeDataString(ownerType)}/{ownerId}");

    public Task<ApiResponseBase<AppHistorySearchResponseDto>?> SearchAuditLogsAsync(AppHistoryFilterPagingDto filter)
        => RequestClient.PostAPIAsync<ApiResponseBase<AppHistorySearchResponseDto>>("appHistories/search", filter);

    private static Task<SeafoodSelect2SearchResponseDto?> SearchOptionsAsync(string endpoint, string? search, int page, int pageSize)
        => RequestClient.GetAPIAsync<SeafoodSelect2SearchResponseDto>($"{endpoint}?search={Uri.EscapeDataString(search?.Trim() ?? "")}&page={Math.Max(1, page)}&pageSize={Math.Clamp(pageSize, 1, 100)}");

    private static string PageEndpoint(string endpoint, string? search, int skip, int take, params object?[] filters)
    {
        var query = new List<string> { $"skip={Math.Max(0, skip)}", $"take={Math.Clamp(take, 1, 100)}" };
        if (!string.IsNullOrWhiteSpace(search)) query.Add($"search={Uri.EscapeDataString(search.Trim())}");
        foreach (var filter in filters)
        {
            if (filter is null) continue;
            query.Add(filter is bool b ? $"includeInactive={b.ToString().ToLowerInvariant()}" : filter is int i ? (endpoint.Contains("partners", StringComparison.OrdinalIgnoreCase) ? $"role={i}" : $"category={i}") : $"filter={Uri.EscapeDataString(filter.ToString()!)}");
        }
        return $"{endpoint}?{string.Join("&", query)}";
    }
}

public class SeafoodPagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
}

public static class SeafoodTableQuery
{
    public static QueryData<T> ToQueryData<T>(SeafoodPagedResult<T>? result, QueryPageOptions options)
        => new()
        {
            Items = result?.Items ?? new List<T>(),
            TotalCount = result?.TotalCount ?? 0,
            IsFiltered = true,
            IsSorted = true,
            IsSearch = !string.IsNullOrWhiteSpace(options.SearchText)
        };
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
public class PieSliceDto { public string Label { get; set; } = ""; public decimal Value { get; set; } }
public class CatalogLookupDto
{
    public int Id { get; set; }
    public int Category { get; set; }
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
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
    public bool IsActive { get; set; } = true;
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
    public int Kind { get; set; } = 1;
    public string? Purchaser { get; set; }
    public string? Note { get; set; }
    public bool IsActive { get; set; } = true;
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
    public List<int> Roles { get; set; } = new();
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
    public bool IsActive { get; set; } = true;
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
    public DateTime ReceivedDate { get; set; } = DateTime.UtcNow;
    public decimal RawMaterialKg { get; set; }
    public int? TargetMarketId { get; set; }
    public decimal RecoveryRatio { get; set; }
    public string? Note { get; set; }
    public List<string> CertificateCodes { get; set; } = new();
    public List<ProductionInputDto> Inputs { get; set; } = new();
    public List<DocumentAttachmentDto> Documents { get; set; } = new();
    public List<ProductionOutputDto> Outputs { get; set; } = new();
}
public class ProductionOutputDto
{
    public int SkuId { get; set; }
    public string? SkuName { get; set; }
    public decimal RecoveredKg { get; set; }
    public decimal RawUsedKg { get; set; }
    public decimal YieldRatio { get; set; }
    public string? ExportMarket { get; set; }
    public decimal? CostUsd { get; set; }
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
public class InventoryRowDto
{
    public int Id { get; set; }
    public int LotId { get; set; }
    public string LotNumber { get; set; } = "";
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
    public int MovementType { get; set; }
    public decimal QuantityKg { get; set; }
    public string ReferenceType { get; set; } = "";
    public int? ReferenceId { get; set; }
    public DateTime OccurredAt { get; set; }
    public string? Reason { get; set; }
}
public class InventoryAdjustmentDto { public int InventoryBalanceId { get; set; } public decimal QuantityKg { get; set; } public string Reason { get; set; } = ""; }
public class AllocationPreviewRequestDto { public int SalesContractId { get; set; } public DateTime? Etd { get; set; } }
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
public class AllocationReleaseDto { public int SalesContractId { get; set; } public string? Reason { get; set; } }
public class AllocationItemDto { public int SalesContractLineId { get; set; } public int InventoryBalanceId { get; set; } public decimal QuantityKg { get; set; } }
public class AllocationResultDto { public int SalesContractId { get; set; } public decimal AllocatedKg { get; set; } public decimal MissingKg { get; set; } public List<AllocationItemDto> Items { get; set; } = new(); }
public class SalesContractDto
{
    public int Id { get; set; }
    public string ContractNo { get; set; } = "";
    public int CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public int Status { get; set; }
    public decimal SuggestedUnitPriceUsd { get; set; }
    public decimal VarianceVsLastPct { get; set; }
    public decimal VarianceVsPeersPct { get; set; }
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
    public DateTime? Etd { get; set; }
    public DateTime? Eta { get; set; }
    public string? InvoiceNo { get; set; }
    public int? PaymentTermId { get; set; }
    public string? PaymentTermName { get; set; }
    public int Cartons { get; set; }
    public decimal QtyKg { get; set; }
    public decimal QtyLbs { get; set; }
    public decimal AmountUsd { get; set; }
    public string? ContainerNo { get; set; }
    public string? ContainerType { get; set; }
    public string? Route { get; set; }
    public int Status { get; set; }
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
public class DocumentTypeDto { public int Id { get; set; } public string Code { get; set; } = ""; public string Name { get; set; } = ""; public string? Description { get; set; } public bool IsActive { get; set; } }
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
    public int Status { get; set; }
    public string FileName { get; set; } = "";
    public long FileSize { get; set; }
    public DateTime UploadedAt { get; set; }
    public string? DownloadUrl { get; set; }
    public string? Note { get; set; }
}
public class DocumentChecklistDto { public string MarketCode { get; set; } = ""; public DateTime EffectiveDate { get; set; } public List<DocumentChecklistItemDto> Items { get; set; } = new(); }
public class DocumentChecklistItemDto { public string DocumentCode { get; set; } = ""; public string DocumentName { get; set; } = ""; public bool IsRequired { get; set; } public bool IsValid { get; set; } public string? Reason { get; set; } }
public class SeafoodImportPreviewDto
{
    public int Id { get; set; }
    public string FileName { get; set; } = "";
    public int Status { get; set; }
    public int TotalRows { get; set; }
    public int ValidRows { get; set; }
    public int ErrorRows { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<SeafoodImportStagingRowDto> Rows { get; set; } = new();
}
public class SeafoodImportStagingRowDto
{
    public long Id { get; set; }
    public string SheetName { get; set; } = "";
    public int RowNumber { get; set; }
    public string PayloadJson { get; set; } = "{}";
    public bool IsValid { get; set; }
    public string? ErrorMessage { get; set; }
}
public class ImportConfirmDto { public bool Confirm { get; set; } public string? Note { get; set; } }
public class PaymentInstallmentDto
{
    public int Id { get; set; }
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
    public DateTime ReceivedDate { get; set; } = DateTime.UtcNow;
    public string? Note { get; set; }
    public decimal AllocatedUsd { get; set; }
}
public class TraceabilityDto
{
    public string RootType { get; set; } = "";
    public long RootId { get; set; }
    public List<TraceabilityNodeDto> Nodes { get; set; } = new();
    public List<TraceabilityEdgeDto> Edges { get; set; } = new();
}
public class TraceabilityNodeDto { public string Type { get; set; } = ""; public long Id { get; set; } public string Label { get; set; } = ""; }
public class TraceabilityEdgeDto { public string FromType { get; set; } = ""; public long FromId { get; set; } public string ToType { get; set; } = ""; public long ToId { get; set; } public decimal QuantityKg { get; set; } public string Relation { get; set; } = ""; }
