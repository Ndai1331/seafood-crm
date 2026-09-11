using Domain.AppConfigs;
using Domain.AppHistories;
using Domain.Departments;
using Domain.Identity.RoleClaims;
using Domain.Identity.Roles;
using Domain.Identity.TotpRateLimits;
using Domain.Identity.UserClaim;
using Domain.Identity.UserLogins;
using Domain.Identity.UserRecoveryCodes;
using Domain.Identity.UserRoles;
using Domain.Identity.UserSecurityKeys;
using Domain.Identity.UserTokens;
using Domain.Identity.UserYubikeys;
using Domain.Identity.Users;
using Domain.MenuLayout;
using Domain.Positions;
using Domain.Seafood;
using Domain.Teams;
using Domain.UserDepartments;
using Domain.WebAuthnSettings;
using Core.Helper;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace SqlServ4r.EntityFramework
{
    public class DreamContext
        : IdentityDbContext<User, Role, int, UserClaim, UserRole, UserLogin, RoleClaim, UserToken>,
          IDataProtectionKeyContext
    {
        public DbSet<DataProtectionKey> DataProtectionKeys { get; set; } = null!;
        public DbSet<AppHistory> AppHistories { get; set; } = null!;
        public DbSet<Position> Positions { get; set; } = null!;
        public DbSet<Team> Teams { get; set; } = null!;
        public DbSet<Department> Departments { get; set; } = null!;
        public DbSet<UserDepartment> UserDepartments { get; set; } = null!;
        public DbSet<UserRecoveryCode> UserRecoveryCodes { get; set; } = null!;
        public DbSet<TotpRateLimit> TotpRateLimits { get; set; } = null!;
        public DbSet<UserSecurityKey> UserSecurityKeys { get; set; } = null!;
        public DbSet<UserYubikey> UserYubikeys { get; set; } = null!;
        public DbSet<WebAuthnSetting> WebAuthnSettings { get; set; } = null!;
        public DbSet<WebAuthnSettingHistory> WebAuthnSettingHistories { get; set; } = null!;
        public DbSet<AppConfig> AppConfigs { get; set; } = null!;
        public DbSet<MenuOverride> MenuOverrides { get; set; } = null!;

        public DbSet<CatalogLookup> CatalogLookups { get; set; } = null!;
        public DbSet<MarketCertificateRule> MarketCertificateRules { get; set; } = null!;
        public DbSet<ProductGroup> ProductGroups { get; set; } = null!;
        public DbSet<ProductSku> ProductSkus { get; set; } = null!;
        public DbSet<PaymentTerm> PaymentTerms { get; set; } = null!;
        public DbSet<Customer> Customers { get; set; } = null!;
        public DbSet<BusinessPartner> BusinessPartners { get; set; } = null!;
        public DbSet<BusinessPartnerRoleLink> BusinessPartnerRoles { get; set; } = null!;
        public DbSet<Vessel> Vessels { get; set; } = null!;
        public DbSet<DocumentType> DocumentTypes { get; set; } = null!;
        public DbSet<DocumentRequirement> DocumentRequirements { get; set; } = null!;
        public DbSet<DocumentAttachment> DocumentAttachments { get; set; } = null!;
        public DbSet<InboundPurchase> InboundPurchases { get; set; } = null!;
        public DbSet<InboundLine> InboundLines { get; set; } = null!;
        public DbSet<RawMaterialLot> RawMaterialLots { get; set; } = null!;
        public DbSet<ProductionLot> ProductionLots { get; set; } = null!;
        public DbSet<ProductionInput> ProductionInputs { get; set; } = null!;
        public DbSet<ProductionOutput> ProductionOutputs { get; set; } = null!;
        public DbSet<LotCertificate> LotCertificates { get; set; } = null!;
        public DbSet<InventoryBalance> InventoryBalances { get; set; } = null!;
        public DbSet<InventoryMovement> InventoryMovements { get; set; } = null!;
        public DbSet<StockReservation> StockReservations { get; set; } = null!;
        public DbSet<SalesAllocation> SalesAllocations { get; set; } = null!;
        public DbSet<TraceabilityLink> TraceabilityLinks { get; set; } = null!;
        public DbSet<SalesContract> SalesContracts { get; set; } = null!;
        public DbSet<SalesContractLine> SalesContractLines { get; set; } = null!;
        public DbSet<ExportShipment> ExportShipments { get; set; } = null!;
        public DbSet<ShipmentContainer> ShipmentContainers { get; set; } = null!;
        public DbSet<ShipmentDocument> ShipmentDocuments { get; set; } = null!;
        public DbSet<SalesInvoice> SalesInvoices { get; set; } = null!;
        public DbSet<PaymentInstallment> PaymentInstallments { get; set; } = null!;
        public DbSet<PaymentTransaction> PaymentTransactions { get; set; } = null!;
        public DbSet<PaymentAllocation> PaymentAllocations { get; set; } = null!;
        public DbSet<CustomerDeposit> CustomerDeposits { get; set; } = null!;
        public DbSet<DepositAllocation> DepositAllocations { get; set; } = null!;
        public DbSet<ExchangeRateSnapshot> ExchangeRateSnapshots { get; set; } = null!;
        public DbSet<DomainAuditLog> DomainAuditLogs { get; set; } = null!;
        public DbSet<ImportBatch> ImportBatches { get; set; } = null!;
        public DbSet<ImportStagingRow> ImportStagingRows { get; set; } = null!;

        public DreamContext(DbContextOptions<DreamContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            foreach (var entityType in builder.Model.GetEntityTypes())
            {
                var tableName = entityType.GetTableName();
                if (!string.IsNullOrEmpty(tableName) && tableName.StartsWith("AspNet"))
                {
                    entityType.SetTableName(tableName.Substring(6).ToLower());
                }
            }

            builder.Entity<DataProtectionKey>(entity =>
            {
                entity.ToTable("data_protection_keys");
                entity.Property(p => p.Id).HasColumnName("id");
                entity.Property(p => p.FriendlyName).HasColumnName("friendly_name");
                entity.Property(p => p.Xml).HasColumnName("xml");
            });

            builder.Entity<Role>(entity =>
            {
                entity.Property(p => p.DefaultPageUrl)
                    .HasColumnName("default_page_url")
                    .HasMaxLength(256);
            });

            builder.Entity<User>(entity =>
            {
                entity.Property(p => p.TotpSecretKey).HasColumnName("totp_secret_key").HasMaxLength(512);
                entity.Property(p => p.IsTotpEnabled).HasColumnName("is_totp_enabled").HasDefaultValue(false);
                entity.Property(p => p.TotpFailedCount).HasColumnName("totp_failed_count").HasDefaultValue(0);
                entity.Property(p => p.TotpLockoutEnd).HasColumnName("totp_lockout_end");
                entity.Property(p => p.AliasNameGgsheet).HasColumnName("alias_name_ggsheet").HasMaxLength(255);
                entity.Property(p => p.PasswordLoginAllowed).HasColumnName("password_login_allowed").HasDefaultValue(false);
                entity.HasIndex(p => p.IsTotpEnabled).HasDatabaseName("ix_users_is_totp_enabled");
                entity.HasIndex(p => p.PhoneNumber);
                entity.HasIndex(p => p.UserCode).IsUnique();
                entity.HasIndex(p => p.Email);
            });

            builder.Entity<UserRecoveryCode>(entity =>
            {
                entity.ToTable("user_recovery_codes");
                entity.HasKey(p => p.Id);
                entity.Property(p => p.CodeHash).HasMaxLength(128).IsRequired();
                entity.Property(p => p.CodeSalt).HasMaxLength(64).IsRequired();
                entity.HasOne(p => p.User).WithMany().HasForeignKey(p => p.UserId).OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<TotpRateLimit>(entity =>
            {
                entity.ToTable("totp_rate_limits");
            });

            builder.Entity<UserSecurityKey>(entity =>
            {
                entity.ToTable("user_security_keys");
                entity.HasOne(p => p.User).WithMany().HasForeignKey(p => p.UserId).OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(p => new { p.UserId, p.CredentialId }).IsUnique();
            });

            builder.Entity<UserYubikey>(entity =>
            {
                entity.ToTable("user_yubikeys");
                entity.HasIndex(p => p.PublicId).IsUnique();
                entity.HasOne(p => p.User).WithMany().HasForeignKey(p => p.UserId).OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<CatalogLookup>(entity =>
            {
                entity.ToTable("catalog_lookups");
                entity.HasIndex(p => new { p.Category, p.Code }).IsUnique();
            });

            builder.Entity<MarketCertificateRule>(entity =>
            {
                entity.ToTable("market_certificate_rules");
                entity.HasIndex(p => p.MarketCode).IsUnique();
            });

            builder.Entity<ProductGroup>(entity =>
            {
                entity.ToTable("product_groups");
                entity.HasIndex(p => p.Code).IsUnique();
            });

            builder.Entity<ProductSku>(entity =>
            {
                entity.ToTable("product_skus");
                entity.HasIndex(p => p.Code).IsUnique();
                entity.HasOne(p => p.Group).WithMany(g => g.Skus).HasForeignKey(p => p.GroupId);
            });

            builder.Entity<PaymentTerm>(entity =>
            {
                entity.ToTable("payment_terms");
                entity.HasIndex(p => p.Code).IsUnique();
            });

            builder.Entity<Customer>(entity =>
            {
                entity.ToTable("customers");
                entity.HasIndex(p => p.Code).IsUnique();
            });

            builder.Entity<BusinessPartner>(entity =>
            {
                entity.ToTable("business_partners");
                entity.HasIndex(p => p.Code).IsUnique();
                entity.HasMany(p => p.Roles).WithOne(r => r.BusinessPartner).HasForeignKey(r => r.BusinessPartnerId).OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<BusinessPartnerRoleLink>(entity =>
            {
                entity.ToTable("business_partner_roles");
                entity.HasIndex(p => new { p.BusinessPartnerId, p.Role }).IsUnique();
            });

            builder.Entity<Vessel>(entity =>
            {
                entity.ToTable("vessels");
                entity.HasIndex(p => p.Code).IsUnique();
                entity.HasOne(p => p.OwnerPartner).WithMany().HasForeignKey(p => p.OwnerPartnerId).OnDelete(DeleteBehavior.SetNull);
            });

            builder.Entity<DocumentType>(entity =>
            {
                entity.ToTable("document_types");
                entity.HasIndex(p => p.Code).IsUnique();
            });

            builder.Entity<DocumentRequirement>(entity =>
            {
                entity.ToTable("document_requirements");
                entity.HasIndex(p => new { p.MarketCode, p.DocumentTypeId }).IsUnique();
                entity.HasOne(p => p.DocumentType).WithMany().HasForeignKey(p => p.DocumentTypeId).OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<DocumentAttachment>(entity =>
            {
                entity.ToTable("document_attachments");
                entity.HasIndex(p => new { p.OwnerType, p.OwnerId });
                entity.HasOne(p => p.DocumentType).WithMany().HasForeignKey(p => p.DocumentTypeId).OnDelete(DeleteBehavior.SetNull);
            });

            builder.Entity<InboundPurchase>(entity =>
            {
                entity.ToTable("inbound_purchases");
                entity.HasMany(p => p.Lines).WithOne(l => l.Purchase).HasForeignKey(l => l.PurchaseId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(p => p.SupplierPartner).WithMany().HasForeignKey(p => p.SupplierPartnerId).OnDelete(DeleteBehavior.SetNull);
                entity.HasOne(p => p.Vessel).WithMany().HasForeignKey(p => p.VesselId).OnDelete(DeleteBehavior.SetNull);
                entity.Ignore(p => p.Documents);
            });

            builder.Entity<InboundLine>(entity =>
            {
                entity.ToTable("inbound_lines");
            });

            builder.Entity<RawMaterialLot>(entity =>
            {
                entity.ToTable("raw_material_lots");
                entity.HasIndex(p => p.LotNumber).IsUnique();
                entity.HasIndex(p => p.InboundLineId).IsUnique();
                entity.HasOne(p => p.InboundPurchase).WithMany().HasForeignKey(p => p.InboundPurchaseId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(p => p.InboundLine).WithOne(l => l.RawMaterialLot).HasForeignKey<RawMaterialLot>(p => p.InboundLineId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(p => p.SupplierPartner).WithMany().HasForeignKey(p => p.SupplierPartnerId).OnDelete(DeleteBehavior.SetNull);
                entity.HasOne(p => p.Vessel).WithMany().HasForeignKey(p => p.VesselId).OnDelete(DeleteBehavior.SetNull);
                entity.Ignore(p => p.Documents);
            });

            builder.Entity<ProductionLot>(entity =>
            {
                entity.ToTable("production_lots");
                entity.HasIndex(p => p.LotNumber).IsUnique();
                entity.HasMany(p => p.Outputs).WithOne(o => o.Lot).HasForeignKey(o => o.LotId).OnDelete(DeleteBehavior.Cascade);
                entity.HasMany(p => p.Certificates).WithOne(c => c.Lot).HasForeignKey(c => c.LotId).OnDelete(DeleteBehavior.Cascade);
                entity.HasMany(p => p.Inputs).WithOne(i => i.ProductionLot).HasForeignKey(i => i.ProductionLotId).OnDelete(DeleteBehavior.Cascade);
                entity.Ignore(p => p.Documents);
            });

            builder.Entity<ProductionInput>(entity =>
            {
                entity.ToTable("production_inputs");
                entity.HasIndex(p => new { p.ProductionLotId, p.RawMaterialLotId }).IsUnique();
                entity.HasOne(p => p.RawMaterialLot).WithMany(l => l.ProductionInputs).HasForeignKey(p => p.RawMaterialLotId).OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<ProductionOutput>(entity =>
            {
                entity.ToTable("production_outputs");
            });

            builder.Entity<LotCertificate>(entity =>
            {
                entity.ToTable("lot_certificates");
            });

            builder.Entity<InventoryBalance>(entity =>
            {
                entity.ToTable("inventory_balances");
                entity.HasIndex(p => new { p.LotId, p.SkuId }).IsUnique();
                entity.HasMany(p => p.Movements).WithOne(m => m.InventoryBalance).HasForeignKey(m => m.InventoryBalanceId).OnDelete(DeleteBehavior.Cascade);
                entity.HasMany(p => p.Reservations).WithOne(r => r.InventoryBalance).HasForeignKey(r => r.InventoryBalanceId).OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<InventoryMovement>(entity =>
            {
                entity.ToTable("inventory_movements");
                entity.HasIndex(p => new { p.ReferenceType, p.ReferenceId });
            });

            builder.Entity<StockReservation>(entity =>
            {
                entity.ToTable("stock_reservations");
                entity.HasIndex(p => new { p.SalesContractLineId, p.InventoryBalanceId, p.Status });
                entity.HasOne(p => p.SalesContractLine).WithMany().HasForeignKey(p => p.SalesContractLineId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(p => p.Shipment).WithMany(s => s.Reservations).HasForeignKey(p => p.ShipmentId).OnDelete(DeleteBehavior.SetNull);
            });

            builder.Entity<SalesAllocation>(entity =>
            {
                entity.ToTable("sales_allocations");
                entity.HasIndex(p => new { p.SalesContractLineId, p.InventoryBalanceId, p.ShipmentId }).IsUnique();
                entity.HasOne(p => p.SalesContractLine).WithMany().HasForeignKey(p => p.SalesContractLineId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(p => p.Shipment).WithMany(s => s.Allocations).HasForeignKey(p => p.ShipmentId).OnDelete(DeleteBehavior.SetNull);
            });

            builder.Entity<TraceabilityLink>(entity =>
            {
                entity.ToTable("traceability_links");
                entity.HasIndex(p => new { p.FromType, p.FromId });
                entity.HasIndex(p => new { p.ToType, p.ToId });
            });

            builder.Entity<SalesContract>(entity =>
            {
                entity.ToTable("sales_contracts");
                entity.HasIndex(p => p.ContractNo).IsUnique();
                entity.HasMany(p => p.Lines).WithOne(l => l.Contract).HasForeignKey(l => l.ContractId).OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<SalesContractLine>(entity =>
            {
                entity.ToTable("sales_contract_lines");
            });

            builder.Entity<ExportShipment>(entity =>
            {
                entity.ToTable("export_shipments");
                entity.HasMany(p => p.Containers).WithOne(c => c.Shipment).HasForeignKey(c => c.ShipmentId).OnDelete(DeleteBehavior.Cascade);
                entity.HasMany(p => p.Documents).WithOne(d => d.Shipment).HasForeignKey(d => d.ShipmentId).OnDelete(DeleteBehavior.Cascade);
                entity.HasMany(p => p.Invoices).WithOne(i => i.Shipment).HasForeignKey(i => i.ShipmentId).OnDelete(DeleteBehavior.SetNull);
            });

            builder.Entity<ShipmentContainer>(entity =>
            {
                entity.ToTable("shipment_containers");
                entity.HasIndex(p => p.ContainerNo).IsUnique();
            });

            builder.Entity<ShipmentDocument>(entity =>
            {
                entity.ToTable("shipment_documents");
                entity.HasIndex(p => new { p.ShipmentId, p.DocumentAttachmentId }).IsUnique();
                entity.HasOne(p => p.DocumentAttachment).WithMany().HasForeignKey(p => p.DocumentAttachmentId).OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<SalesInvoice>(entity =>
            {
                entity.ToTable("sales_invoices");
                entity.HasIndex(p => p.InvoiceNo).IsUnique();
                entity.HasOne(p => p.SalesContract).WithMany(c => c.Invoices).HasForeignKey(p => p.SalesContractId).OnDelete(DeleteBehavior.SetNull);
                entity.HasOne(p => p.Customer).WithMany().HasForeignKey(p => p.CustomerId).OnDelete(DeleteBehavior.SetNull);
            });

            builder.Entity<PaymentInstallment>(entity =>
            {
                entity.ToTable("payment_installments");
                entity.HasOne(p => p.SalesInvoice).WithMany().HasForeignKey(p => p.SalesInvoiceId).OnDelete(DeleteBehavior.SetNull);
            });

            builder.Entity<PaymentTransaction>(entity =>
            {
                entity.ToTable("payment_transactions");
                entity.HasOne(p => p.Customer).WithMany().HasForeignKey(p => p.CustomerId).OnDelete(DeleteBehavior.SetNull);
                entity.HasOne(p => p.Invoice).WithMany().HasForeignKey(p => p.InvoiceId).OnDelete(DeleteBehavior.SetNull);
                entity.HasMany(p => p.Allocations).WithOne(a => a.PaymentTransaction).HasForeignKey(a => a.PaymentTransactionId).OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<PaymentAllocation>(entity =>
            {
                entity.ToTable("payment_allocations");
                entity.HasOne(p => p.PaymentInstallment).WithMany().HasForeignKey(p => p.PaymentInstallmentId).OnDelete(DeleteBehavior.SetNull);
                entity.HasOne(p => p.Invoice).WithMany().HasForeignKey(p => p.InvoiceId).OnDelete(DeleteBehavior.SetNull);
            });

            builder.Entity<CustomerDeposit>(entity =>
            {
                entity.ToTable("customer_deposits");
                entity.HasMany(p => p.Allocations).WithOne(a => a.Deposit).HasForeignKey(a => a.DepositId).OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<DepositAllocation>(entity =>
            {
                entity.ToTable("deposit_allocations");
            });

            builder.Entity<ExchangeRateSnapshot>(entity =>
            {
                entity.ToTable("exchange_rate_snapshots");
                entity.HasIndex(p => new { p.FromCurrency, p.ToCurrency, p.EffectiveDate }).IsUnique();
            });

            builder.Entity<DomainAuditLog>(entity =>
            {
                entity.ToTable("domain_audit_logs");
                entity.HasIndex(p => new { p.EntityType, p.EntityId, p.CreatedAt });
            });

            builder.Entity<ImportBatch>(entity =>
            {
                entity.ToTable("import_batches");
                entity.HasMany(p => p.Rows).WithOne(r => r.ImportBatch).HasForeignKey(r => r.ImportBatchId).OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<ImportStagingRow>(entity =>
            {
                entity.ToTable("import_staging_rows");
                entity.HasIndex(p => new { p.ImportBatchId, p.SheetName, p.RowNumber }).IsUnique();
            });
        }

        public override int SaveChanges()
        {
            SetNormalizedValues();
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SetNormalizedValues();
            return await base.SaveChangesAsync(cancellationToken);
        }

        private void SetNormalizedValues()
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified)
                .ToList();

            foreach (var entry in entries)
            {
                if (entry.Entity is User user)
                {
                    if (entry.State == EntityState.Added
                        || entry.HasChangedProperty(nameof(User.LastName))
                        || entry.HasChangedProperty(nameof(User.FirstName)))
                    {
                        user.UserNameNormalized = NormalizeHelper.NormalizeUserName(user.LastName, user.FirstName);
                    }
                }
            }
        }
    }

    public static class EntityEntryExtensions
    {
        public static bool HasChangedProperty(this Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry, string propertyName)
        {
            return entry.Property(propertyName).IsModified;
        }
    }
}
