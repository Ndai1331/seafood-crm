using System.Net;
using Contract.Seafood;
using Core.Exceptions;
using Domain.Seafood;
using Microsoft.EntityFrameworkCore;
using SqlServ4r.EntityFramework;
using Volo.Abp.DependencyInjection;

namespace Application.Seafood;

public class SeafoodPartnerService : ITransientDependency
{
    private readonly DreamContext _db;
    public SeafoodPartnerService(DreamContext db) => _db = db;

    public async Task<List<BusinessPartnerDto>> ListPartnersAsync(BusinessPartnerRole? role)
    {
        var query = _db.BusinessPartners.Include(x => x.Roles).AsQueryable();
        if (role.HasValue) query = query.Where(x => x.Roles.Any(r => r.Role == role.Value));
        return (await query.OrderBy(x => x.Code).ToListAsync()).Select(Map).ToList();
    }

    public async Task<SeafoodPagedResult<BusinessPartnerDto>> ListPartnersPageAsync(BusinessPartnerRole? role, string? search, int skip, int take)
    {
        var query = _db.BusinessPartners.Include(x => x.Roles).AsNoTracking().AsQueryable();
        if (role.HasValue) query = query.Where(x => x.Roles.Any(r => r.Role == role.Value));
        var term = search?.Trim();
        if (!string.IsNullOrWhiteSpace(term))
            query = query.Where(x => x.Code.Contains(term) || x.Name.Contains(term) || (x.ContactName ?? "").Contains(term)
                || (x.TaxCode ?? "").Contains(term) || (x.Note ?? "").Contains(term));
        var total = await query.CountAsync();
        var rows = await query.OrderBy(x => x.Code).Skip(Math.Max(0, skip)).Take(Math.Clamp(take, 1, 100)).ToListAsync();
        return new SeafoodPagedResult<BusinessPartnerDto> { Items = rows.Select(Map).ToList(), TotalCount = total };
    }

