using Contract.Seafood;
using Domain.Seafood;
using Microsoft.EntityFrameworkCore;
using SqlServ4r.EntityFramework;
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

        public async Task<CatalogLookupDto> SaveLookupAsync(CatalogLookupDto dto)
        {
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
            entity.Code = dto.Code;
            entity.Name = dto.Name;
            entity.Description = dto.Description;
            entity.SortOrder = dto.SortOrder;
            entity.IsActive = dto.IsActive;
            await _db.SaveChangesAsync();
            dto.Id = entity.Id;
            return dto;
        }

        public async Task DeleteLookupAsync(int id)
        {
            var entity = await _db.CatalogLookups.FindAsync(id) ?? throw new InvalidOperationException("Not found");
            entity.IsActive = false;
            await _db.SaveChangesAsync();
        }

        public Task<List<ProductGroupDto>> GetProductsAsync() =>
            _db.ProductGroups.Include(g => g.Skus).OrderBy(g => g.SortOrder)
                .Select(g => new ProductGroupDto
                {
                    Id = g.Id, Code = g.Code, Name = g.Name, DefaultMarket = g.DefaultMarket,
                    Skus = g.Skus.Select(s => new ProductSkuDto
                    {
                        Id = s.Id, GroupId = g.Id, GroupName = g.Name, Code = s.Code, Name = s.Name,
                        ExportMarket = s.ExportMarket, DefaultUnitPriceUsd = s.DefaultUnitPriceUsd,
                        IsByproduct = s.IsByproduct, IsActive = s.IsActive
                    }).ToList()
                }).ToListAsync();

        public async Task<ProductGroupDto> SaveGroupAsync(ProductGroupDto dto)
        {
            ProductGroup entity;
            if (dto.Id == 0)
            {
                entity = new ProductGroup();
                _db.ProductGroups.Add(entity);
            }
            else entity = await _db.ProductGroups.FindAsync(dto.Id) ?? throw new InvalidOperationException("Not found");
            entity.Code = dto.Code;
            entity.Name = dto.Name;
            entity.DefaultMarket = dto.DefaultMarket;
            await _db.SaveChangesAsync();
            dto.Id = entity.Id;
            return dto;
        }

        public async Task DeleteSkuAsync(int id)
        {
            var used = await _db.ProductionOutputs.AnyAsync(x => x.SkuId == id)
                       || await _db.SalesContractLines.AnyAsync(x => x.SkuId == id);
            if (used) throw new InvalidOperationException("SKU is in use.");
            var entity = await _db.ProductSkus.FindAsync(id) ?? throw new InvalidOperationException("Not found");
            entity.IsActive = false;
            await _db.SaveChangesAsync();
        }

        public async Task<ProductSkuDto> SaveSkuAsync(ProductSkuDto dto)
        {
            ProductSku entity;
            if (dto.Id == 0)
            {
                entity = new ProductSku();
                _db.ProductSkus.Add(entity);
            }
            else entity = await _db.ProductSkus.FindAsync(dto.Id) ?? throw new InvalidOperationException("Not found");
            entity.GroupId = dto.GroupId;
            entity.Code = dto.Code;
            entity.Name = dto.Name;
            entity.ExportMarket = dto.ExportMarket;
            entity.DefaultUnitPriceUsd = dto.DefaultUnitPriceUsd;
            entity.IsByproduct = dto.IsByproduct;
            entity.IsActive = dto.IsActive;
            await _db.SaveChangesAsync();
            dto.Id = entity.Id;
            return dto;
        }

        public Task<List<CustomerDto>> GetCustomersAsync() =>
            _db.Customers.OrderBy(c => c.Code).Select(c => new CustomerDto
            {
                Id = c.Id, Code = c.Code, Name = c.Name, ContactName = c.ContactName,
                Kind = c.Kind, Purchaser = c.Purchaser, Note = c.Note, IsActive = c.IsActive
            }).ToListAsync();

        public async Task<CustomerDto> SaveCustomerAsync(CustomerDto dto)
        {
            Customer entity;
            if (dto.Id == 0)
            {
                entity = new Customer();
                _db.Customers.Add(entity);
            }
            else entity = await _db.Customers.FindAsync(dto.Id) ?? throw new InvalidOperationException("Not found");
            entity.Code = dto.Code;
            entity.Name = dto.Name;
            entity.ContactName = dto.ContactName;
            entity.Kind = dto.Kind;
            entity.Purchaser = dto.Purchaser;
            entity.Note = dto.Note;
            entity.IsActive = dto.IsActive;
            await _db.SaveChangesAsync();
            dto.Id = entity.Id;
            return dto;
        }

        public async Task DeleteCustomerAsync(int id)
        {
            var used = await _db.InboundPurchases.AnyAsync(x => x.CustomerId == id)
                       || await _db.SalesContracts.AnyAsync(x => x.CustomerId == id);
            var entity = await _db.Customers.FindAsync(id) ?? throw new InvalidOperationException("Not found");
            if (used)
            {
                entity.IsActive = false;
            }
            else
            {
                _db.Customers.Remove(entity);
            }
            await _db.SaveChangesAsync();
        }

        public Task<List<PaymentTermDto>> GetPaymentTermsAsync() =>
            _db.PaymentTerms.OrderBy(x => x.Name).Select(x => new PaymentTermDto
            {
                Id = x.Id, Code = x.Code, Name = x.Name, RatiosJson = x.RatiosJson,
                DueDays = x.DueDays, IsLc = x.IsLc, IsActive = x.IsActive
            }).ToListAsync();

        public async Task<PaymentTermDto> SavePaymentTermAsync(PaymentTermDto dto)
        {
            PaymentTerm entity;
            if (dto.Id == 0)
            {
                entity = new PaymentTerm();
                _db.PaymentTerms.Add(entity);
            }
            else entity = await _db.PaymentTerms.FindAsync(dto.Id) ?? throw new InvalidOperationException("Not found");
            entity.Code = dto.Code;
            entity.Name = dto.Name;
            entity.RatiosJson = dto.RatiosJson;
            entity.DueDays = dto.DueDays;
            entity.IsLc = dto.IsLc;
            entity.IsActive = dto.IsActive;
            await _db.SaveChangesAsync();
            dto.Id = entity.Id;
            return dto;
        }

        public async Task DeletePaymentTermAsync(int id)
        {
            var used = await _db.ExportShipments.AnyAsync(x => x.PaymentTermId == id);
            var entity = await _db.PaymentTerms.FindAsync(id) ?? throw new InvalidOperationException("Not found");
            if (used) entity.IsActive = false;
            else _db.PaymentTerms.Remove(entity);
            await _db.SaveChangesAsync();
        }
    }
}
