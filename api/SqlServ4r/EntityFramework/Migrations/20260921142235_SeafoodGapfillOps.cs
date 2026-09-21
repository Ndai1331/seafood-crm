using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SqlServ4r.EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class SeafoodGapfillOps : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_inventory_balances_LotId_SkuId",
                table: "inventory_balances");

            migrationBuilder.AddColumn<int>(
                name: "PaymentTermId",
                table: "sales_contracts",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "RawEquivalentKg",
                table: "sales_allocations",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "YieldRatioUsed",
                table: "sales_allocations",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "Kind",
                table: "product_skus",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PortOfDischargeId",
                table: "export_shipments",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PortOfLoadingId",
                table: "export_shipments",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SalesInvoiceId",
                table: "deposit_allocations",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_sales_contracts_PaymentTermId",
                table: "sales_contracts",
                column: "PaymentTermId");

            migrationBuilder.CreateIndex(
                name: "IX_inventory_balances_LotId_SkuId_WarehouseId",
                table: "inventory_balances",
                columns: new[] { "LotId", "SkuId", "WarehouseId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_deposit_allocations_SalesInvoiceId",
                table: "deposit_allocations",
                column: "SalesInvoiceId");

            migrationBuilder.AddForeignKey(
                name: "FK_deposit_allocations_sales_invoices_SalesInvoiceId",
                table: "deposit_allocations",
                column: "SalesInvoiceId",
                principalTable: "sales_invoices",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_sales_contracts_payment_terms_PaymentTermId",
                table: "sales_contracts",
                column: "PaymentTermId",
                principalTable: "payment_terms",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_deposit_allocations_sales_invoices_SalesInvoiceId",
                table: "deposit_allocations");

            migrationBuilder.DropForeignKey(
                name: "FK_sales_contracts_payment_terms_PaymentTermId",
                table: "sales_contracts");

            migrationBuilder.DropIndex(
                name: "IX_sales_contracts_PaymentTermId",
                table: "sales_contracts");

            migrationBuilder.DropIndex(
                name: "IX_inventory_balances_LotId_SkuId_WarehouseId",
                table: "inventory_balances");

            migrationBuilder.DropIndex(
                name: "IX_deposit_allocations_SalesInvoiceId",
                table: "deposit_allocations");

            migrationBuilder.DropColumn(
                name: "PaymentTermId",
                table: "sales_contracts");

            migrationBuilder.DropColumn(
                name: "RawEquivalentKg",
                table: "sales_allocations");

            migrationBuilder.DropColumn(
                name: "YieldRatioUsed",
                table: "sales_allocations");

            migrationBuilder.DropColumn(
                name: "Kind",
                table: "product_skus");

            migrationBuilder.DropColumn(
                name: "PortOfDischargeId",
                table: "export_shipments");

            migrationBuilder.DropColumn(
                name: "PortOfLoadingId",
                table: "export_shipments");

            migrationBuilder.DropColumn(
                name: "SalesInvoiceId",
                table: "deposit_allocations");

            migrationBuilder.CreateIndex(
                name: "IX_inventory_balances_LotId_SkuId",
                table: "inventory_balances",
                columns: new[] { "LotId", "SkuId" },
                unique: true);
        }
    }
}
