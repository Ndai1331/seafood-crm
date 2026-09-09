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
        public DbSet<InboundPurchase> InboundPurchases { get; set; } = null!;
        public DbSet<InboundLine> InboundLines { get; set; } = null!;
        public DbSet<ProductionLot> ProductionLots { get; set; } = null!;
        public DbSet<ProductionOutput> ProductionOutputs { get; set; } = null!;
        public DbSet<LotCertificate> LotCertificates { get; set; } = null!;
        public DbSet<InventoryBalance> InventoryBalances { get; set; } = null!;
        public DbSet<SalesContract> SalesContracts { get; set; } = null!;
        public DbSet<SalesContractLine> SalesContractLines { get; set; } = null!;
        public DbSet<ExportShipment> ExportShipments { get; set; } = null!;
        public DbSet<PaymentInstallment> PaymentInstallments { get; set; } = null!;
        public DbSet<CustomerDeposit> CustomerDeposits { get; set; } = null!;
        public DbSet<DepositAllocation> DepositAllocations { get; set; } = null!;

        public DbSet<LotAllocation> LotAllocations { get; set; } = null!;
        public DbSet<ReportFavorite> ReportFavorites { get; set; } = null!;

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

            builder.Entity<InboundPurchase>(entity =>
            {
                entity.ToTable("inbound_purchases");
                entity.HasMany(p => p.Lines).WithOne(l => l.Purchase).HasForeignKey(l => l.PurchaseId).OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<InboundLine>(entity =>
            {
                entity.ToTable("inbound_lines");
            });

            builder.Entity<ProductionLot>(entity =>
            {
                entity.ToTable("production_lots");
                entity.HasIndex(p => p.LotNumber).IsUnique();
                entity.HasMany(p => p.Outputs).WithOne(o => o.Lot).HasForeignKey(o => o.LotId).OnDelete(DeleteBehavior.Cascade);
                entity.HasMany(p => p.Certificates).WithOne(c => c.Lot).HasForeignKey(c => c.LotId).OnDelete(DeleteBehavior.Cascade);
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
            });

            builder.Entity<PaymentInstallment>(entity =>
            {
                entity.ToTable("payment_installments");
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

            builder.Entity<LotAllocation>(entity =>
            {
                entity.ToTable("lot_allocations");
            });

            builder.Entity<ReportFavorite>(entity =>
            {
                entity.ToTable("report_favorites");
                entity.HasIndex(p => new { p.UserId, p.ReportCode }).IsUnique();
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