    public async Task<SeafoodSelect2SearchResponseDto> SearchOptionsAsync(BusinessPartnerRole? role, string? search, int page = 1, int pageSize = 20)
    {
        var query = _db.BusinessPartners.AsNoTracking().Where(x => x.IsActive);
        if (role.HasValue) query = query.Where(x => x.Roles.Any(r => r.Role == role.Value));
        var term = search?.Trim();
        if (!string.IsNullOrWhiteSpace(term))
            query = query.Where(x => x.Code.Contains(term) || x.Name.Contains(term) || (x.ContactName ?? "").Contains(term));
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);
        var total = await query.CountAsync();
        var results = await query.OrderBy(x => x.Code).Skip((page - 1) * pageSize).Take(pageSize)
            .Select(x => new SeafoodSelectOptionDto { Id = x.Id, Text = x.Code + " — " + x.Name, Description = x.ContactName })
            .ToListAsync();
        return new SeafoodSelect2SearchResponseDto { Results = results, More = page * pageSize < total };
    }

    public async Task<BusinessPartnerDto> SavePartnerAsync(BusinessPartnerDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Code) || string.IsNullOrWhiteSpace(dto.Name))
            throw new GlobalException("Mã và tên đối tác là bắt buộc.", HttpStatusCode.BadRequest);
        if (dto.Roles.Any(role => !Enum.IsDefined(typeof(BusinessPartnerRole), role)))
            throw new GlobalException("Vai trò đối tác không hợp lệ.", HttpStatusCode.BadRequest);
        var code = dto.Code.Trim();
        if (await _db.BusinessPartners.AnyAsync(x => x.Id != dto.Id && x.Code.ToUpper() == code.ToUpper()))
            throw new GlobalException("Mã đối tác đã tồn tại.", HttpStatusCode.Conflict);
        var entity = dto.Id == 0 ? new BusinessPartner() : await _db.BusinessPartners.Include(x => x.Roles).FirstOrDefaultAsync(x => x.Id == dto.Id)
            ?? throw new GlobalException("Không tìm thấy đối tác.", HttpStatusCode.NotFound);
        entity.Code = code; entity.Name = dto.Name.Trim(); entity.ContactName = dto.ContactName?.Trim(); entity.TaxCode = dto.TaxCode?.Trim(); entity.Note = dto.Note?.Trim(); entity.IsActive = dto.IsActive;
        if (dto.Id > 0) _db.BusinessPartnerRoles.RemoveRange(entity.Roles);
        entity.Roles = dto.Roles.Distinct().Select(x => new BusinessPartnerRoleLink { BusinessPartner = entity, BusinessPartnerId = entity.Id, Role = x }).ToList();
        await _db.SaveChangesAsync();
        return Map(await _db.BusinessPartners.Include(x => x.Roles).FirstAsync(x => x.Id == entity.Id));
    }

    public async Task<List<VesselDto>> ListVesselsAsync()
        => (await _db.Vessels.Include(x => x.OwnerPartner).OrderBy(x => x.Code).ToListAsync()).Select(Map).ToList();

    public async Task<SeafoodPagedResult<VesselDto>> ListVesselsPageAsync(string? search, int skip, int take)
    {
        var query = _db.Vessels.Include(x => x.OwnerPartner).AsNoTracking().AsQueryable();
        var term = search?.Trim();
        if (!string.IsNullOrWhiteSpace(term))
            query = query.Where(x => x.Code.Contains(term) || x.Name.Contains(term) || (x.Flag ?? "").Contains(term)
                || (x.OwnerPartner != null && x.OwnerPartner.Name.Contains(term)) || (x.Note ?? "").Contains(term));
        var total = await query.CountAsync();
        var rows = await query.OrderBy(x => x.Code).Skip(Math.Max(0, skip)).Take(Math.Clamp(take, 1, 100)).ToListAsync();
        return new SeafoodPagedResult<VesselDto> { Items = rows.Select(Map).ToList(), TotalCount = total };
    }

    public async Task<VesselDto> SaveVesselAsync(VesselDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Code) || string.IsNullOrWhiteSpace(dto.Name))
            throw new GlobalException("Mã và tên tàu là bắt buộc.", HttpStatusCode.BadRequest);
        var code = dto.Code.Trim();
        if (await _db.Vessels.AnyAsync(x => x.Id != dto.Id && x.Code.ToUpper() == code.ToUpper()))
            throw new GlobalException("Mã tàu đã tồn tại.", HttpStatusCode.Conflict);
        if (dto.OwnerPartnerId is int ownerId && !await _db.BusinessPartners.AnyAsync(x => x.Id == ownerId && x.IsActive))
            throw new GlobalException("Chủ tàu không tồn tại hoặc đã khóa.", HttpStatusCode.BadRequest);
        var entity = dto.Id == 0 ? new Vessel() : await _db.Vessels.FirstOrDefaultAsync(x => x.Id == dto.Id)
            ?? throw new GlobalException("Không tìm thấy tàu.", HttpStatusCode.NotFound);
        entity.Code = code; entity.Name = dto.Name.Trim(); entity.Flag = dto.Flag?.Trim(); entity.OwnerPartnerId = dto.OwnerPartnerId; entity.Note = dto.Note?.Trim(); entity.IsActive = dto.IsActive;
        await _db.SaveChangesAsync();
        return Map(await _db.Vessels.Include(x => x.OwnerPartner).FirstAsync(x => x.Id == entity.Id));
    }

    public async Task<bool> DeletePartnerAsync(int id)
    {
        var entity = await _db.BusinessPartners.FindAsync(id);
        if (entity == null) return false;
        entity.IsActive = false;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteVesselAsync(int id)
    {
        var entity = await _db.Vessels.FindAsync(id);
        if (entity == null) return false;
        entity.IsActive = false;
        await _db.SaveChangesAsync();
        return true;
    }

    private static BusinessPartnerDto Map(BusinessPartner x) => new()
    {
        Id = x.Id, Code = x.Code, Name = x.Name, ContactName = x.ContactName, TaxCode = x.TaxCode, Note = x.Note,
        IsActive = x.IsActive, Roles = x.Roles.Select(r => r.Role).ToList()
    };

    private static VesselDto Map(Vessel x) => new()
    {
        Id = x.Id, Code = x.Code, Name = x.Name, Flag = x.Flag, OwnerPartnerId = x.OwnerPartnerId,
        OwnerPartnerName = x.OwnerPartner?.Name, Note = x.Note, IsActive = x.IsActive
    };
}
