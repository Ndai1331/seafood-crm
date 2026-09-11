using Application.Seafood;
using Contract.Seafood;
using Core.Const;
using Domain.Seafood;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebApi.Authorization;

namespace WebApi.Controllers.Seafood
{
    [ApiController]
    [Authorize]
    [Route("api/seafood")]
    public class SeafoodController : ControllerBase
    {
        private readonly SeafoodCatalogService _catalog;
        private readonly InboundService _inbound;
        private readonly ProductionService _production;
        private readonly InventoryService _inventory;
        private readonly ExportService _export;
        private readonly FinanceService _finance;
        private readonly DashboardService _dashboard;
        private readonly SeafoodAllocationService _allocation;
        private readonly SeafoodPartnerService _partners;
        private readonly SeafoodRawLotService _rawLots;
        private readonly SeafoodTraceabilityService _traceability;

        public SeafoodController(
            SeafoodCatalogService catalog,
            InboundService inbound,
            ProductionService production,
            InventoryService inventory,
            ExportService export,
            FinanceService finance,
            DashboardService dashboard,
            SeafoodAllocationService allocation,
            SeafoodPartnerService partners,
            SeafoodRawLotService rawLots,
            SeafoodTraceabilityService traceability)
        {
            _catalog = catalog;
            _inbound = inbound;
            _production = production;
            _inventory = inventory;
            _export = export;
            _finance = finance;
            _dashboard = dashboard;
            _allocation = allocation;
            _partners = partners;
            _rawLots = rawLots;
            _traceability = traceability;
        }

        [HttpGet("dashboard")]
        [HasPermission(Permissions.Dashboard)]
        public Task<DashboardDto> Dashboard() => _dashboard.GetAsync();

        [HttpGet("catalog")]
        [HasPermission(Permissions.MasterCatalog)]
        public Task<List<CatalogLookupDto>> Catalog([FromQuery] LookupCategory? category) => _catalog.GetLookupsAsync(category);

        [HttpGet("catalog/page")]
        [HasPermission(Permissions.MasterCatalog)]
        public Task<SeafoodPagedResult<CatalogLookupDto>> CatalogPage([FromQuery] LookupCategory? category, [FromQuery] string? search, [FromQuery] int skip = 0, [FromQuery] int take = 20)
            => _catalog.GetLookupsPageAsync(category, search, skip, take);

