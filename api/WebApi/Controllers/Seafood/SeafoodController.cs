using Application.Seafood;
using Contract.Seafood;
using Core.Const;
using Domain.Seafood;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

        public SeafoodController(
            SeafoodCatalogService catalog,
            InboundService inbound,
            ProductionService production,
            InventoryService inventory,
            ExportService export,
            FinanceService finance,
            DashboardService dashboard)
        {
            _catalog = catalog;
            _inbound = inbound;
            _production = production;
            _inventory = inventory;
            _export = export;
            _finance = finance;
            _dashboard = dashboard;
        }

        [HttpGet("dashboard")]
        [HasPermission(Permissions.Dashboard)]
        public Task<DashboardDto> Dashboard() => _dashboard.GetAsync();

        [HttpGet("catalog")]
        [HasPermission(Permissions.MasterCatalog)]
        public Task<List<CatalogLookupDto>> Catalog([FromQuery] LookupCategory? category) => _catalog.GetLookupsAsync(category);

        [HttpPost("catalog")]
        [HasPermission(Permissions.MasterCatalog)]
        public Task<CatalogLookupDto> SaveCatalog([FromBody] CatalogLookupDto dto) => _catalog.SaveLookupAsync(dto);

        [HttpGet("products")]
        [HasPermission(Permissions.MasterProducts)]
        public Task<List<ProductGroupDto>> Products() => _catalog.GetProductsAsync();

        [HttpPost("products/sku")]
        [HasPermission(Permissions.MasterProducts)]
        public Task<ProductSkuDto> SaveSku([FromBody] ProductSkuDto dto) => _catalog.SaveSkuAsync(dto);

        [HttpGet("customers")]
        [HasPermission(Permissions.MasterCustomers)]
        public Task<List<CustomerDto>> Customers() => _catalog.GetCustomersAsync();

        [HttpPost("customers")]
        [HasPermission(Permissions.MasterCustomers)]
        public Task<CustomerDto> SaveCustomer([FromBody] CustomerDto dto) => _catalog.SaveCustomerAsync(dto);

        [HttpGet("payment-terms")]
        [HasPermission(Permissions.MasterPaymentTerms)]
        public Task<List<PaymentTermDto>> PaymentTerms() => _catalog.GetPaymentTermsAsync();

        [HttpPost("payment-terms")]
        [HasPermission(Permissions.MasterPaymentTerms)]
        public Task<PaymentTermDto> SavePaymentTerm([FromBody] PaymentTermDto dto) => _catalog.SavePaymentTermAsync(dto);

        [HttpGet("inbound")]
        [HasPermission(Permissions.Inbound)]
        public Task<List<InboundPurchaseDto>> Inbound() => _inbound.ListAsync();

        [HttpPost("inbound")]
        [HasPermission(Permissions.Inbound)]
        public Task<InboundPurchaseDto> SaveInbound([FromBody] InboundPurchaseDto dto) => _inbound.SaveAsync(dto);

        [HttpGet("production")]
        [HasPermission(Permissions.Production)]
        public Task<List<ProductionLotDto>> Production() => _production.ListAsync();

        [HttpPost("production")]
        [HasPermission(Permissions.Production)]
        public Task<ProductionLotDto> SaveProduction([FromBody] ProductionLotDto dto) => _production.SaveAsync(dto);

        [HttpGet("inventory")]
        [HasPermission(Permissions.Inventory)]
        public Task<List<InventoryRowDto>> Inventory() => _inventory.ListAsync();

        [HttpGet("quotes")]
        [HasPermission(Permissions.ExportQuotes)]
        public Task<List<SalesContractDto>> Quotes() => _export.ListContractsAsync();

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

        [HttpPost("orders")]
        [HasPermission(Permissions.ExportOrders)]
        public Task<ExportShipmentDto> SaveOrder([FromBody] ExportShipmentDto dto) => _export.SaveShipmentAsync(dto);

        [HttpGet("payments")]
        [HasPermission(Permissions.FinancePayments)]
        public Task<List<PaymentInstallmentDto>> Payments() => _finance.ListPaymentsAsync();

        [HttpPost("payments/{id:int}/receive")]
        [HasPermission(Permissions.FinancePayments)]
        public Task<PaymentInstallmentDto> Receive(int id, [FromQuery] decimal amount, [FromQuery] DateTime? date)
            => _finance.ReceiveAsync(id, amount, date);

        [HttpGet("deposits")]
        [HasPermission(Permissions.FinanceDeposits)]
        public Task<List<CustomerDepositDto>> Deposits() => _finance.ListDepositsAsync();

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
    }
}
