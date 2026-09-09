using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SqlServ4r.EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class InitialPostgres : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "appconfigs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SearchVolumeVeryEasyRange = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    SearchVolumeEasyRange = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    SearchVolumeMediumRange = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    SearchVolumeHardLevel1Range = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    SearchVolumeHardLevel2Range = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    SearchVolumeHardLevel3Range = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    SearchVolumeHardLevel4Range = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    SearchVolumeHardLevel5Range = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    QuyTrinhDoiDomainHtml = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_appconfigs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "catalog_lookups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Category = table.Column<int>(type: "integer", nullable: false),
                    Code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_catalog_lookups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "customers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ContactName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Kind = table.Column<int>(type: "integer", nullable: false),
                    Purchaser = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    Note = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "data_protection_keys",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    friendly_name = table.Column<string>(type: "text", nullable: true),
                    xml = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_data_protection_keys", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "departments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "text", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Note = table.Column<string>(type: "text", nullable: true),
                    ODX = table.Column<int>(type: "integer", nullable: false),
                    ParentCode = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_departments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "market_certificate_rules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MarketCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    MustHaveCertificateCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    AddonCertificateCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    Note = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_market_certificate_rules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "menu_overrides",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ItemType = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    ItemKey = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ModuleText = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    Icon = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    TextOverride = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    UpdatedBy = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_menu_overrides", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "payment_terms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    RatiosJson = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    DueDays = table.Column<int>(type: "integer", nullable: true),
                    IsLc = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payment_terms", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "positions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "text", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Note = table.Column<string>(type: "text", nullable: true),
                    ODX = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_positions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "product_groups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    DefaultMarket = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_product_groups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "roles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "text", nullable: true),
                    default_page_url = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "teams",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_teams", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "totp_rate_limits",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IpAddress = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<int>(type: "integer", nullable: true),
                    FailureCount = table.Column<int>(type: "integer", nullable: false),
                    WindowStart = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LockedUntil = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_totp_rate_limits", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "webauthn_setting_history",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    is_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    is_enforced = table.Column<bool>(type: "boolean", nullable: false),
                    allow_password_login = table.Column<bool>(type: "boolean", nullable: false),
                    yubico_client_id = table.Column<string>(type: "text", nullable: true),
                    yubico_secret_key = table.Column<string>(type: "text", nullable: true),
                    changed_fields = table.Column<string>(type: "text", nullable: false),
                    changed_by = table.Column<string>(type: "text", nullable: true),
                    changed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_webauthn_setting_history", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "webauthn_settings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    is_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    is_enforced = table.Column<bool>(type: "boolean", nullable: false),
                    allow_password_login = table.Column<bool>(type: "boolean", nullable: false),
                    yubico_client_id = table.Column<string>(type: "text", nullable: true),
                    yubico_secret_key = table.Column<string>(type: "text", nullable: true),
                    updated_by = table.Column<string>(type: "text", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_webauthn_settings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "customer_deposits",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CustomerId = table.Column<int>(type: "integer", nullable: false),
                    AmountUsd = table.Column<decimal>(type: "numeric", nullable: false),
                    ReceivedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Note = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customer_deposits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_customer_deposits_customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "inbound_purchases",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CustomerId = table.Column<int>(type: "integer", nullable: true),
                    CustomerCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    ContractNo = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    Purchaser = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    MissingDocuments = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Eta = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    WarehouseDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    WarehouseId = table.Column<int>(type: "integer", nullable: true),
                    TargetMarketId = table.Column<int>(type: "integer", nullable: true),
                    PaymentTermId = table.Column<int>(type: "integer", nullable: true),
                    EstimatePaymentDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    BlNumber = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    ContainerNo = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    ContainerType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    Note = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_inbound_purchases", x => x.Id);
                    table.ForeignKey(
                        name: "FK_inbound_purchases_customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "customers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "sales_contracts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ContractNo = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CustomerContractNo = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    CustomerId = table.Column<int>(type: "integer", nullable: false),
                    MarketId = table.Column<int>(type: "integer", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    SuggestedUnitPriceUsd = table.Column<decimal>(type: "numeric", nullable: false),
                    VarianceVsLastPct = table.Column<decimal>(type: "numeric", nullable: false),
                    VarianceVsPeersPct = table.Column<decimal>(type: "numeric", nullable: false),
                    ApprovalNote = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    ApprovedBy = table.Column<int>(type: "integer", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sales_contracts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_sales_contracts_customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "product_skus",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GroupId = table.Column<int>(type: "integer", nullable: false),
                    Code = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ExportMarket = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    DefaultUnitPriceUsd = table.Column<decimal>(type: "numeric", nullable: false),
                    IsByproduct = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_product_skus", x => x.Id);
                    table.ForeignKey(
                        name: "FK_product_skus_product_groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "product_groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "roleclaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoleId = table.Column<int>(type: "integer", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_roleclaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_roleclaims_roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserCode = table.Column<string>(type: "text", nullable: false),
                    FirstName = table.Column<string>(type: "text", nullable: false),
                    LastName = table.Column<string>(type: "text", nullable: false),
                    Gender = table.Column<int>(type: "integer", nullable: false),
                    UserType = table.Column<int>(type: "integer", nullable: false),
                    DOB = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false),
                    RefreshToken = table.Column<string>(type: "text", nullable: true),
                    password_login_allowed = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    totp_secret_key = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    is_totp_enabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    totp_failed_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    totp_lockout_end = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AvatarURL = table.Column<string>(type: "text", nullable: true),
                    Address = table.Column<string>(type: "text", nullable: true),
                    DependsId = table.Column<int>(type: "integer", nullable: true),
                    UserDependId = table.Column<int>(type: "integer", nullable: true),
                    Relationship = table.Column<int>(type: "integer", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<int>(type: "integer", nullable: true),
                    ModifiedBy = table.Column<int>(type: "integer", nullable: true),
                    PositionId = table.Column<int>(type: "integer", nullable: true),
                    TeamId = table.Column<int>(type: "integer", nullable: true),
                    user_name_normalized = table.Column<string>(type: "text", nullable: false),
                    alias_name_ggsheet = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    UserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: true),
                    SecurityStamp = table.Column<string>(type: "text", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_users_positions_PositionId",
                        column: x => x.PositionId,
                        principalTable: "positions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_users_teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "teams",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_users_users_UserDependId",
                        column: x => x.UserDependId,
                        principalTable: "users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "inbound_lines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PurchaseId = table.Column<int>(type: "integer", nullable: false),
                    Commodity = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    QtyKgLongLine = table.Column<decimal>(type: "numeric", nullable: false),
                    QtyKgHandline = table.Column<decimal>(type: "numeric", nullable: false),
                    QtyKgPs = table.Column<decimal>(type: "numeric", nullable: false),
                    QtyKgLand = table.Column<decimal>(type: "numeric", nullable: false),
                    MahiKg = table.Column<decimal>(type: "numeric", nullable: false),
                    FinishedLbs = table.Column<decimal>(type: "numeric", nullable: false),
                    PriceUsd = table.Column<decimal>(type: "numeric", nullable: false),
                    InvoiceAmountUsd = table.Column<decimal>(type: "numeric", nullable: false),
                    ContainerQty = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_inbound_lines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_inbound_lines_inbound_purchases_PurchaseId",
                        column: x => x.PurchaseId,
                        principalTable: "inbound_purchases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "production_lots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LotNumber = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    InboundPurchaseId = table.Column<int>(type: "integer", nullable: true),
                    ReceivedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RawMaterialKg = table.Column<decimal>(type: "numeric", nullable: false),
                    TargetMarketId = table.Column<int>(type: "integer", nullable: true),
                    Note = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_production_lots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_production_lots_inbound_purchases_InboundPurchaseId",
                        column: x => x.InboundPurchaseId,
                        principalTable: "inbound_purchases",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "deposit_allocations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DepositId = table.Column<int>(type: "integer", nullable: false),
                    SalesContractId = table.Column<int>(type: "integer", nullable: true),
                    AmountUsd = table.Column<decimal>(type: "numeric", nullable: false),
                    IsExported = table.Column<bool>(type: "boolean", nullable: false),
                    ExportedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_deposit_allocations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_deposit_allocations_customer_deposits_DepositId",
                        column: x => x.DepositId,
                        principalTable: "customer_deposits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_deposit_allocations_sales_contracts_SalesContractId",
                        column: x => x.SalesContractId,
                        principalTable: "sales_contracts",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "export_shipments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SalesContractId = table.Column<int>(type: "integer", nullable: true),
                    CustomerId = table.Column<int>(type: "integer", nullable: true),
                    ProductionNoticeNo = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    PackingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Etd = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Eta = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    InvoiceNo = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    PaymentTermId = table.Column<int>(type: "integer", nullable: true),
                    Cartons = table.Column<int>(type: "integer", nullable: false),
                    QtyLbs = table.Column<decimal>(type: "numeric", nullable: false),
                    QtyKg = table.Column<decimal>(type: "numeric", nullable: false),
                    AmountUsd = table.Column<decimal>(type: "numeric", nullable: false),
                    ContainerNo = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    ContainerType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    CarrierId = table.Column<int>(type: "integer", nullable: true),
                    Route = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Note = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_export_shipments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_export_shipments_customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "customers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_export_shipments_payment_terms_PaymentTermId",
                        column: x => x.PaymentTermId,
                        principalTable: "payment_terms",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_export_shipments_sales_contracts_SalesContractId",
                        column: x => x.SalesContractId,
                        principalTable: "sales_contracts",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "sales_contract_lines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ContractId = table.Column<int>(type: "integer", nullable: false),
                    SkuId = table.Column<int>(type: "integer", nullable: false),
                    QtyKg = table.Column<decimal>(type: "numeric", nullable: false),
                    QtyLbs = table.Column<decimal>(type: "numeric", nullable: false),
                    UnitPriceUsd = table.Column<decimal>(type: "numeric", nullable: false),
                    AmountUsd = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sales_contract_lines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_sales_contract_lines_product_skus_SkuId",
                        column: x => x.SkuId,
                        principalTable: "product_skus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_sales_contract_lines_sales_contracts_ContractId",
                        column: x => x.ContractId,
                        principalTable: "sales_contracts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "apphistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IpAddress = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Functions = table.Column<string>(type: "text", nullable: false),
                    Operation = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_apphistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_apphistories_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_recovery_codes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    CodeHash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    CodeSalt = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    IsUsed = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UsedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_recovery_codes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_user_recovery_codes_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_security_keys",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    CredentialId = table.Column<byte[]>(type: "bytea", nullable: false),
                    PublicKey = table.Column<byte[]>(type: "bytea", nullable: false),
                    SignatureCounter = table.Column<long>(type: "bigint", nullable: false),
                    DeviceName = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastUsedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_security_keys", x => x.Id);
                    table.ForeignKey(
                        name: "FK_user_security_keys_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_yubikeys",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    PublicId = table.Column<string>(type: "text", nullable: false),
                    DeviceName = table.Column<string>(type: "text", nullable: false),
                    PinHash = table.Column<string>(type: "text", nullable: true),
                    PinFailedCount = table.Column<int>(type: "integer", nullable: false),
                    PinLockoutEnd = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastUsedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_yubikeys", x => x.Id);
                    table.ForeignKey(
                        name: "FK_user_yubikeys_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "userclaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_userclaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_userclaims_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "userdepartments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    DepartmentId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_userdepartments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_userdepartments_departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_userdepartments_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "userlogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    ProviderKey = table.Column<string>(type: "text", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_userlogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_userlogins_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "userroles",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    RoleId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_userroles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_userroles_roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_userroles_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "usertokens",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usertokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_usertokens_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "inventory_balances",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LotId = table.Column<int>(type: "integer", nullable: false),
                    SkuId = table.Column<int>(type: "integer", nullable: false),
                    OnHandKg = table.Column<decimal>(type: "numeric", nullable: false),
                    AllocatedKg = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_inventory_balances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_inventory_balances_product_skus_SkuId",
                        column: x => x.SkuId,
                        principalTable: "product_skus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_inventory_balances_production_lots_LotId",
                        column: x => x.LotId,
                        principalTable: "production_lots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "lot_certificates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LotId = table.Column<int>(type: "integer", nullable: false),
                    CertificateCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_lot_certificates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_lot_certificates_production_lots_LotId",
                        column: x => x.LotId,
                        principalTable: "production_lots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "production_outputs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LotId = table.Column<int>(type: "integer", nullable: false),
                    SkuId = table.Column<int>(type: "integer", nullable: false),
                    RecoveredKg = table.Column<decimal>(type: "numeric", nullable: false),
                    RawUsedKg = table.Column<decimal>(type: "numeric", nullable: false),
                    YieldRatio = table.Column<decimal>(type: "numeric", nullable: false),
                    ExportMarket = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_production_outputs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_production_outputs_product_skus_SkuId",
                        column: x => x.SkuId,
                        principalTable: "product_skus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_production_outputs_production_lots_LotId",
                        column: x => x.LotId,
                        principalTable: "production_lots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "payment_installments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ExportShipmentId = table.Column<int>(type: "integer", nullable: false),
                    Sequence = table.Column<int>(type: "integer", nullable: false),
                    Ratio = table.Column<decimal>(type: "numeric", nullable: false),
                    AmountUsd = table.Column<decimal>(type: "numeric", nullable: false),
                    DueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReceivedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReceivedAmountUsd = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payment_installments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_payment_installments_export_shipments_ExportShipmentId",
                        column: x => x.ExportShipmentId,
                        principalTable: "export_shipments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_apphistories_UserId",
                table: "apphistories",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_catalog_lookups_Category_Code",
                table: "catalog_lookups",
                columns: new[] { "Category", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_customer_deposits_CustomerId",
                table: "customer_deposits",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_customers_Code",
                table: "customers",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_deposit_allocations_DepositId",
                table: "deposit_allocations",
                column: "DepositId");

            migrationBuilder.CreateIndex(
                name: "IX_deposit_allocations_SalesContractId",
                table: "deposit_allocations",
                column: "SalesContractId");

            migrationBuilder.CreateIndex(
                name: "IX_export_shipments_CustomerId",
                table: "export_shipments",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_export_shipments_PaymentTermId",
                table: "export_shipments",
                column: "PaymentTermId");

            migrationBuilder.CreateIndex(
                name: "IX_export_shipments_SalesContractId",
                table: "export_shipments",
                column: "SalesContractId");

            migrationBuilder.CreateIndex(
                name: "IX_inbound_lines_PurchaseId",
                table: "inbound_lines",
                column: "PurchaseId");

            migrationBuilder.CreateIndex(
                name: "IX_inbound_purchases_CustomerId",
                table: "inbound_purchases",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_inventory_balances_LotId_SkuId",
                table: "inventory_balances",
                columns: new[] { "LotId", "SkuId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_inventory_balances_SkuId",
                table: "inventory_balances",
                column: "SkuId");

            migrationBuilder.CreateIndex(
                name: "IX_lot_certificates_LotId",
                table: "lot_certificates",
                column: "LotId");

            migrationBuilder.CreateIndex(
                name: "IX_market_certificate_rules_MarketCode",
                table: "market_certificate_rules",
                column: "MarketCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_payment_installments_ExportShipmentId",
                table: "payment_installments",
                column: "ExportShipmentId");

            migrationBuilder.CreateIndex(
                name: "IX_payment_terms_Code",
                table: "payment_terms",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_product_groups_Code",
                table: "product_groups",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_product_skus_Code",
                table: "product_skus",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_product_skus_GroupId",
                table: "product_skus",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_production_lots_InboundPurchaseId",
                table: "production_lots",
                column: "InboundPurchaseId");

            migrationBuilder.CreateIndex(
                name: "IX_production_lots_LotNumber",
                table: "production_lots",
                column: "LotNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_production_outputs_LotId",
                table: "production_outputs",
                column: "LotId");

            migrationBuilder.CreateIndex(
                name: "IX_production_outputs_SkuId",
                table: "production_outputs",
                column: "SkuId");

            migrationBuilder.CreateIndex(
                name: "IX_roleclaims_RoleId",
                table: "roleclaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "roles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_sales_contract_lines_ContractId",
                table: "sales_contract_lines",
                column: "ContractId");

            migrationBuilder.CreateIndex(
                name: "IX_sales_contract_lines_SkuId",
                table: "sales_contract_lines",
                column: "SkuId");

            migrationBuilder.CreateIndex(
                name: "IX_sales_contracts_ContractNo",
                table: "sales_contracts",
                column: "ContractNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_sales_contracts_CustomerId",
                table: "sales_contracts",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_user_recovery_codes_UserId",
                table: "user_recovery_codes",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_user_security_keys_UserId_CredentialId",
                table: "user_security_keys",
                columns: new[] { "UserId", "CredentialId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_yubikeys_PublicId",
                table: "user_yubikeys",
                column: "PublicId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_yubikeys_UserId",
                table: "user_yubikeys",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_userclaims_UserId",
                table: "userclaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_userdepartments_DepartmentId",
                table: "userdepartments",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_userdepartments_UserId",
                table: "userdepartments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_userlogins_UserId",
                table: "userlogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_userroles_RoleId",
                table: "userroles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "users",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "IX_users_Email",
                table: "users",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "ix_users_is_totp_enabled",
                table: "users",
                column: "is_totp_enabled");

            migrationBuilder.CreateIndex(
                name: "IX_users_PhoneNumber",
                table: "users",
                column: "PhoneNumber");

            migrationBuilder.CreateIndex(
                name: "IX_users_PositionId",
                table: "users",
                column: "PositionId");

            migrationBuilder.CreateIndex(
                name: "IX_users_TeamId",
                table: "users",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_users_UserCode",
                table: "users",
                column: "UserCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_UserDependId",
                table: "users",
                column: "UserDependId");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "users",
                column: "NormalizedUserName",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "appconfigs");

            migrationBuilder.DropTable(
                name: "apphistories");

            migrationBuilder.DropTable(
                name: "catalog_lookups");

            migrationBuilder.DropTable(
                name: "data_protection_keys");

            migrationBuilder.DropTable(
                name: "deposit_allocations");

            migrationBuilder.DropTable(
                name: "inbound_lines");

            migrationBuilder.DropTable(
                name: "inventory_balances");

            migrationBuilder.DropTable(
                name: "lot_certificates");

            migrationBuilder.DropTable(
                name: "market_certificate_rules");

            migrationBuilder.DropTable(
                name: "menu_overrides");

            migrationBuilder.DropTable(
                name: "payment_installments");

            migrationBuilder.DropTable(
                name: "production_outputs");

            migrationBuilder.DropTable(
                name: "roleclaims");

            migrationBuilder.DropTable(
                name: "sales_contract_lines");

            migrationBuilder.DropTable(
                name: "totp_rate_limits");

            migrationBuilder.DropTable(
                name: "user_recovery_codes");

            migrationBuilder.DropTable(
                name: "user_security_keys");

            migrationBuilder.DropTable(
                name: "user_yubikeys");

            migrationBuilder.DropTable(
                name: "userclaims");

            migrationBuilder.DropTable(
                name: "userdepartments");

            migrationBuilder.DropTable(
                name: "userlogins");

            migrationBuilder.DropTable(
                name: "userroles");

            migrationBuilder.DropTable(
                name: "usertokens");

            migrationBuilder.DropTable(
                name: "webauthn_setting_history");

            migrationBuilder.DropTable(
                name: "webauthn_settings");

            migrationBuilder.DropTable(
                name: "customer_deposits");

            migrationBuilder.DropTable(
                name: "export_shipments");

            migrationBuilder.DropTable(
                name: "production_lots");

            migrationBuilder.DropTable(
                name: "product_skus");

            migrationBuilder.DropTable(
                name: "departments");

            migrationBuilder.DropTable(
                name: "roles");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "payment_terms");

            migrationBuilder.DropTable(
                name: "sales_contracts");

            migrationBuilder.DropTable(
                name: "inbound_purchases");

            migrationBuilder.DropTable(
                name: "product_groups");

            migrationBuilder.DropTable(
                name: "positions");

            migrationBuilder.DropTable(
                name: "teams");

            migrationBuilder.DropTable(
                name: "customers");
        }
    }
}
