using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using SqlServ4r.EntityFramework;

#nullable disable

namespace SqlServ4r.EntityFramework.Migrations
{
    [DbContext(typeof(DreamContext))]
    [Migration("20260909120000_SeafoodUxFields")]
    public partial class SeafoodUxFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(name: "TruckName", table: "inbound_purchases", type: "character varying(128)", maxLength: 128, nullable: true);
            migrationBuilder.AddColumn<string>(name: "LotNumber", table: "inbound_purchases", type: "character varying(64)", maxLength: 64, nullable: true);
            migrationBuilder.AddColumn<string>(name: "CustomsDeclarationNo", table: "inbound_purchases", type: "character varying(64)", maxLength: 64, nullable: true);
            migrationBuilder.AddColumn<string>(name: "Origin", table: "inbound_purchases", type: "character varying(64)", maxLength: 64, nullable: true);
            migrationBuilder.AddColumn<string>(name: "CareMarket", table: "inbound_purchases", type: "character varying(64)", maxLength: 64, nullable: true);
            migrationBuilder.AddColumn<decimal>(name: "ActualWeightKg", table: "inbound_purchases", type: "numeric", nullable: true);
            migrationBuilder.AddColumn<string>(name: "ReceivedQtyNote", table: "inbound_purchases", type: "character varying(128)", maxLength: 128, nullable: true);
            migrationBuilder.AddColumn<string>(name: "RewriteQtyNote", table: "inbound_purchases", type: "character varying(128)", maxLength: 128, nullable: true);
            migrationBuilder.AddColumn<DateTime>(name: "ArrivalDate", table: "inbound_purchases", type: "timestamp with time zone", nullable: true);
            migrationBuilder.AddColumn<DateTime>(name: "PullContainerDate", table: "inbound_purchases", type: "timestamp with time zone", nullable: true);
            migrationBuilder.AddColumn<int>(name: "PortStayDays", table: "inbound_purchases", type: "integer", nullable: true);
            migrationBuilder.AddColumn<bool>(name: "RawFileImported", table: "inbound_purchases", type: "boolean", nullable: false, defaultValue: false);
            migrationBuilder.AddColumn<decimal>(name: "FeeCommand", table: "inbound_purchases", type: "numeric", nullable: false, defaultValue: 0m);
            migrationBuilder.AddColumn<decimal>(name: "FeeCold", table: "inbound_purchases", type: "numeric", nullable: false, defaultValue: 0m);
            migrationBuilder.AddColumn<decimal>(name: "FeeLift", table: "inbound_purchases", type: "numeric", nullable: false, defaultValue: 0m);
            migrationBuilder.AddColumn<decimal>(name: "FeeCustoms", table: "inbound_purchases", type: "numeric", nullable: false, defaultValue: 0m);
            migrationBuilder.AddColumn<decimal>(name: "FeeInfra", table: "inbound_purchases", type: "numeric", nullable: false, defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(name: "FxRateVnd", table: "production_lots", type: "numeric", nullable: false, defaultValue: 0m);
            migrationBuilder.AddColumn<decimal>(name: "PurchasePriceVnd", table: "production_lots", type: "numeric", nullable: false, defaultValue: 0m);
            migrationBuilder.AddColumn<decimal>(name: "AdjustedPriceVnd", table: "production_lots", type: "numeric", nullable: false, defaultValue: 0m);
            migrationBuilder.AddColumn<decimal>(name: "TrimKg", table: "production_lots", type: "numeric", nullable: false, defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(name: "UnitPriceUsd", table: "production_outputs", type: "numeric", nullable: false, defaultValue: 0m);
            migrationBuilder.AddColumn<decimal>(name: "AmountUsd", table: "production_outputs", type: "numeric", nullable: false, defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(name: "YardTime", table: "export_shipments", type: "timestamp with time zone", nullable: true);
            migrationBuilder.AddColumn<string>(name: "ForwarderName", table: "export_shipments", type: "character varying(256)", maxLength: 256, nullable: true);
            migrationBuilder.AddColumn<string>(name: "TransportName", table: "export_shipments", type: "character varying(256)", maxLength: 256, nullable: true);

            migrationBuilder.CreateTable(
                name: "lot_allocations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LotId = table.Column<int>(type: "integer", nullable: false),
                    SalesContractId = table.Column<int>(type: "integer", nullable: true),
                    SkuId = table.Column<int>(type: "integer", nullable: false),
                    AllocatedKg = table.Column<decimal>(type: "numeric", nullable: false),
                    YieldAfterAdjust = table.Column<decimal>(type: "numeric", nullable: false),
                    RawUsedAfterAdjust = table.Column<decimal>(type: "numeric", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    InternalNote = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_lot_allocations", x => x.Id);
                    table.ForeignKey(name: "FK_lot_allocations_production_lots_LotId", column: x => x.LotId, principalTable: "production_lots", principalColumn: "Id", onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(name: "FK_lot_allocations_sales_contracts_SalesContractId", column: x => x.SalesContractId, principalTable: "sales_contracts", principalColumn: "Id");
                    table.ForeignKey(name: "FK_lot_allocations_product_skus_SkuId", column: x => x.SkuId, principalTable: "product_skus", principalColumn: "Id", onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "report_favorites",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    ReportCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    PinnedToDashboard = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_report_favorites", x => x.Id);
                });

            migrationBuilder.CreateIndex(name: "IX_lot_allocations_LotId", table: "lot_allocations", column: "LotId");
            migrationBuilder.CreateIndex(name: "IX_lot_allocations_SalesContractId", table: "lot_allocations", column: "SalesContractId");
            migrationBuilder.CreateIndex(name: "IX_lot_allocations_SkuId", table: "lot_allocations", column: "SkuId");
            migrationBuilder.CreateIndex(name: "IX_report_favorites_UserId_ReportCode", table: "report_favorites", columns: new[] { "UserId", "ReportCode" }, unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "lot_allocations");
            migrationBuilder.DropTable(name: "report_favorites");
            migrationBuilder.DropColumn(name: "TruckName", table: "inbound_purchases");
            migrationBuilder.DropColumn(name: "LotNumber", table: "inbound_purchases");
            migrationBuilder.DropColumn(name: "CustomsDeclarationNo", table: "inbound_purchases");
            migrationBuilder.DropColumn(name: "Origin", table: "inbound_purchases");
            migrationBuilder.DropColumn(name: "CareMarket", table: "inbound_purchases");
            migrationBuilder.DropColumn(name: "ActualWeightKg", table: "inbound_purchases");
            migrationBuilder.DropColumn(name: "ReceivedQtyNote", table: "inbound_purchases");
            migrationBuilder.DropColumn(name: "RewriteQtyNote", table: "inbound_purchases");
            migrationBuilder.DropColumn(name: "ArrivalDate", table: "inbound_purchases");
            migrationBuilder.DropColumn(name: "PullContainerDate", table: "inbound_purchases");
            migrationBuilder.DropColumn(name: "PortStayDays", table: "inbound_purchases");
            migrationBuilder.DropColumn(name: "RawFileImported", table: "inbound_purchases");
            migrationBuilder.DropColumn(name: "FeeCommand", table: "inbound_purchases");
            migrationBuilder.DropColumn(name: "FeeCold", table: "inbound_purchases");
            migrationBuilder.DropColumn(name: "FeeLift", table: "inbound_purchases");
            migrationBuilder.DropColumn(name: "FeeCustoms", table: "inbound_purchases");
            migrationBuilder.DropColumn(name: "FeeInfra", table: "inbound_purchases");
            migrationBuilder.DropColumn(name: "FxRateVnd", table: "production_lots");
            migrationBuilder.DropColumn(name: "PurchasePriceVnd", table: "production_lots");
            migrationBuilder.DropColumn(name: "AdjustedPriceVnd", table: "production_lots");
            migrationBuilder.DropColumn(name: "TrimKg", table: "production_lots");
            migrationBuilder.DropColumn(name: "UnitPriceUsd", table: "production_outputs");
            migrationBuilder.DropColumn(name: "AmountUsd", table: "production_outputs");
            migrationBuilder.DropColumn(name: "YardTime", table: "export_shipments");
            migrationBuilder.DropColumn(name: "ForwarderName", table: "export_shipments");
            migrationBuilder.DropColumn(name: "TransportName", table: "export_shipments");
        }
    }
}
