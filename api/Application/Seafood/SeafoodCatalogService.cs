using Contract.Seafood;
using Core.Exceptions;
using Domain.Seafood;
using Microsoft.EntityFrameworkCore;
using SqlServ4r.EntityFramework;
using System.Net;
using Newtonsoft.Json;
using Volo.Abp.DependencyInjection;

namespace Application.Seafood
{
    public class SeafoodCatalogService : ITransientDependency
    {
        private readonly DreamContext _db;
        public SeafoodCatalogService(DreamContext db) => _db = db;

        public Task<List<CatalogLookupDto>> GetLookupsAsync(LookupCategory? category) =>
            _db.CatalogLookups
                .Where(x => category == null || x.Category == category)
                .OrderBy(x => x.Category).ThenBy(x => x.SortOrder)
                .Select(x => new CatalogLookupDto
                {
                    Id = x.Id, Category = x.Category, Code = x.Code, Name = x.Name,
                    Description = x.Description, SortOrder = x.SortOrder, IsActive = x.IsActive
                }).ToListAsync();

        public async Task<SeafoodPagedResult<CatalogLookupDto>> GetLookupsPageAsync(LookupCategory? category, string? search, int skip, int take)
        {
            var query = _db.CatalogLookups.AsNoTracking().Where(x => category == null || x.Category == category.Value);
            var term = search?.Trim();
            if (!string.IsNullOrWhiteSpace(term))
                query = query.Where(x => x.Code.Contains(term) || x.Name.Contains(term) || (x.Description ?? "").Contains(term));
            var total = await query.CountAsync();
            var items = await query.OrderBy(x => x.SortOrder).ThenBy(x => x.Code)
                .Skip(Math.Max(0, skip)).Take(Math.Clamp(take, 1, 100))
                .Select(x => new CatalogLookupDto
                {
                    Id = x.Id, Category = x.Category, Code = x.Code, Name = x.Name,
                    Description = x.Description, SortOrder = x.SortOrder, IsActive = x.IsActive
                }).ToListAsync();
            return new SeafoodPagedResult<CatalogLookupDto> { Items = items, TotalCount = total };
        }

        public async Task<SeafoodSelect2SearchResponseDto> SearchLookupsAsync(LookupCategory category, string? search, int page = 1, int pageSize = 20)
        {
            var term = search?.Trim();
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 100);
            var query = _db.CatalogLookups.AsNoTracking().Where(x => x.Category == category && x.IsActive);
            if (!string.IsNullOrWhiteSpace(term))
                query = query.Where(x => x.Code.Contains(term) || x.Name.Contains(term) || (x.Description ?? "").Contains(term));

            var total = await query.CountAsync();
            var results = await query.OrderBy(x => x.SortOrder).ThenBy(x => x.Code)
                .Skip((page - 1) * pageSize).Take(pageSize)
                .Select(x => new SeafoodSelectOptionDto
                {
                    Id = x.Id,
                    Text = x.Code + " — " + x.Name,
                    Description = x.Description
                }).ToListAsync();
            return new SeafoodSelect2SearchResponseDto { Results = results, More = page * pageSize < total };
        }

        public async Task<CatalogLookupDto> SaveLookupAsync(CatalogLookupDto dto)
        {
            if (!Enum.IsDefined(dto.Category)
                || string.IsNullOrWhiteSpace(dto.Code)
                || string.IsNullOrWhiteSpace(dto.Name))
            {
                throw new GlobalException("Loại, mã và tên danh mục là bắt buộc.", HttpStatusCode.BadRequest);
            }

            var code = dto.Code.Trim();
            if (await _db.CatalogLookups.AnyAsync(x => x.Id != dto.Id
                && x.Category == dto.Category
                && x.Code.ToUpper() == code.ToUpper()))
            {
                throw new GlobalException("Mã danh mục đã tồn tại trong nhóm này.", HttpStatusCode.Conflict);
            }

            CatalogLookup entity;
            if (dto.Id == 0)
            {
                entity = new CatalogLookup();
                _db.CatalogLookups.Add(entity);
            }
            else
            {
                entity = await _db.CatalogLookups.FindAsync(dto.Id) ?? throw new InvalidOperationException("Not found");
            }
            entity.Category = dto.Category;
            entity.Code = code;
            entity.Name = dto.Name.Trim();
            entity.Description = dto.Description?.Trim();
            entity.SortOrder = dto.SortOrder;
            entity.IsActive = dto.IsActive;
            await _db.SaveChangesAsync();
            dto.Id = entity.Id;
            return dto;
        }

