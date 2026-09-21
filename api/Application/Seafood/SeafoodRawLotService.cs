using Contract.Seafood;
using Domain.Seafood;
using Microsoft.EntityFrameworkCore;
using SqlServ4r.EntityFramework;
using Volo.Abp.DependencyInjection;

namespace Application.Seafood;

public class SeafoodRawLotService : ITransientDependency
{
    private readonly DreamContext _db;
    public SeafoodRawLotService(DreamContext db) => _db = db;

    public async Task<List<RawMaterialLotDto>> ListAsync(string? search = null)
    {
        var query = _db.RawMaterialLots.Include(x => x.ProductionInputs).Include(x => x.SupplierPartner).Include(x => x.Vessel).AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(x => x.LotNumber.Contains(term) || (x.FishFormCode ?? "").Contains(term) || (x.SizeCode ?? "").Contains(term));
        }
        return (await query.OrderBy(x => x.ReceivedDate).Take(500).ToListAsync()).Select(Map).ToList();
    }

    public async Task<SeafoodPagedResult<RawMaterialLotDto>> PageAsync(string? search, int skip, int take)
    {
        var query = _db.RawMaterialLots.AsNoTracking();
        var term = search?.Trim();
        if (!string.IsNullOrWhiteSpace(term))
            query = query.Where(x => x.LotNumber.Contains(term) || (x.FishFormCode ?? "").Contains(term) || (x.SizeCode ?? "").Contains(term)
                || (x.FishSpeciesCode ?? "").Contains(term));
        var total = await query.CountAsync();
        var rows = await _db.RawMaterialLots.Include(x => x.ProductionInputs).Include(x => x.SupplierPartner).Include(x => x.Vessel)
            .Where(x => string.IsNullOrWhiteSpace(term) || x.LotNumber.Contains(term) || (x.FishFormCode ?? "").Contains(term)
                || (x.SizeCode ?? "").Contains(term) || (x.FishSpeciesCode ?? "").Contains(term))
            .OrderByDescending(x => x.ReceivedDate).Skip(Math.Max(0, skip)).Take(Math.Clamp(take, 1, 100)).ToListAsync();
        var ids = rows.Select(x => x.Id).ToList();
        var docs = await _db.DocumentAttachments.Include(x => x.DocumentType)
            .Where(x => x.OwnerType == "RawMaterialLot" && ids.Contains(x.OwnerId)).ToListAsync();
        var inboundIds = rows.Select(x => x.InboundPurchaseId).Distinct().ToList();
        var inboundDocs = await _db.DocumentAttachments.Include(x => x.DocumentType)
            .Where(x => x.OwnerType == "InboundPurchase" && inboundIds.Contains(x.OwnerId)).ToListAsync();
        var warehouses = await _db.CatalogLookups.AsNoTracking()
            .Where(x => x.Category == LookupCategory.Warehouse)
            .ToDictionaryAsync(x => x.Id, x => x.Name);
        return new SeafoodPagedResult<RawMaterialLotDto>
        {
            TotalCount = total,
            Items = rows.Select(x =>
            {
                var dto = Map(x);
                dto.WarehouseName = warehouses.GetValueOrDefault(x.WarehouseId ?? 0);
                dto.Documents = docs.Where(d => d.OwnerId == x.Id)
                    .Concat(inboundDocs.Where(d => d.OwnerId == x.InboundPurchaseId))
                    .Select(d => new DocumentAttachmentDto
                    {
                        Id = d.Id, OwnerType = d.OwnerType, OwnerId = d.OwnerId, DocumentTypeId = d.DocumentTypeId,
                        DocumentTypeCode = d.DocumentType?.Code, DocumentTypeName = d.DocumentType?.Name, FileName = d.FileName,
                        Status = d.Status, UploadedAt = d.UploadedAt
                    }).ToList();
                return dto;
            }).ToList()
        };
    }

    private static RawMaterialLotDto Map(RawMaterialLot x) => new()
    {
        Id = x.Id, LotNumber = x.LotNumber, InboundPurchaseId = x.InboundPurchaseId, InboundLineId = x.InboundLineId,
        SupplierName = x.SupplierPartner?.Name, VesselName = x.Vessel?.Name, ReceivedDate = x.ReceivedDate,
        DeclaredKg = x.DeclaredKg, ActualKg = x.ActualKg, ConsumedKg = x.ProductionInputs.Sum(i => i.QuantityKg),
        AvailableKg = Math.Max(0, x.ActualKg - x.ProductionInputs.Sum(i => i.QuantityKg)),
        FishSpeciesCode = x.FishSpeciesCode, SensoryCode = x.SensoryCode, WarehouseId = x.WarehouseId,
        FishFormCode = x.FishFormCode,
        SizeCode = x.SizeCode, CatchMethodCode = x.CatchMethodCode, FreezeMethodCode = x.FreezeMethodCode, OriginCode = x.OriginCode
    };

    public async Task<SeafoodSelect2SearchResponseDto> SearchOptionsAsync(string? search, int page = 1, int pageSize = 20)
    {
        var term = search?.Trim();
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);
        var query = _db.RawMaterialLots.AsNoTracking()
            .Include(x => x.ProductionInputs)
            .Include(x => x.SupplierPartner)
            .Include(x => x.Vessel)
            .AsQueryable();
        if (!string.IsNullOrWhiteSpace(term))
            query = query.Where(x => x.LotNumber.Contains(term) || (x.FishFormCode ?? "").Contains(term) || (x.SizeCode ?? "").Contains(term));

        var total = await query.CountAsync();
        var lots = await query.OrderBy(x => x.ReceivedDate).ThenBy(x => x.LotNumber)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        var results = lots.Select(x => new SeafoodSelectOptionDto
        {
            Id = x.Id,
            Text = x.LotNumber + " — còn " + Math.Max(0, x.ActualKg - x.ProductionInputs.Sum(i => i.QuantityKg)).ToString("N0") + " kg",
            Description = string.Join(" · ", new[] { x.SupplierPartner?.Name, x.Vessel?.Name, x.FishFormCode, x.SizeCode }.Where(v => !string.IsNullOrWhiteSpace(v)))
        }).ToList();
        return new SeafoodSelect2SearchResponseDto { Results = results, More = page * pageSize < total };
    }
}
