using System.Security.Claims;
using Core.Const;
using Domain.Identity.Roles;
using Domain.Identity.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SqlServ4r.Repository.RoleClaims;
using Volo.Abp.DependencyInjection;

namespace Application.Identity.RoleManager
{
    public class PermissionSeeder : ITransientDependency
    {
        private readonly RoleManager<Role> _roleManager;
        private readonly UserManager<User> _userManager;
        private readonly RoleClaimRepository _roleClaimRepository;
        private readonly ILogger<PermissionSeeder> _logger;
        private readonly IConfiguration _configuration;

        public PermissionSeeder(
            RoleManager<Role> roleManager,
            UserManager<User> userManager,
            RoleClaimRepository roleClaimRepository,
            ILogger<PermissionSeeder> logger,
            IConfiguration configuration)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _roleClaimRepository = roleClaimRepository;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task SeedSuperAdminAsync()
        {
            foreach (var roleName in new[]
                     {
                         RoleNames.SuperAdmin, RoleNames.Admin, RoleNames.Purchasing,
                         RoleNames.Production, RoleNames.Warehouse, RoleNames.Sales,
                         RoleNames.Accounting, RoleNames.Viewer
                     })
            {
                if (await _roleManager.FindByNameAsync(roleName) != null) continue;
                var created = await _roleManager.CreateAsync(new Role
                {
                    Name = roleName,
                    Code = roleName,
                    DefaultPageUrl = roleName is RoleNames.Viewer or RoleNames.Accounting ? "dashboard" : "dashboard"
                });
                if (created.Succeeded)
                    _logger.LogInformation("Created role {Role}", roleName);
            }

            var userName = _configuration["Bootstrap:AdminUser"] ?? "admin";
            var password = _configuration["Bootstrap:AdminPassword"] ?? "Admin@123";
            var user = await _userManager.FindByNameAsync(userName);
            if (user == null)
            {
                user = new User
                {
                    UserName = userName,
                    Email = "admin@seafood.local",
                    UserCode = "ADMIN001",
                    FirstName = "Admin",
                    LastName = "Seafood",
                    PhoneNumber = "0000000000",
                    IsActive = true,
                    PasswordLoginAllowed = true,
                    EmailConfirmed = true,
                };
                var createUser = await _userManager.CreateAsync(user, password);
                if (!createUser.Succeeded)
                {
                    _logger.LogWarning("Failed to create bootstrap admin: {Error}",
                        createUser.Errors.FirstOrDefault()?.Description);
                    return;
                }
                _logger.LogInformation("Created bootstrap admin user '{User}'", userName);
            }

            if (!await _userManager.IsInRoleAsync(user, RoleNames.SuperAdmin))
            {
                await _userManager.AddToRoleAsync(user, RoleNames.SuperAdmin);
            }
            if (!await _userManager.IsInRoleAsync(user, RoleNames.Admin))
            {
                await _userManager.AddToRoleAsync(user, RoleNames.Admin);
            }
        }

        public async Task SeedIfEmptyAsync()
        {
            var hasAny = await _roleClaimRepository.GetQueryable()
                .AnyAsync(x => x.ClaimType == PermissionClaimType.Permission);
            if (hasAny) return;

            var roles = await _roleManager.Roles.ToListAsync();
            var seeded = 0;
            foreach (var role in roles)
            {
                foreach (var code in GetPermissionsForRole(role.Name ?? ""))
                {
                    var result = await _roleManager.AddClaimAsync(role,
                        new Claim(PermissionClaimType.Permission, code));
                    if (result.Succeeded) seeded++;
                }
            }
            _logger.LogInformation("Seeded {Count} permission claims", seeded);
        }

        private static IEnumerable<string> GetPermissionsForRole(string role)
        {
            var all = Permissions.All.Select(p => p.Code).ToList();
            var name = role.ToUpperInvariant();
            if (name is RoleNames.Admin or RoleNames.SuperAdmin) return all;

            var codes = new HashSet<string> { Permissions.Dashboard };
            if (name == RoleNames.Purchasing)
            {
                codes.UnionWith(new[] { Permissions.Inbound, Permissions.MasterCatalog, Permissions.MasterCustomers, Permissions.Inventory });
            }
            else if (name == RoleNames.Production)
            {
                codes.UnionWith(new[] { Permissions.Production, Permissions.Inventory, Permissions.MasterProducts });
            }
            else if (name == RoleNames.Warehouse)
            {
                codes.UnionWith(new[] { Permissions.Inventory, Permissions.Inbound, Permissions.Production });
            }
            else if (name == RoleNames.Sales)
            {
                codes.UnionWith(new[]
                {
                    Permissions.ExportQuotes, Permissions.ExportOrders, Permissions.MasterCustomers,
                    Permissions.MasterProducts, Permissions.Inventory
                });
            }
            else if (name == RoleNames.Accounting)
            {
                codes.UnionWith(new[] { Permissions.FinancePayments, Permissions.FinanceDeposits, Permissions.ExportOrders });
            }
            return codes;
        }
    }
}
