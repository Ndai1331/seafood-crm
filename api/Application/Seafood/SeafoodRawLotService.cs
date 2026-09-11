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
        return (await query.OrderBy(x => x.ReceivedDate).Take(500).ToListAsync()).Select(x => new RawMaterialLotDto
        {
            Id = x.Id, LotNumber = x.LotNumber, InboundPurchaseId = x.InboundPurchaseId, InboundLineId = x.InboundLineId,
            SupplierName = x.SupplierPartner?.Name, VesselName = x.Vessel?.Name, ReceivedDate = x.ReceivedDate,
            DeclaredKg = x.DeclaredKg, ActualKg = x.ActualKg, ConsumedKg = x.ProductionInputs.Sum(i => i.QuantityKg),
            AvailableKg = Math.Max(0, x.ActualKg - x.ProductionInputs.Sum(i => i.QuantityKg)), FishFormCode = x.FishFormCode,
            SizeCode = x.SizeCode, CatchMethodCode = x.CatchMethodCode, FreezeMethodCode = x.FreezeMethodCode, OriginCode = x.OriginCode
        }).ToList();
    }

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
