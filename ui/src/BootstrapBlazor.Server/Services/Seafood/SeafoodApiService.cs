using BootstrapBlazor.Server.Http;

namespace BootstrapBlazor.Server.Services.Seafood;

public class SeafoodApiService
{
    public Task<DashboardDto?> DashboardAsync() => RequestClient.GetAPIAsync<DashboardDto>("seafood/dashboard");
    public Task<List<CatalogLookupDto>?> CatalogAsync(int? category = null)
        => RequestClient.GetAPIAsync<List<CatalogLookupDto>>(category == null ? "seafood/catalog" : $"seafood/catalog?category={category}");
    public Task<CatalogLookupDto?> SaveCatalogAsync(CatalogLookupDto dto) => RequestClient.PostAPIAsync<CatalogLookupDto>("seafood/catalog", dto);
    public Task<List<ProductGroupDto>?> ProductsAsync() => RequestClient.GetAPIAsync<List<ProductGroupDto>>("seafood/products");
    public Task<ProductSkuDto?> SaveSkuAsync(ProductSkuDto dto) => RequestClient.PostAPIAsync<ProductSkuDto>("seafood/products/sku", dto);
    public Task<List<CustomerDto>?> CustomersAsync() => RequestClient.GetAPIAsync<List<CustomerDto>>("seafood/customers");
    public Task<CustomerDto?> SaveCustomerAsync(CustomerDto dto) => RequestClient.PostAPIAsync<CustomerDto>("seafood/customers", dto);
    public Task<List<PaymentTermDto>?> PaymentTermsAsync() => RequestClient.GetAPIAsync<List<PaymentTermDto>>("seafood/payment-terms");
    public Task<PaymentTermDto?> SavePaymentTermAsync(PaymentTermDto dto) => RequestClient.PostAPIAsync<PaymentTermDto>("seafood/payment-terms", dto);
    public Task<List<InboundPurchaseDto>?> InboundAsync() => RequestClient.GetAPIAsync<List<InboundPurchaseDto>>("seafood/inbound");
    public Task<InboundPurchaseDto?> SaveInboundAsync(InboundPurchaseDto dto) => RequestClient.PostAPIAsync<InboundPurchaseDto>("seafood/inbound", dto);
    public Task<List<ProductionLotDto>?> ProductionAsync() => RequestClient.GetAPIAsync<List<ProductionLotDto>>("seafood/production");
    public Task<ProductionLotDto?> SaveProductionAsync(ProductionLotDto dto) => RequestClient.PostAPIAsync<ProductionLotDto>("seafood/production", dto);
    public Task<List<InventoryRowDto>?> InventoryAsync() => RequestClient.GetAPIAsync<List<InventoryRowDto>>("seafood/inventory");
    public Task<List<SalesContractDto>?> QuotesAsync() => RequestClient.GetAPIAsync<List<SalesContractDto>>("seafood/quotes");
    public Task<SalesContractDto?> SaveQuoteAsync(SalesContractDto dto) => RequestClient.PostAPIAsync<SalesContractDto>("seafood/quotes", dto);
    public Task<PriceSuggestionDto?> SuggestPriceAsync(int customerId, int skuId)
        => RequestClient.GetAPIAsync<PriceSuggestionDto>($"seafood/quotes/suggest-price?customerId={customerId}&skuId={skuId}");
    public Task<SalesContractDto?> ApproveAsync(int id) => RequestClient.PostAPIAsync<SalesContractDto>($"seafood/quotes/{id}/approve", "");
    public Task<List<ExportShipmentDto>?> OrdersAsync() => RequestClient.GetAPIAsync<List<ExportShipmentDto>>("seafood/orders");
    public Task<ExportShipmentDto?> SaveOrderAsync(ExportShipmentDto dto) => RequestClient.PostAPIAsync<ExportShipmentDto>("seafood/orders", dto);
    public Task<List<PaymentInstallmentDto>?> PaymentsAsync() => RequestClient.GetAPIAsync<List<PaymentInstallmentDto>>("seafood/payments");
    public Task<PaymentInstallmentDto?> ReceivePaymentAsync(int id, decimal amount)
        => RequestClient.PostAPIAsync<PaymentInstallmentDto>($"seafood/payments/{id}/receive?amount={amount}", "");
    public Task<List<CustomerDepositDto>?> DepositsAsync() => RequestClient.GetAPIAsync<List<CustomerDepositDto>>("seafood/deposits");
    public Task<CustomerDepositDto?> SaveDepositAsync(CustomerDepositDto dto) => RequestClient.PostAPIAsync<CustomerDepositDto>("seafood/deposits", dto);
    public Task<CustomerDepositDto?> AllocateDepositAsync(int id, int contractId, decimal amount)
        => RequestClient.PostAPIAsync<CustomerDepositDto>($"seafood/deposits/{id}/allocate?contractId={contractId}&amount={amount}", "");
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
    public bool IsActive { get; set; } = true;
}
public class CustomerDto
{
    public int Id { get; set; }
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public string? ContactName { get; set; }
    public int Kind { get; set; } = 1;
    public string? Purchaser { get; set; }
    public bool IsActive { get; set; } = true;
}
public class PaymentTermDto
{
    public int Id { get; set; }
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public string RatiosJson { get; set; } = "[]";
    public bool IsLc { get; set; }
    public bool IsActive { get; set; } = true;
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
    public decimal TotalKg { get; set; }
    public decimal TotalAmountUsd { get; set; }
    public List<InboundLineDto> Lines { get; set; } = new();
}
public class InboundLineDto
{
    public string Commodity { get; set; } = "";
    public decimal QtyKgLongLine { get; set; }
    public decimal QtyKgHandline { get; set; }
    public decimal QtyKgPs { get; set; }
    public decimal QtyKgLand { get; set; }
    public decimal PriceUsd { get; set; }
    public decimal InvoiceAmountUsd { get; set; }
}
public class ProductionLotDto
{
    public int Id { get; set; }
    public string LotNumber { get; set; } = "";
    public int? InboundPurchaseId { get; set; }
    public DateTime ReceivedDate { get; set; } = DateTime.UtcNow;
    public decimal RawMaterialKg { get; set; }
    public decimal RecoveryRatio { get; set; }
    public string? Note { get; set; }
    public List<string> CertificateCodes { get; set; } = new();
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
}
public class InventoryRowDto
{
    public string LotNumber { get; set; } = "";
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
    public int CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public int Status { get; set; }
    public decimal SuggestedUnitPriceUsd { get; set; }
    public decimal VarianceVsLastPct { get; set; }
    public decimal VarianceVsPeersPct { get; set; }
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
}
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