        [HttpGet("select-options/lookups/{category:int}")]
        [HasPermission(Permissions.MasterCatalog)]
        public Task<SeafoodSelect2SearchResponseDto> LookupOptions(int category, [FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
            => _catalog.SearchLookupsAsync((LookupCategory)category, search, page, pageSize);

        [HttpPost("catalog")]
        [HasPermission(Permissions.MasterCatalog)]
        public Task<CatalogLookupDto> SaveCatalog([FromBody] CatalogLookupDto dto) => _catalog.SaveLookupAsync(dto);

        [HttpDelete("catalog/{id:int}")]
        [HasPermission(Permissions.MasterCatalog)]
        public Task<bool> DeleteCatalog(int id) => _catalog.DeleteLookupAsync(id);

        [HttpGet("market-document-rules")]
        [HasPermission(Permissions.MasterCatalog)]
        public Task<List<MarketCertificateRuleDto>> MarketDocumentRules() => _catalog.GetMarketCertificateRulesAsync();

        [HttpGet("market-document-rules/page")]
        [HasPermission(Permissions.MasterCatalog)]
        public Task<SeafoodPagedResult<MarketCertificateRuleDto>> MarketDocumentRulesPage([FromQuery] string? search, [FromQuery] int skip = 0, [FromQuery] int take = 20)
            => _catalog.GetMarketCertificateRulesPageAsync(search, skip, take);

        [HttpPost("market-document-rules")]
        [HasPermission(Permissions.MasterCatalog)]
        public Task<MarketCertificateRuleDto> SaveMarketDocumentRule([FromBody] MarketCertificateRuleDto dto)
            => _catalog.SaveMarketCertificateRuleAsync(dto);

        [HttpDelete("market-document-rules/{id:int}")]
        [HasPermission(Permissions.MasterCatalog)]
        public Task<bool> DeleteMarketDocumentRule(int id) => _catalog.DeleteMarketCertificateRuleAsync(id);

        [HttpGet("products")]
        [HasPermission(Permissions.MasterProducts)]
        public Task<List<ProductGroupDto>> Products() => _catalog.GetProductsAsync();

        [HttpGet("products/page")]
        [HasPermission(Permissions.MasterProducts)]
        public Task<SeafoodPagedResult<ProductGroupDto>> ProductsPage([FromQuery] string? search, [FromQuery] int skip = 0, [FromQuery] int take = 20)
            => _catalog.GetProductsPageAsync(search, skip, take);

        [HttpGet("products/{groupId:int}/skus/page")]
        [HasPermission(Permissions.MasterProducts)]
        public Task<SeafoodPagedResult<ProductSkuDto>> ProductSkusPage(int groupId, [FromQuery] string? search, [FromQuery] int skip = 0, [FromQuery] int take = 20)
            => _catalog.GetProductSkusPageAsync(groupId, search, skip, take);

        [HttpGet("select-options/skus")]
        [HasPermission(Permissions.MasterProducts)]
        public Task<SeafoodSelect2SearchResponseDto> SkuOptions([FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
            => _catalog.SearchSkusAsync(search, page, pageSize);

        [HttpGet("select-options/product-groups")]
        [HasPermission(Permissions.MasterProducts)]
        public Task<SeafoodSelect2SearchResponseDto> ProductGroupOptions([FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
            => _catalog.SearchProductGroupsAsync(search, page, pageSize);

        [HttpPost("products/groups")]
        [HasPermission(Permissions.MasterProducts)]
        public Task<ProductGroupDto> SaveProductGroup([FromBody] ProductGroupDto dto) => _catalog.SaveProductGroupAsync(dto);

        [HttpDelete("products/groups/{id:int}")]
        [HasPermission(Permissions.MasterProducts)]
        public Task<bool> DeleteProductGroup(int id) => _catalog.DeleteProductGroupAsync(id);

        [HttpPost("products/sku")]
        [HasPermission(Permissions.MasterProducts)]
        public Task<ProductSkuDto> SaveSku([FromBody] ProductSkuDto dto) => _catalog.SaveSkuAsync(dto);

        [HttpDelete("products/sku/{id:int}")]
        [HasPermission(Permissions.MasterProducts)]
        public Task<bool> DeleteSku(int id) => _catalog.DeleteSkuAsync(id);

        [HttpGet("customers")]
        [HasPermission(Permissions.MasterCustomers)]
        public Task<List<CustomerDto>> Customers() => _catalog.GetCustomersAsync();

        [HttpGet("customers/page")]
        [HasPermission(Permissions.MasterCustomers)]
        public Task<SeafoodPagedResult<CustomerDto>> CustomersPage([FromQuery] string? search, [FromQuery] int skip = 0, [FromQuery] int take = 20)
            => _catalog.GetCustomersPageAsync(search, skip, take);

        [HttpGet("select-options/customers")]
        [HasPermission(Permissions.MasterCustomers)]
        public Task<SeafoodSelect2SearchResponseDto> CustomerOptions([FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
            => _catalog.SearchCustomersAsync(search, page, pageSize);

        [HttpPost("customers")]
        [HasPermission(Permissions.MasterCustomers)]
        public Task<CustomerDto> SaveCustomer([FromBody] CustomerDto dto) => _catalog.SaveCustomerAsync(dto);

        [HttpDelete("customers/{id:int}")]
        [HasPermission(Permissions.MasterCustomers)]
        public Task<bool> DeleteCustomer(int id) => _catalog.DeleteCustomerAsync(id);

        [HttpGet("partners")]
        [HasPermission(Permissions.MasterCustomers)]
        public Task<List<BusinessPartnerDto>> Partners([FromQuery] BusinessPartnerRole? role) => _partners.ListPartnersAsync(role);

        [HttpGet("partners/page")]
        [HasPermission(Permissions.MasterCustomers)]
        public Task<SeafoodPagedResult<BusinessPartnerDto>> PartnersPage([FromQuery] BusinessPartnerRole? role, [FromQuery] string? search, [FromQuery] int skip = 0, [FromQuery] int take = 20)
            => _partners.ListPartnersPageAsync(role, search, skip, take);

        [HttpGet("select-options/partners")]
        [HasPermission(Permissions.MasterCustomers)]
        public Task<SeafoodSelect2SearchResponseDto> PartnerOptions([FromQuery] BusinessPartnerRole? role, [FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
            => _partners.SearchOptionsAsync(role, search, page, pageSize);

        [HttpPost("partners")]
        [HasPermission(Permissions.MasterCustomers)]
        public Task<BusinessPartnerDto> SavePartner([FromBody] BusinessPartnerDto dto) => _partners.SavePartnerAsync(dto);

        [HttpDelete("partners/{id:int}")]
        [HasPermission(Permissions.MasterCustomers)]
        public Task<bool> DeletePartner(int id) => _partners.DeletePartnerAsync(id);

        [HttpGet("vessels")]
        [HasPermission(Permissions.MasterCustomers)]
        public Task<List<VesselDto>> Vessels() => _partners.ListVesselsAsync();

        [HttpGet("vessels/page")]
        [HasPermission(Permissions.MasterCustomers)]
        public Task<SeafoodPagedResult<VesselDto>> VesselsPage([FromQuery] string? search, [FromQuery] int skip = 0, [FromQuery] int take = 20)
            => _partners.ListVesselsPageAsync(search, skip, take);

        [HttpPost("vessels")]
        [HasPermission(Permissions.MasterCustomers)]
        public Task<VesselDto> SaveVessel([FromBody] VesselDto dto) => _partners.SaveVesselAsync(dto);

        [HttpDelete("vessels/{id:int}")]
        [HasPermission(Permissions.MasterCustomers)]
        public Task<bool> DeleteVessel(int id) => _partners.DeleteVesselAsync(id);

        [HttpGet("payment-terms")]
        [HasPermission(Permissions.MasterPaymentTerms)]
        public Task<List<PaymentTermDto>> PaymentTerms() => _catalog.GetPaymentTermsAsync();

        [HttpGet("payment-terms/page")]
        [HasPermission(Permissions.MasterPaymentTerms)]
        public Task<SeafoodPagedResult<PaymentTermDto>> PaymentTermsPage([FromQuery] string? search, [FromQuery] int skip = 0, [FromQuery] int take = 20)
            => _catalog.GetPaymentTermsPageAsync(search, skip, take);

        [HttpGet("select-options/payment-terms")]
        [HasPermission(Permissions.MasterPaymentTerms)]
        public Task<SeafoodSelect2SearchResponseDto> PaymentTermOptions([FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
            => _catalog.SearchPaymentTermsAsync(search, page, pageSize);

        [HttpPost("payment-terms")]
        [HasPermission(Permissions.MasterPaymentTerms)]
        public Task<PaymentTermDto> SavePaymentTerm([FromBody] PaymentTermDto dto) => _catalog.SavePaymentTermAsync(dto);

        [HttpDelete("payment-terms/{id:int}")]
        [HasPermission(Permissions.MasterPaymentTerms)]
        public Task<bool> DeletePaymentTerm(int id) => _catalog.DeletePaymentTermAsync(id);

        [HttpGet("inbound")]
        [HasPermission(Permissions.Inbound)]
        public Task<List<InboundPurchaseDto>> Inbound([FromQuery] string? search, [FromQuery] int skip = 0, [FromQuery] int take = 100)
            => _inbound.ListAsync(search, skip, take);

        [HttpGet("inbound/page")]
        [HasPermission(Permissions.Inbound)]
        public Task<SeafoodPagedResult<InboundPurchaseDto>> InboundPage([FromQuery] string? search, [FromQuery] int skip = 0, [FromQuery] int take = 20)
            => _inbound.PageAsync(search, skip, take);

        [HttpPost("inbound")]
        [HasPermission(Permissions.Inbound)]
        public Task<InboundPurchaseDto> SaveInbound([FromBody] InboundPurchaseDto dto) => _inbound.SaveAsync(dto);

        [HttpGet("production")]
        [HasPermission(Permissions.Production)]
        public Task<List<ProductionLotDto>> Production() => _production.ListAsync();

        [HttpGet("production/page")]
        [HasPermission(Permissions.Production)]
        public Task<SeafoodPagedResult<ProductionLotDto>> ProductionPage([FromQuery] string? search, [FromQuery] int skip = 0, [FromQuery] int take = 20)
            => _production.PageAsync(search, skip, take);

        [HttpPost("production")]
        [HasPermission(Permissions.Production)]
        public Task<ProductionLotDto> SaveProduction([FromBody] ProductionLotDto dto) => _production.SaveAsync(dto);

        [HttpGet("inventory")]
        [HasPermission(Permissions.Inventory)]
        public Task<List<InventoryRowDto>> Inventory([FromQuery] string? search, [FromQuery] int skip = 0, [FromQuery] int take = 100)
            => _inventory.ListAsync(search, skip, take);

        [HttpGet("inventory/page")]
        [HasPermission(Permissions.Inventory)]
        public Task<SeafoodPagedResult<InventoryRowDto>> InventoryPage([FromQuery] string? search, [FromQuery] int skip = 0, [FromQuery] int take = 20)
            => _inventory.PageAsync(search, skip, take);

        [HttpPost("inventory/adjust")]
        [HasPermission(Permissions.Inventory)]
        public Task<InventoryRowDto> AdjustInventory([FromBody] InventoryAdjustmentDto request)
            => _inventory.AdjustAsync(request, CurrentUserId());

        [HttpGet("raw-lots")]
        [HasPermission(Permissions.Production)]
        public Task<List<RawMaterialLotDto>> RawLots([FromQuery] string? search)
            => _rawLots.ListAsync(search);

        [HttpGet("select-options/raw-lots")]
        [HasPermission(Permissions.Production)]
        public Task<SeafoodSelect2SearchResponseDto> RawLotOptions([FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
            => _rawLots.SearchOptionsAsync(search, page, pageSize);

        [HttpPost("allocation/preview")]
        [HasPermission(Permissions.ExportAllocations)]
        public Task<AllocationPreviewDto> PreviewAllocation([FromBody] AllocationPreviewRequestDto request)
            => _allocation.PreviewAsync(request);

        [HttpPost("allocation/confirm")]
        [HasPermission(Permissions.ExportAllocations)]
        public Task<AllocationResultDto> ConfirmAllocation([FromBody] AllocationConfirmDto request)
            => _allocation.ConfirmAsync(request, CurrentUserId());

        [HttpPost("allocation/release")]
        [HasPermission(Permissions.ExportAllocations)]
        public async Task<IActionResult> ReleaseAllocation([FromBody] AllocationReleaseDto request)
        {
            await _allocation.ReleaseAsync(request, CurrentUserId());
            return Ok(true);
        }

        [HttpGet("traceability/{ownerType}/{ownerId:int}")]
        [HasPermission(Permissions.Inventory)]
        public Task<TraceabilityDto> Traceability(string ownerType, int ownerId)
            => _traceability.GetAsync(ownerType, ownerId);

        [HttpGet("quotes")]
        [HasPermission(Permissions.ExportQuotes)]
        public Task<List<SalesContractDto>> Quotes() => _export.ListContractsAsync();

        [HttpGet("quotes/page")]
        [HasPermission(Permissions.ExportQuotes)]
        public Task<SeafoodPagedResult<SalesContractDto>> QuotesPage([FromQuery] string? search, [FromQuery] int skip = 0, [FromQuery] int take = 20)
            => _export.PageContractsAsync(search, skip, take);

        [HttpGet("select-options/contracts")]
        [HasPermission(Permissions.ExportOrders)]
        public Task<SeafoodSelect2SearchResponseDto> ContractOptions([FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
            => _catalog.SearchContractsAsync(search, page, pageSize);

        [HttpPost("quotes")]
        [HasPermission(Permissions.ExportQuotes)]
        public Task<SalesContractDto> SaveQuote([FromBody] SalesContractDto dto) => _export.SaveContractAsync(dto);

        [HttpGet("quotes/suggest-price")]
        [HasPermission(Permissions.ExportQuotes)]
        public Task<PriceSuggestionDto> Suggest([FromQuery] int customerId, [FromQuery] int skuId)
            => _export.SuggestPriceAsync(customerId, skuId);

        [HttpPost("quotes/{id:int}/approve")]
        [HasPermission(Permissions.ExportQuotes)]
        public Task<SalesContractDto> Approve(int id, [FromBody] string? note)
            => _export.ApproveAsync(id, note, null);

        [HttpGet("orders")]
        [HasPermission(Permissions.ExportOrders)]
        public Task<List<ExportShipmentDto>> Orders() => _export.ListShipmentsAsync();

        [HttpGet("orders/page")]
        [HasPermission(Permissions.ExportOrders)]
        public Task<SeafoodPagedResult<ExportShipmentDto>> OrdersPage([FromQuery] string? search, [FromQuery] int skip = 0, [FromQuery] int take = 20)
            => _export.PageShipmentsAsync(search, skip, take);

        [HttpPost("orders")]
        [HasPermission(Permissions.ExportOrders)]
        public Task<ExportShipmentDto> SaveOrder([FromBody] ExportShipmentDto dto) => _export.SaveShipmentAsync(dto);

        [HttpPost("orders/{id:int}/confirm")]
        [HasPermission(Permissions.ExportOrders)]
        public Task<ExportShipmentDto> ConfirmOrder(int id) => _export.ConfirmShipmentAsync(id, CurrentUserId());

        [HttpGet("payments")]
        [HasPermission(Permissions.FinancePayments)]
        public Task<List<PaymentInstallmentDto>> Payments() => _finance.ListPaymentsAsync();

        [HttpGet("payments/page")]
        [HasPermission(Permissions.FinancePayments)]
        public Task<SeafoodPagedResult<PaymentInstallmentDto>> PaymentsPage([FromQuery] string? search, [FromQuery] int skip = 0, [FromQuery] int take = 20)
            => _finance.PagePaymentsAsync(search, skip, take);

        [HttpPost("payments/{id:int}/receive")]
        [HasPermission(Permissions.FinancePayments)]
        public Task<PaymentInstallmentDto> Receive(int id, [FromQuery] decimal amount, [FromQuery] DateTime? date)
            => _finance.ReceiveAsync(id, amount, date);

        [HttpGet("deposits")]
        [HasPermission(Permissions.FinanceDeposits)]
        public Task<List<CustomerDepositDto>> Deposits() => _finance.ListDepositsAsync();

        [HttpGet("deposits/page")]
        [HasPermission(Permissions.FinanceDeposits)]
        public Task<SeafoodPagedResult<CustomerDepositDto>> DepositsPage([FromQuery] string? search, [FromQuery] int skip = 0, [FromQuery] int take = 20)
            => _finance.PageDepositsAsync(search, skip, take);

        [HttpPost("deposits")]
        [HasPermission(Permissions.FinanceDeposits)]
        public Task<CustomerDepositDto> SaveDeposit([FromBody] CustomerDepositDto dto) => _finance.SaveDepositAsync(dto);

        [HttpPost("deposits/{id:int}/allocate")]
        [HasPermission(Permissions.FinanceDeposits)]
        public async Task<CustomerDepositDto> Allocate(int id, [FromQuery] int contractId, [FromQuery] decimal amount)
        {
            await _finance.AllocateAsync(depositId: id, contractId, amount);
            var rows = await _finance.ListDepositsAsync();
            return rows.First(d => d.Id == id);
        }

        private int? CurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.PrimarySid) ?? User.FindFirst("sub") ?? User.FindFirst("userId");
            return int.TryParse(claim?.Value, out var id) ? id : null;
        }
    }
}