        public async Task<bool> DeleteLookupAsync(int id)
        {
            var entity = await _db.CatalogLookups.FindAsync(id);
            if (entity == null)
            {
                return false;
            }

            entity.IsActive = false;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<List<MarketCertificateRuleDto>> GetMarketCertificateRulesAsync()
        {
            var rules = await _db.MarketCertificateRules.AsNoTracking().OrderBy(x => x.MarketCode).ToListAsync();
            var types = await _db.DocumentTypes.AsNoTracking().ToDictionaryAsync(x => x.Code, StringComparer.OrdinalIgnoreCase);
            return rules.Select(rule =>
            {
                types.TryGetValue(rule.MustHaveCertificateCode ?? string.Empty, out var required);
                types.TryGetValue(rule.AddonCertificateCode ?? string.Empty, out var supplemental);
                return new MarketCertificateRuleDto
                {
                    Id = rule.Id,
                    MarketCode = rule.MarketCode,
                    RequiredDocumentTypeId = required?.Id,
                    RequiredDocumentTypeCode = rule.MustHaveCertificateCode,
                    RequiredDocumentTypeName = required?.Name,
                    SupplementalDocumentTypeId = supplemental?.Id,
                    SupplementalDocumentTypeCode = rule.AddonCertificateCode,
                    SupplementalDocumentTypeName = supplemental?.Name,
                    Note = rule.Note,
                    IsActive = rule.IsActive
                };
            }).ToList();
        }

        public async Task<SeafoodPagedResult<MarketCertificateRuleDto>> GetMarketCertificateRulesPageAsync(string? search, int skip, int take)
        {
            var query = _db.MarketCertificateRules.AsNoTracking();
            var term = search?.Trim();
            if (!string.IsNullOrWhiteSpace(term))
                query = query.Where(x => x.MarketCode.Contains(term) || (x.MustHaveCertificateCode ?? "").Contains(term)
                    || (x.AddonCertificateCode ?? "").Contains(term) || (x.Note ?? "").Contains(term));
            var total = await query.CountAsync();
            var rules = await query.OrderBy(x => x.MarketCode).Skip(Math.Max(0, skip)).Take(Math.Clamp(take, 1, 100)).ToListAsync();
            var codes = rules.SelectMany(x => new[] { x.MustHaveCertificateCode, x.AddonCertificateCode })
                .Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x!).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            var types = await _db.DocumentTypes.AsNoTracking().Where(x => codes.Contains(x.Code)).ToDictionaryAsync(x => x.Code, StringComparer.OrdinalIgnoreCase);
            var items = rules.Select(rule =>
            {
                types.TryGetValue(rule.MustHaveCertificateCode ?? string.Empty, out var required);
                types.TryGetValue(rule.AddonCertificateCode ?? string.Empty, out var supplemental);
                return new MarketCertificateRuleDto
                {
                    Id = rule.Id, MarketCode = rule.MarketCode,
                    RequiredDocumentTypeId = required?.Id, RequiredDocumentTypeCode = rule.MustHaveCertificateCode, RequiredDocumentTypeName = required?.Name,
                    SupplementalDocumentTypeId = supplemental?.Id, SupplementalDocumentTypeCode = rule.AddonCertificateCode, SupplementalDocumentTypeName = supplemental?.Name,
                    Note = rule.Note, IsActive = rule.IsActive
                };
            }).ToList();
            return new SeafoodPagedResult<MarketCertificateRuleDto> { Items = items, TotalCount = total };
        }

