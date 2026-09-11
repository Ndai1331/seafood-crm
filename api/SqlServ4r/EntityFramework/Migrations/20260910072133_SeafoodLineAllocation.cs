using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SqlServ4r.EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class SeafoodLineAllocation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "AllocatedKg",
                table: "sales_contract_lines",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "CostUsd",
                table: "production_outputs",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SalesInvoiceId",
                table: "payment_installments",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WarehouseId",
                table: "inventory_balances",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ActualWeightKg",
                table: "inbound_purchases",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PurchaseInvoiceNo",
                table: "inbound_purchases",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ReceivedWeightKg",
                table: "inbound_purchases",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SupplierPartnerId",
                table: "inbound_purchases",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "VesselId",
                table: "inbound_purchases",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ActualWeightKg",
                table: "inbound_lines",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "CatchMethodCode",
                table: "inbound_lines",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FishFormCode",
                table: "inbound_lines",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FishSpeciesCode",
                table: "inbound_lines",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FreezeMethodCode",
                table: "inbound_lines",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OriginCode",
                table: "inbound_lines",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "QtyKg",
                table: "inbound_lines",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "SensoryCode",
                table: "inbound_lines",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SizeCode",
                table: "inbound_lines",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "export_shipments",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "business_partners",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ContactName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    TaxCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    Note = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_business_partners", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "document_types",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_document_types", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "domain_audit_logs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EntityType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    EntityId = table.Column<int>(type: "integer", nullable: false),
                    Action = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ChangesJson = table.Column<string>(type: "text", nullable: true),
                    Reason = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_domain_audit_logs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "exchange_rate_snapshots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FromCurrency = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    ToCurrency = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    Rate = table.Column<decimal>(type: "numeric", nullable: false),
                    EffectiveDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Source = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exchange_rate_snapshots", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "inventory_movements",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    InventoryBalanceId = table.Column<int>(type: "integer", nullable: false),
                    MovementType = table.Column<int>(type: "integer", nullable: false),
                    QuantityKg = table.Column<decimal>(type: "numeric", nullable: false),
                    ReferenceType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ReferenceId = table.Column<int>(type: "integer", nullable: true),
                    OccurredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "integer", nullable: true),
                    Reason = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_inventory_movements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_inventory_movements_inventory_balances_InventoryBalanceId",
                        column: x => x.InventoryBalanceId,
                        principalTable: "inventory_balances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "sales_allocations",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SalesContractLineId = table.Column<int>(type: "integer", nullable: false),
                    InventoryBalanceId = table.Column<int>(type: "integer", nullable: false),
                    ShipmentId = table.Column<int>(type: "integer", nullable: true),
                    QuantityKg = table.Column<decimal>(type: "numeric", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sales_allocations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_sales_allocations_export_shipments_ShipmentId",
                        column: x => x.ShipmentId,
                        principalTable: "export_shipments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_sales_allocations_inventory_balances_InventoryBalanceId",
                        column: x => x.InventoryBalanceId,
                        principalTable: "inventory_balances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_sales_allocations_sales_contract_lines_SalesContractLineId",
                        column: x => x.SalesContractLineId,
                        principalTable: "sales_contract_lines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "sales_invoices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    InvoiceNo = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SalesContractId = table.Column<int>(type: "integer", nullable: true),
                    ShipmentId = table.Column<int>(type: "integer", nullable: true),
                    CustomerId = table.Column<int>(type: "integer", nullable: true),
                    AmountUsd = table.Column<decimal>(type: "numeric", nullable: false),
                    IssueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sales_invoices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_sales_invoices_customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_sales_invoices_export_shipments_ShipmentId",
                        column: x => x.ShipmentId,
                        principalTable: "export_shipments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_sales_invoices_sales_contracts_SalesContractId",
                        column: x => x.SalesContractId,
                        principalTable: "sales_contracts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "shipment_containers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ShipmentId = table.Column<int>(type: "integer", nullable: false),
                    ContainerNo = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ContainerType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    SealNo = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    Cartons = table.Column<int>(type: "integer", nullable: false),
                    QtyKg = table.Column<decimal>(type: "numeric", nullable: false),
                    QtyLbs = table.Column<decimal>(type: "numeric", nullable: false),
                    LoadingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsConfirmed = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_shipment_containers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_shipment_containers_export_shipments_ShipmentId",
                        column: x => x.ShipmentId,
                        principalTable: "export_shipments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "stock_reservations",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SalesContractLineId = table.Column<int>(type: "integer", nullable: false),
                    InventoryBalanceId = table.Column<int>(type: "integer", nullable: false),
                    QuantityKg = table.Column<decimal>(type: "numeric", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ShipmentId = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "integer", nullable: true),
                    ReleasedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Reason = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_stock_reservations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_stock_reservations_export_shipments_ShipmentId",
                        column: x => x.ShipmentId,
                        principalTable: "export_shipments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_stock_reservations_inventory_balances_InventoryBalanceId",
                        column: x => x.InventoryBalanceId,
                        principalTable: "inventory_balances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_stock_reservations_sales_contract_lines_SalesContractLineId",
                        column: x => x.SalesContractLineId,
                        principalTable: "sales_contract_lines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "traceability_links",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FromType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    FromId = table.Column<int>(type: "integer", nullable: false),
                    ToType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ToId = table.Column<int>(type: "integer", nullable: false),
                    QuantityKg = table.Column<decimal>(type: "numeric", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_traceability_links", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "business_partner_roles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BusinessPartnerId = table.Column<int>(type: "integer", nullable: false),
                    Role = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_business_partner_roles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_business_partner_roles_business_partners_BusinessPartnerId",
                        column: x => x.BusinessPartnerId,
                        principalTable: "business_partners",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "vessels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Flag = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    OwnerPartnerId = table.Column<int>(type: "integer", nullable: true),
                    Note = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vessels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_vessels_business_partners_OwnerPartnerId",
                        column: x => x.OwnerPartnerId,
                        principalTable: "business_partners",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "document_requirements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MarketCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    DocumentTypeId = table.Column<int>(type: "integer", nullable: false),
                    IsRequired = table.Column<bool>(type: "boolean", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    Note = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_document_requirements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_document_requirements_document_types_DocumentTypeId",
                        column: x => x.DocumentTypeId,
                        principalTable: "document_types",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "payment_transactions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CustomerId = table.Column<int>(type: "integer", nullable: true),
                    InvoiceId = table.Column<int>(type: "integer", nullable: true),
                    ReferenceNo = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    AmountUsd = table.Column<decimal>(type: "numeric", nullable: false),
                    ReceivedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Note = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payment_transactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_payment_transactions_customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_payment_transactions_sales_invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "sales_invoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "raw_material_lots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LotNumber = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    InboundPurchaseId = table.Column<int>(type: "integer", nullable: false),
                    InboundLineId = table.Column<int>(type: "integer", nullable: false),
                    SupplierPartnerId = table.Column<int>(type: "integer", nullable: true),
                    VesselId = table.Column<int>(type: "integer", nullable: true),
                    ReceivedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    WarehouseId = table.Column<int>(type: "integer", nullable: true),
                    TargetMarketId = table.Column<int>(type: "integer", nullable: true),
                    FishSpeciesCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    FishFormCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    SizeCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    SensoryCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    CatchMethodCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    FreezeMethodCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    OriginCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    DeclaredKg = table.Column<decimal>(type: "numeric", nullable: false),
                    ActualKg = table.Column<decimal>(type: "numeric", nullable: false),
                    Note = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_raw_material_lots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_raw_material_lots_business_partners_SupplierPartnerId",
                        column: x => x.SupplierPartnerId,
                        principalTable: "business_partners",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_raw_material_lots_inbound_lines_InboundLineId",
                        column: x => x.InboundLineId,
                        principalTable: "inbound_lines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_raw_material_lots_inbound_purchases_InboundPurchaseId",
                        column: x => x.InboundPurchaseId,
                        principalTable: "inbound_purchases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_raw_material_lots_vessels_VesselId",
                        column: x => x.VesselId,
                        principalTable: "vessels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "payment_allocations",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PaymentTransactionId = table.Column<long>(type: "bigint", nullable: false),
                    PaymentInstallmentId = table.Column<int>(type: "integer", nullable: true),
                    InvoiceId = table.Column<int>(type: "integer", nullable: true),
                    AmountUsd = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payment_allocations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_payment_allocations_payment_installments_PaymentInstallment~",
                        column: x => x.PaymentInstallmentId,
                        principalTable: "payment_installments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_payment_allocations_payment_transactions_PaymentTransaction~",
                        column: x => x.PaymentTransactionId,
                        principalTable: "payment_transactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_payment_allocations_sales_invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "sales_invoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "document_attachments",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OwnerType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    OwnerId = table.Column<int>(type: "integer", nullable: false),
                    DocumentTypeId = table.Column<int>(type: "integer", nullable: true),
                    DocumentNo = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    IssueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExpiryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ObjectKey = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    FileName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    UploadedByUserId = table.Column<int>(type: "integer", nullable: true),
                    VerifiedByUserId = table.Column<int>(type: "integer", nullable: true),
                    UploadedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    VerifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Note = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    InboundPurchaseId = table.Column<int>(type: "integer", nullable: true),
                    ProductionLotId = table.Column<int>(type: "integer", nullable: true),
                    RawMaterialLotId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_document_attachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_document_attachments_document_types_DocumentTypeId",
                        column: x => x.DocumentTypeId,
                        principalTable: "document_types",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_document_attachments_inbound_purchases_InboundPurchaseId",
                        column: x => x.InboundPurchaseId,
                        principalTable: "inbound_purchases",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_document_attachments_production_lots_ProductionLotId",
                        column: x => x.ProductionLotId,
                        principalTable: "production_lots",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_document_attachments_raw_material_lots_RawMaterialLotId",
                        column: x => x.RawMaterialLotId,
                        principalTable: "raw_material_lots",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "production_inputs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProductionLotId = table.Column<int>(type: "integer", nullable: false),
                    RawMaterialLotId = table.Column<int>(type: "integer", nullable: false),
                    QuantityKg = table.Column<decimal>(type: "numeric", nullable: false),
                    Reason = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_production_inputs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_production_inputs_production_lots_ProductionLotId",
                        column: x => x.ProductionLotId,
                        principalTable: "production_lots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_production_inputs_raw_material_lots_RawMaterialLotId",
                        column: x => x.RawMaterialLotId,
                        principalTable: "raw_material_lots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "shipment_documents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ShipmentId = table.Column<int>(type: "integer", nullable: false),
                    DocumentAttachmentId = table.Column<long>(type: "bigint", nullable: false),
                    IsRequired = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_shipment_documents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_shipment_documents_document_attachments_DocumentAttachmentId",
                        column: x => x.DocumentAttachmentId,
                        principalTable: "document_attachments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_shipment_documents_export_shipments_ShipmentId",
                        column: x => x.ShipmentId,
                        principalTable: "export_shipments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_payment_installments_SalesInvoiceId",
                table: "payment_installments",
                column: "SalesInvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_inbound_purchases_SupplierPartnerId",
                table: "inbound_purchases",
                column: "SupplierPartnerId");

            migrationBuilder.CreateIndex(
                name: "IX_inbound_purchases_VesselId",
                table: "inbound_purchases",
                column: "VesselId");

            migrationBuilder.CreateIndex(
                name: "IX_business_partner_roles_BusinessPartnerId_Role",
                table: "business_partner_roles",
                columns: new[] { "BusinessPartnerId", "Role" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_business_partners_Code",
                table: "business_partners",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_document_attachments_DocumentTypeId",
                table: "document_attachments",
                column: "DocumentTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_document_attachments_InboundPurchaseId",
                table: "document_attachments",
                column: "InboundPurchaseId");

            migrationBuilder.CreateIndex(
                name: "IX_document_attachments_OwnerType_OwnerId",
                table: "document_attachments",
                columns: new[] { "OwnerType", "OwnerId" });

            migrationBuilder.CreateIndex(
                name: "IX_document_attachments_ProductionLotId",
                table: "document_attachments",
                column: "ProductionLotId");

            migrationBuilder.CreateIndex(
                name: "IX_document_attachments_RawMaterialLotId",
                table: "document_attachments",
                column: "RawMaterialLotId");

            migrationBuilder.CreateIndex(
                name: "IX_document_requirements_DocumentTypeId",
                table: "document_requirements",
                column: "DocumentTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_document_requirements_MarketCode_DocumentTypeId",
                table: "document_requirements",
                columns: new[] { "MarketCode", "DocumentTypeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_document_types_Code",
                table: "document_types",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_domain_audit_logs_EntityType_EntityId_CreatedAt",
                table: "domain_audit_logs",
                columns: new[] { "EntityType", "EntityId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_exchange_rate_snapshots_FromCurrency_ToCurrency_EffectiveDa~",
                table: "exchange_rate_snapshots",
                columns: new[] { "FromCurrency", "ToCurrency", "EffectiveDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_inventory_movements_InventoryBalanceId",
                table: "inventory_movements",
                column: "InventoryBalanceId");

            migrationBuilder.CreateIndex(
                name: "IX_inventory_movements_ReferenceType_ReferenceId",
                table: "inventory_movements",
                columns: new[] { "ReferenceType", "ReferenceId" });

            migrationBuilder.CreateIndex(
                name: "IX_payment_allocations_InvoiceId",
                table: "payment_allocations",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_payment_allocations_PaymentInstallmentId",
                table: "payment_allocations",
                column: "PaymentInstallmentId");

            migrationBuilder.CreateIndex(
                name: "IX_payment_allocations_PaymentTransactionId",
                table: "payment_allocations",
                column: "PaymentTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_payment_transactions_CustomerId",
                table: "payment_transactions",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_payment_transactions_InvoiceId",
                table: "payment_transactions",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_production_inputs_ProductionLotId_RawMaterialLotId",
                table: "production_inputs",
                columns: new[] { "ProductionLotId", "RawMaterialLotId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_production_inputs_RawMaterialLotId",
                table: "production_inputs",
                column: "RawMaterialLotId");

            migrationBuilder.CreateIndex(
                name: "IX_raw_material_lots_InboundLineId",
                table: "raw_material_lots",
                column: "InboundLineId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_raw_material_lots_InboundPurchaseId",
                table: "raw_material_lots",
                column: "InboundPurchaseId");

            migrationBuilder.CreateIndex(
                name: "IX_raw_material_lots_LotNumber",
                table: "raw_material_lots",
                column: "LotNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_raw_material_lots_SupplierPartnerId",
                table: "raw_material_lots",
                column: "SupplierPartnerId");

            migrationBuilder.CreateIndex(
                name: "IX_raw_material_lots_VesselId",
                table: "raw_material_lots",
                column: "VesselId");

            migrationBuilder.CreateIndex(
                name: "IX_sales_allocations_InventoryBalanceId",
                table: "sales_allocations",
                column: "InventoryBalanceId");

            migrationBuilder.CreateIndex(
                name: "IX_sales_allocations_SalesContractLineId_InventoryBalanceId_Sh~",
                table: "sales_allocations",
                columns: new[] { "SalesContractLineId", "InventoryBalanceId", "ShipmentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_sales_allocations_ShipmentId",
                table: "sales_allocations",
                column: "ShipmentId");

            migrationBuilder.CreateIndex(
                name: "IX_sales_invoices_CustomerId",
                table: "sales_invoices",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_sales_invoices_InvoiceNo",
                table: "sales_invoices",
                column: "InvoiceNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_sales_invoices_SalesContractId",
                table: "sales_invoices",
                column: "SalesContractId");

            migrationBuilder.CreateIndex(
                name: "IX_sales_invoices_ShipmentId",
                table: "sales_invoices",
                column: "ShipmentId");

            migrationBuilder.CreateIndex(
                name: "IX_shipment_containers_ContainerNo",
                table: "shipment_containers",
                column: "ContainerNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_shipment_containers_ShipmentId",
                table: "shipment_containers",
                column: "ShipmentId");

            migrationBuilder.CreateIndex(
                name: "IX_shipment_documents_DocumentAttachmentId",
                table: "shipment_documents",
                column: "DocumentAttachmentId");

            migrationBuilder.CreateIndex(
                name: "IX_shipment_documents_ShipmentId_DocumentAttachmentId",
                table: "shipment_documents",
                columns: new[] { "ShipmentId", "DocumentAttachmentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_stock_reservations_InventoryBalanceId",
                table: "stock_reservations",
                column: "InventoryBalanceId");

            migrationBuilder.CreateIndex(
                name: "IX_stock_reservations_SalesContractLineId_InventoryBalanceId_S~",
                table: "stock_reservations",
                columns: new[] { "SalesContractLineId", "InventoryBalanceId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_stock_reservations_ShipmentId",
                table: "stock_reservations",
                column: "ShipmentId");

            migrationBuilder.CreateIndex(
                name: "IX_traceability_links_FromType_FromId",
                table: "traceability_links",
                columns: new[] { "FromType", "FromId" });

            migrationBuilder.CreateIndex(
                name: "IX_traceability_links_ToType_ToId",
                table: "traceability_links",
                columns: new[] { "ToType", "ToId" });

            migrationBuilder.CreateIndex(
                name: "IX_vessels_Code",
                table: "vessels",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_vessels_OwnerPartnerId",
                table: "vessels",
                column: "OwnerPartnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_inbound_purchases_business_partners_SupplierPartnerId",
                table: "inbound_purchases",
                column: "SupplierPartnerId",
                principalTable: "business_partners",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_inbound_purchases_vessels_VesselId",
                table: "inbound_purchases",
                column: "VesselId",
                principalTable: "vessels",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_payment_installments_sales_invoices_SalesInvoiceId",
                table: "payment_installments",
                column: "SalesInvoiceId",
                principalTable: "sales_invoices",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_inbound_purchases_business_partners_SupplierPartnerId",
                table: "inbound_purchases");

            migrationBuilder.DropForeignKey(
                name: "FK_inbound_purchases_vessels_VesselId",
                table: "inbound_purchases");

            migrationBuilder.DropForeignKey(
                name: "FK_payment_installments_sales_invoices_SalesInvoiceId",
                table: "payment_installments");

            migrationBuilder.DropTable(
                name: "business_partner_roles");

            migrationBuilder.DropTable(
                name: "document_requirements");

            migrationBuilder.DropTable(
                name: "domain_audit_logs");

            migrationBuilder.DropTable(
                name: "exchange_rate_snapshots");

            migrationBuilder.DropTable(
                name: "inventory_movements");

            migrationBuilder.DropTable(
                name: "payment_allocations");

            migrationBuilder.DropTable(
                name: "production_inputs");

            migrationBuilder.DropTable(
                name: "sales_allocations");

            migrationBuilder.DropTable(
                name: "shipment_containers");

            migrationBuilder.DropTable(
                name: "shipment_documents");

            migrationBuilder.DropTable(
                name: "stock_reservations");

            migrationBuilder.DropTable(
                name: "traceability_links");

            migrationBuilder.DropTable(
                name: "payment_transactions");

            migrationBuilder.DropTable(
                name: "document_attachments");

            migrationBuilder.DropTable(
                name: "sales_invoices");

            migrationBuilder.DropTable(
                name: "document_types");

            migrationBuilder.DropTable(
                name: "raw_material_lots");

            migrationBuilder.DropTable(
                name: "vessels");

            migrationBuilder.DropTable(
                name: "business_partners");

            migrationBuilder.DropIndex(
                name: "IX_payment_installments_SalesInvoiceId",
                table: "payment_installments");

            migrationBuilder.DropIndex(
                name: "IX_inbound_purchases_SupplierPartnerId",
                table: "inbound_purchases");

            migrationBuilder.DropIndex(
                name: "IX_inbound_purchases_VesselId",
                table: "inbound_purchases");

            migrationBuilder.DropColumn(
                name: "AllocatedKg",
                table: "sales_contract_lines");

            migrationBuilder.DropColumn(
                name: "CostUsd",
                table: "production_outputs");

            migrationBuilder.DropColumn(
                name: "SalesInvoiceId",
                table: "payment_installments");

            migrationBuilder.DropColumn(
                name: "WarehouseId",
                table: "inventory_balances");

            migrationBuilder.DropColumn(
                name: "ActualWeightKg",
                table: "inbound_purchases");

            migrationBuilder.DropColumn(
                name: "PurchaseInvoiceNo",
                table: "inbound_purchases");

            migrationBuilder.DropColumn(
                name: "ReceivedWeightKg",
                table: "inbound_purchases");

            migrationBuilder.DropColumn(
                name: "SupplierPartnerId",
                table: "inbound_purchases");

            migrationBuilder.DropColumn(
                name: "VesselId",
                table: "inbound_purchases");

            migrationBuilder.DropColumn(
                name: "ActualWeightKg",
                table: "inbound_lines");

            migrationBuilder.DropColumn(
                name: "CatchMethodCode",
                table: "inbound_lines");

            migrationBuilder.DropColumn(
                name: "FishFormCode",
                table: "inbound_lines");

            migrationBuilder.DropColumn(
                name: "FishSpeciesCode",
                table: "inbound_lines");

            migrationBuilder.DropColumn(
                name: "FreezeMethodCode",
                table: "inbound_lines");

            migrationBuilder.DropColumn(
                name: "OriginCode",
                table: "inbound_lines");

            migrationBuilder.DropColumn(
                name: "QtyKg",
                table: "inbound_lines");

            migrationBuilder.DropColumn(
                name: "SensoryCode",
                table: "inbound_lines");

            migrationBuilder.DropColumn(
                name: "SizeCode",
                table: "inbound_lines");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "export_shipments");
        }
    }
}