        public async Task<MarketCertificateRuleDto> SaveMarketCertificateRuleAsync(MarketCertificateRuleDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.MarketCode))
                throw new GlobalException("Thị trường là bắt buộc.", HttpStatusCode.BadRequest);
            var marketCode = dto.MarketCode.Trim().ToUpperInvariant();
            if (await _db.MarketCertificateRules.AnyAsync(x => x.Id != dto.Id && x.MarketCode.ToUpper() == marketCode))
                throw new GlobalException("Thị trường đã có quy tắc chứng từ.", HttpStatusCode.Conflict);

            var requiredCode = await ResolveDocumentCodeAsync(dto.RequiredDocumentTypeId, dto.RequiredDocumentTypeCode);
            var supplementalCode = await ResolveDocumentCodeAsync(dto.SupplementalDocumentTypeId, dto.SupplementalDocumentTypeCode);
            var entity = dto.Id == 0
                ? new MarketCertificateRule()
                : await _db.MarketCertificateRules.FindAsync(dto.Id)
                    ?? throw new GlobalException("Không tìm thấy quy tắc chứng từ.", HttpStatusCode.NotFound);
            entity.MarketCode = marketCode;
            entity.MustHaveCertificateCode = requiredCode;
            entity.AddonCertificateCode = supplementalCode;
            entity.Note = dto.Note?.Trim();
            entity.IsActive = dto.IsActive;
            if (dto.Id == 0) _db.MarketCertificateRules.Add(entity);
            await SyncDocumentRequirementsAsync(entity);
            await _db.SaveChangesAsync();
            dto.Id = entity.Id;
            dto.MarketCode = entity.MarketCode;
            dto.RequiredDocumentTypeCode = entity.MustHaveCertificateCode;
            dto.SupplementalDocumentTypeCode = entity.AddonCertificateCode;
            return dto;
        }

        public async Task<bool> DeleteMarketCertificateRuleAsync(int id)
        {
            var entity = await _db.MarketCertificateRules.FindAsync(id);
            if (entity == null) return false;
            entity.IsActive = false;
            await SyncDocumentRequirementsAsync(entity);
            await _db.SaveChangesAsync();
            return true;
        }

        private async Task<string?> ResolveDocumentCodeAsync(int? documentTypeId, string? fallbackCode)
        {
            if (documentTypeId is int id && id > 0)
            {
                var type = await _db.DocumentTypes.FirstOrDefaultAsync(x => x.Id == id && x.IsActive)
                    ?? throw new GlobalException("Loại chứng từ không tồn tại hoặc đã bị khóa.", HttpStatusCode.BadRequest);
                return type.Code;
            }
            if (string.IsNullOrWhiteSpace(fallbackCode)) return null;
            var typeByCode = await _db.DocumentTypes.FirstOrDefaultAsync(x => x.IsActive && x.Code.ToUpper() == fallbackCode.Trim().ToUpper());
            return typeByCode?.Code ?? throw new GlobalException("Loại chứng từ không tồn tại hoặc đã bị khóa.", HttpStatusCode.BadRequest);
        }

        private async Task SyncDocumentRequirementsAsync(MarketCertificateRule rule)
        {
            var current = await _db.DocumentRequirements
                .Where(x => x.MarketCode == rule.MarketCode)
                .ToListAsync();
            _db.DocumentRequirements.RemoveRange(current);
            if (!rule.IsActive) return;

            var codes = new[] { rule.MustHaveCertificateCode, rule.AddonCertificateCode }
                .Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x!).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            if (codes.Count == 0) return;
            var types = await _db.DocumentTypes.Where(x => codes.Contains(x.Code)).ToListAsync();
            var requiredType = types.FirstOrDefault(x => string.Equals(x.Code, rule.MustHaveCertificateCode, StringComparison.OrdinalIgnoreCase));
            var supplementalType = types.FirstOrDefault(x => string.Equals(x.Code, rule.AddonCertificateCode, StringComparison.OrdinalIgnoreCase));
            if (requiredType != null)
                _db.DocumentRequirements.Add(new DocumentRequirement { MarketCode = rule.MarketCode, DocumentTypeId = requiredType.Id, IsRequired = true, SortOrder = 1, Note = rule.Note });
            if (supplementalType != null && supplementalType.Id != requiredType?.Id)
                _db.DocumentRequirements.Add(new DocumentRequirement { MarketCode = rule.MarketCode, DocumentTypeId = supplementalType.Id, IsRequired = false, SortOrder = 2, Note = rule.Note });
        }

        public Task<List<ProductGroupDto>> GetProductsAsync() =>
            _db.ProductGroups.Include(g => g.Skus).OrderBy(g => g.SortOrder)
                .Select(g => new ProductGroupDto
                {
                    Id = g.Id, Code = g.Code, Name = g.Name, DefaultMarket = g.DefaultMarket,
                    SortOrder = g.SortOrder, IsActive = g.IsActive, SkuCount = g.Skus.Count,
                    Skus = g.Skus.Select(s => new ProductSkuDto
                    {
                        Id = s.Id, GroupId = g.Id, GroupName = g.Name, Code = s.Code, Name = s.Name,
                        ExportMarket = s.ExportMarket, DefaultUnitPriceUsd = s.DefaultUnitPriceUsd,
                        IsByproduct = s.IsByproduct, IsActive = s.IsActive
                    }).ToList()
                }).ToListAsync();

        public async Task<SeafoodPagedResult<ProductGroupDto>> GetProductsPageAsync(string? search, int skip, int take)
        {
            var query = _db.ProductGroups.AsNoTracking();
            var term = search?.Trim();
            if (!string.IsNullOrWhiteSpace(term))
                query = query.Where(g => g.Code.Contains(term) || g.Name.Contains(term) || (g.DefaultMarket ?? "").Contains(term)
                    || g.Skus.Any(s => s.Code.Contains(term) || s.Name.Contains(term) || (s.ExportMarket ?? "").Contains(term)));
            var total = await query.CountAsync();
            var groups = await query.OrderBy(g => g.SortOrder).ThenBy(g => g.Name)
                .Skip(Math.Max(0, skip)).Take(Math.Clamp(take, 1, 100)).ToListAsync();
            var items = groups.Select(g => new ProductGroupDto
            {
                Id = g.Id, Code = g.Code, Name = g.Name, DefaultMarket = g.DefaultMarket,
                SortOrder = g.SortOrder, IsActive = g.IsActive, SkuCount = g.Skus.Count
            }).ToList();
            return new SeafoodPagedResult<ProductGroupDto> { Items = items, TotalCount = total };
        }

        public async Task<SeafoodPagedResult<ProductSkuDto>> GetProductSkusPageAsync(int groupId, string? search, int skip, int take)
        {
            var query = _db.ProductSkus.AsNoTracking().Where(x => x.GroupId == groupId && x.Group != null && x.Group.IsActive);
            var term = search?.Trim();
            if (!string.IsNullOrWhiteSpace(term))
                query = query.Where(x => x.Code.Contains(term) || x.Name.Contains(term) || (x.ExportMarket ?? "").Contains(term));
            var total = await query.CountAsync();
            var items = await query.OrderBy(x => x.Code).Skip(Math.Max(0, skip)).Take(Math.Clamp(take, 1, 100))
                .Select(s => new ProductSkuDto
                {
                    Id = s.Id, GroupId = s.GroupId, GroupName = s.Group == null ? null : s.Group.Name,
                    Code = s.Code, Name = s.Name, ExportMarket = s.ExportMarket,
                    DefaultUnitPriceUsd = s.DefaultUnitPriceUsd, IsByproduct = s.IsByproduct, IsActive = s.IsActive
                }).ToListAsync();
            return new SeafoodPagedResult<ProductSkuDto> { Items = items, TotalCount = total };
        }

        public async Task<ProductGroupDto> SaveProductGroupAsync(ProductGroupDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Code) || string.IsNullOrWhiteSpace(dto.Name))
                throw new GlobalException("Mã và tên nhóm sản phẩm là bắt buộc.", HttpStatusCode.BadRequest);

            var code = dto.Code.Trim();
            if (await _db.ProductGroups.AnyAsync(x => x.Id != dto.Id && x.Code.ToUpper() == code.ToUpper()))
                throw new GlobalException("Mã nhóm sản phẩm đã tồn tại.", HttpStatusCode.Conflict);

            var entity = dto.Id == 0
                ? new ProductGroup()
                : await _db.ProductGroups.FindAsync(dto.Id)
                    ?? throw new GlobalException("Không tìm thấy nhóm sản phẩm.", HttpStatusCode.NotFound);
            entity.Code = code;
            entity.Name = dto.Name.Trim();
            entity.DefaultMarket = dto.DefaultMarket?.Trim();
            entity.SortOrder = dto.SortOrder;
            entity.IsActive = dto.IsActive;
            if (dto.Id == 0) _db.ProductGroups.Add(entity);
            await _db.SaveChangesAsync();
            return new ProductGroupDto
            {
                Id = entity.Id, Code = entity.Code, Name = entity.Name, DefaultMarket = entity.DefaultMarket,
                SortOrder = entity.SortOrder, IsActive = entity.IsActive
            };
        }

        public async Task<bool> DeleteProductGroupAsync(int id)
        {
            var entity = await _db.ProductGroups.Include(x => x.Skus).FirstOrDefaultAsync(x => x.Id == id);
            if (entity == null) return false;
            entity.IsActive = false;
            foreach (var sku in entity.Skus) sku.IsActive = false;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<SeafoodSelect2SearchResponseDto> SearchSkusAsync(string? search, int page = 1, int pageSize = 20)
        {
            var term = search?.Trim();
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 100);
            var query = _db.ProductSkus.AsNoTracking().Where(x => x.IsActive && x.Group != null && x.Group.IsActive);
            if (!string.IsNullOrWhiteSpace(term))
                query = query.Where(x => x.Code.Contains(term) || x.Name.Contains(term) || (x.Group != null && x.Group.Name.Contains(term)));

            var total = await query.CountAsync();
            var results = await query.OrderBy(x => x.Code)
                .Skip((page - 1) * pageSize).Take(pageSize)
                .Select(x => new SeafoodSelectOptionDto
                {
                    Id = x.Id,
                    Text = x.Code + " — " + x.Name,
                    Description = x.Group == null ? x.ExportMarket : x.Group.Name + (string.IsNullOrWhiteSpace(x.ExportMarket) ? "" : " · " + x.ExportMarket)
                }).ToListAsync();
            return new SeafoodSelect2SearchResponseDto { Results = results, More = page * pageSize < total };
        }

        public async Task<SeafoodSelect2SearchResponseDto> SearchProductGroupsAsync(string? search, int page = 1, int pageSize = 20)
        {
            var term = search?.Trim();
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 100);
            var query = _db.ProductGroups.AsNoTracking().Where(x => x.IsActive);
            if (!string.IsNullOrWhiteSpace(term))
                query = query.Where(x => x.Code.Contains(term) || x.Name.Contains(term) || (x.DefaultMarket ?? "").Contains(term));

            var total = await query.CountAsync();
            var results = await query.OrderBy(x => x.Code)
                .Skip((page - 1) * pageSize).Take(pageSize)
                .Select(x => new SeafoodSelectOptionDto
                {
                    Id = x.Id,
                    Text = x.Code + " — " + x.Name,
                    Description = x.DefaultMarket
                }).ToListAsync();
            return new SeafoodSelect2SearchResponseDto { Results = results, More = page * pageSize < total };
        }

        public async Task<ProductSkuDto> SaveSkuAsync(ProductSkuDto dto)
        {
            if (dto.GroupId <= 0 || string.IsNullOrWhiteSpace(dto.Code) || string.IsNullOrWhiteSpace(dto.Name))
                throw new GlobalException("Nhóm, mã và tên SKU là bắt buộc.", HttpStatusCode.BadRequest);
            if (!await _db.ProductGroups.AnyAsync(x => x.Id == dto.GroupId && x.IsActive))
                throw new GlobalException("Nhóm sản phẩm không tồn tại hoặc đã bị khóa.", HttpStatusCode.BadRequest);
            var code = dto.Code.Trim();
            if (await _db.ProductSkus.AnyAsync(x => x.Id != dto.Id && x.Code.ToUpper() == code.ToUpper()))
                throw new GlobalException("Mã SKU đã tồn tại.", HttpStatusCode.Conflict);

            ProductSku entity;
            if (dto.Id == 0)
            {
                entity = new ProductSku();
                _db.ProductSkus.Add(entity);
            }
            else entity = await _db.ProductSkus.FindAsync(dto.Id) ?? throw new InvalidOperationException("Not found");
            entity.GroupId = dto.GroupId;
            entity.Code = code;
            entity.Name = dto.Name.Trim();
            entity.ExportMarket = dto.ExportMarket?.Trim();
            entity.DefaultUnitPriceUsd = dto.DefaultUnitPriceUsd;
            entity.IsByproduct = dto.IsByproduct;
            entity.IsActive = dto.IsActive;
            await _db.SaveChangesAsync();
            dto.Id = entity.Id;
            return dto;
        }

        public async Task<bool> DeleteSkuAsync(int id)
        {
            var entity = await _db.ProductSkus.FindAsync(id);
            if (entity == null) return false;
            entity.IsActive = false;
            await _db.SaveChangesAsync();
            return true;
        }

        public Task<List<CustomerDto>> GetCustomersAsync() =>
            _db.Customers.OrderBy(c => c.Code).Select(c => new CustomerDto
            {
                Id = c.Id, Code = c.Code, Name = c.Name, ContactName = c.ContactName,
                Phone = c.Phone, Email = c.Email, Address = c.Address, CountryCode = c.CountryCode,
                TaxCode = c.TaxCode, BusinessRegistrationNo = c.BusinessRegistrationNo,
                Kind = c.Kind, Purchaser = c.Purchaser, Note = c.Note, IsActive = c.IsActive
            }).ToListAsync();

        public async Task<SeafoodPagedResult<CustomerDto>> GetCustomersPageAsync(string? search, int skip, int take)
        {
            var query = _db.Customers.AsNoTracking();
            var term = search?.Trim();
            if (!string.IsNullOrWhiteSpace(term))
                query = query.Where(x => x.Code.Contains(term) || x.Name.Contains(term) || (x.ContactName ?? "").Contains(term)
                    || (x.Phone ?? "").Contains(term) || (x.Email ?? "").Contains(term)
                    || (x.Address ?? "").Contains(term) || (x.CountryCode ?? "").Contains(term)
                    || (x.TaxCode ?? "").Contains(term) || (x.BusinessRegistrationNo ?? "").Contains(term)
                    || (x.Purchaser ?? "").Contains(term) || (x.Note ?? "").Contains(term));
            var total = await query.CountAsync();
            var items = await query.OrderBy(x => x.Code).Skip(Math.Max(0, skip)).Take(Math.Clamp(take, 1, 100))
                .Select(c => new CustomerDto
                {
                    Id = c.Id, Code = c.Code, Name = c.Name, ContactName = c.ContactName,
                    Phone = c.Phone, Email = c.Email, Address = c.Address, CountryCode = c.CountryCode,
                    TaxCode = c.TaxCode, BusinessRegistrationNo = c.BusinessRegistrationNo,
                    Kind = c.Kind, Purchaser = c.Purchaser, Note = c.Note, IsActive = c.IsActive
                }).ToListAsync();
            return new SeafoodPagedResult<CustomerDto> { Items = items, TotalCount = total };
        }

        public async Task<SeafoodSelect2SearchResponseDto> SearchCustomersAsync(string? search, int page = 1, int pageSize = 20)
        {
            var term = search?.Trim();
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 100);
            var query = _db.Customers.AsNoTracking().Where(x => x.IsActive);
            if (!string.IsNullOrWhiteSpace(term))
                query = query.Where(x => x.Code.Contains(term) || x.Name.Contains(term) || (x.ContactName ?? "").Contains(term)
                    || (x.Phone ?? "").Contains(term) || (x.Email ?? "").Contains(term)
                    || (x.TaxCode ?? "").Contains(term));

            var total = await query.CountAsync();
            var results = await query.OrderBy(x => x.Code)
                .Skip((page - 1) * pageSize).Take(pageSize)
                .Select(x => new SeafoodSelectOptionDto
                {
                    Id = x.Id,
                    Text = x.Code + " — " + x.Name,
                    Description = x.ContactName
                }).ToListAsync();
            return new SeafoodSelect2SearchResponseDto { Results = results, More = page * pageSize < total };
        }

        public async Task<CustomerDto> SaveCustomerAsync(CustomerDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Code) || string.IsNullOrWhiteSpace(dto.Name))
                throw new GlobalException("Mã và tên khách hàng là bắt buộc.", HttpStatusCode.BadRequest);
            if (!Enum.IsDefined(dto.Kind))
                throw new GlobalException("Loại khách hàng không hợp lệ.", HttpStatusCode.BadRequest);
            var code = dto.Code.Trim();
            if (await _db.Customers.AnyAsync(x => x.Id != dto.Id && x.Code.ToUpper() == code.ToUpper()))
                throw new GlobalException("Mã khách hàng đã tồn tại.", HttpStatusCode.Conflict);

            Customer entity;
            if (dto.Id == 0)
            {
                entity = new Customer();
                _db.Customers.Add(entity);
            }
            else entity = await _db.Customers.FindAsync(dto.Id) ?? throw new InvalidOperationException("Not found");
            entity.Code = code;
            entity.Name = dto.Name.Trim();
            entity.ContactName = dto.ContactName?.Trim();
            entity.Phone = dto.Phone?.Trim();
            entity.Email = dto.Email?.Trim();
            entity.Address = dto.Address?.Trim();
            entity.CountryCode = dto.CountryCode?.Trim().ToUpperInvariant();
            entity.TaxCode = dto.TaxCode?.Trim();
            entity.BusinessRegistrationNo = dto.BusinessRegistrationNo?.Trim();
            entity.Kind = dto.Kind;
            entity.Purchaser = dto.Purchaser;
            entity.Note = dto.Note;
            entity.IsActive = dto.IsActive;
            await _db.SaveChangesAsync();
            dto.Id = entity.Id;
            return dto;
        }

        public async Task<bool> DeleteCustomerAsync(int id)
        {
            var entity = await _db.Customers.FindAsync(id);
            if (entity == null) return false;
            entity.IsActive = false;
            await _db.SaveChangesAsync();
            return true;
        }

        public Task<List<PaymentTermDto>> GetPaymentTermsAsync() =>
            _db.PaymentTerms.OrderBy(x => x.Name).Select(x => new PaymentTermDto
            {
                Id = x.Id, Code = x.Code, Name = x.Name, RatiosJson = x.RatiosJson,
                DueDays = x.DueDays, IsLc = x.IsLc, IsActive = x.IsActive
            }).ToListAsync();

        public async Task<SeafoodPagedResult<PaymentTermDto>> GetPaymentTermsPageAsync(string? search, int skip, int take)
        {
            var query = _db.PaymentTerms.AsNoTracking();
            var term = search?.Trim();
            if (!string.IsNullOrWhiteSpace(term))
                query = query.Where(x => x.Code.Contains(term) || x.Name.Contains(term) || x.RatiosJson.Contains(term));
            var total = await query.CountAsync();
            var items = await query.OrderBy(x => x.Code).Skip(Math.Max(0, skip)).Take(Math.Clamp(take, 1, 100))
                .Select(x => new PaymentTermDto
                {
                    Id = x.Id, Code = x.Code, Name = x.Name, RatiosJson = x.RatiosJson,
                    DueDays = x.DueDays, IsLc = x.IsLc, IsActive = x.IsActive
                }).ToListAsync();
            return new SeafoodPagedResult<PaymentTermDto> { Items = items, TotalCount = total };
        }

        public async Task<SeafoodSelect2SearchResponseDto> SearchPaymentTermsAsync(string? search, int page = 1, int pageSize = 20)
        {
            var term = search?.Trim();
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 100);
            var query = _db.PaymentTerms.AsNoTracking().Where(x => x.IsActive);
            if (!string.IsNullOrWhiteSpace(term))
                query = query.Where(x => x.Code.Contains(term) || x.Name.Contains(term));

            var total = await query.CountAsync();
            var results = await query.OrderBy(x => x.Code)
                .Skip((page - 1) * pageSize).Take(pageSize)
                .Select(x => new SeafoodSelectOptionDto
                {
                    Id = x.Id,
                    Text = x.Code + " — " + x.Name,
                    Description = x.IsLc ? "LC" : "TT"
                }).ToListAsync();
            return new SeafoodSelect2SearchResponseDto { Results = results, More = page * pageSize < total };
        }

        public async Task<SeafoodSelect2SearchResponseDto> SearchContractsAsync(string? search, int page = 1, int pageSize = 20)
        {
            var term = search?.Trim();
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 100);
            var query = _db.SalesContracts.AsNoTracking().Include(x => x.Customer).Where(x => x.Status != ContractStatus.Cancelled);
            if (!string.IsNullOrWhiteSpace(term))
                query = query.Where(x => x.ContractNo.Contains(term) || (x.Customer != null && (x.Customer.Code.Contains(term) || x.Customer.Name.Contains(term))));

            var total = await query.CountAsync();
            var results = await query.OrderByDescending(x => x.CreatedAt)
                .Skip((page - 1) * pageSize).Take(pageSize)
                .Select(x => new SeafoodSelectOptionDto
                {
                    Id = x.Id,
                    Text = x.ContractNo + " — " + (x.Customer == null ? "" : x.Customer.Name),
                    Description = x.Customer == null ? null : x.Customer.Code
                }).ToListAsync();
            return new SeafoodSelect2SearchResponseDto { Results = results, More = page * pageSize < total };
        }

        public async Task<PaymentTermDto> SavePaymentTermAsync(PaymentTermDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Code) || string.IsNullOrWhiteSpace(dto.Name))
                throw new GlobalException("Mã và tên điều khoản thanh toán là bắt buộc.", HttpStatusCode.BadRequest);
            List<decimal> ratios;
            try
            {
                ratios = JsonConvert.DeserializeObject<List<decimal>>(dto.RatiosJson ?? "[]") ?? new List<decimal>();
            }
            catch (JsonException)
            {
                throw new GlobalException("Tỷ lệ thanh toán phải là JSON array hợp lệ, ví dụ [0.3,0.7].", HttpStatusCode.BadRequest);
            }
            if (ratios.Count == 0 || ratios.Any(x => x <= 0) || Math.Abs(ratios.Sum() - 1m) > 0.0001m)
                throw new GlobalException("Tổng tỷ lệ thanh toán phải bằng 100% và từng tỷ lệ phải lớn hơn 0.", HttpStatusCode.BadRequest);
            var code = dto.Code.Trim();
            if (await _db.PaymentTerms.AnyAsync(x => x.Id != dto.Id && x.Code.ToUpper() == code.ToUpper()))
                throw new GlobalException("Mã điều khoản thanh toán đã tồn tại.", HttpStatusCode.Conflict);
            PaymentTerm entity;
            if (dto.Id == 0)
            {
                entity = new PaymentTerm();
                _db.PaymentTerms.Add(entity);
            }
            else entity = await _db.PaymentTerms.FindAsync(dto.Id) ?? throw new InvalidOperationException("Not found");
            entity.Code = code;
            entity.Name = dto.Name.Trim();
            entity.RatiosJson = dto.RatiosJson;
            entity.DueDays = dto.DueDays;
            entity.IsLc = dto.IsLc;
            entity.IsActive = dto.IsActive;
            await _db.SaveChangesAsync();
            dto.Id = entity.Id;
            return dto;
        }

        public async Task<bool> DeletePaymentTermAsync(int id)
        {
            var entity = await _db.PaymentTerms.FindAsync(id);
            if (entity == null) return false;
            entity.IsActive = false;
            await _db.SaveChangesAsync();
            return true;
        }
    }
}
