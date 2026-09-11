using System.Net;
using Contract.Seafood;
using Core.Exceptions;
using Domain.Seafood;
using Microsoft.EntityFrameworkCore;
using SqlServ4r.EntityFramework;
using Volo.Abp.DependencyInjection;

namespace Application.Seafood;

public class SeafoodDocumentService : ITransientDependency
{
    private readonly DreamContext _db;

    public SeafoodDocumentService(DreamContext db) => _db = db;

    public async Task<List<DocumentTypeDto>> ListTypesAsync(bool includeInactive = false)
        => await _db.DocumentTypes.Where(x => includeInactive || x.IsActive).OrderBy(x => x.Code)
            .Select(x => new DocumentTypeDto { Id = x.Id, Code = x.Code, Name = x.Name, Description = x.Description, IsActive = x.IsActive })
            .ToListAsync();

    public async Task<SeafoodPagedResult<DocumentTypeDto>> ListTypesPageAsync(bool includeInactive, string? search, int skip, int take)
    {
        var query = _db.DocumentTypes.AsNoTracking().Where(x => includeInactive || x.IsActive);
        var term = search?.Trim();
        if (!string.IsNullOrWhiteSpace(term))
            query = query.Where(x => x.Code.Contains(term) || x.Name.Contains(term) || (x.Description ?? "").Contains(term));
        var total = await query.CountAsync();
        var items = await query.OrderBy(x => x.Code).Skip(Math.Max(0, skip)).Take(Math.Clamp(take, 1, 100))
            .Select(x => new DocumentTypeDto { Id = x.Id, Code = x.Code, Name = x.Name, Description = x.Description, IsActive = x.IsActive })
            .ToListAsync();
        return new SeafoodPagedResult<DocumentTypeDto> { Items = items, TotalCount = total };
    }

    public async Task<DocumentTypeDto> SaveTypeAsync(DocumentTypeDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Code) || string.IsNullOrWhiteSpace(dto.Name))
            throw new GlobalException("Mã và tên loại chứng từ là bắt buộc.", HttpStatusCode.BadRequest);
        var code = dto.Code.Trim();
        if (await _db.DocumentTypes.AnyAsync(x => x.Id != dto.Id && x.Code.ToUpper() == code.ToUpper()))
            throw new GlobalException("Mã loại chứng từ đã tồn tại.", HttpStatusCode.Conflict);

        var entity = dto.Id == 0
            ? new DocumentType()
            : await _db.DocumentTypes.FindAsync(dto.Id)
                ?? throw new GlobalException("Không tìm thấy loại chứng từ.", HttpStatusCode.NotFound);
        entity.Code = code;
        entity.Name = dto.Name.Trim();
        entity.Description = dto.Description?.Trim();
        entity.IsActive = dto.IsActive;
        if (dto.Id == 0) _db.DocumentTypes.Add(entity);
        await _db.SaveChangesAsync();
        return new DocumentTypeDto
        {
            Id = entity.Id, Code = entity.Code, Name = entity.Name,
            Description = entity.Description, IsActive = entity.IsActive
        };
    }

    public async Task<bool> DeleteTypeAsync(int id)
    {
        var entity = await _db.DocumentTypes.FindAsync(id);
        if (entity == null) return false;
        entity.IsActive = false;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<SeafoodSelect2SearchResponseDto> SearchTypesAsync(string? search, int page = 1, int pageSize = 20)
    {
        var term = search?.Trim();
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);
        var query = _db.DocumentTypes.AsNoTracking().Where(x => x.IsActive);
        if (!string.IsNullOrWhiteSpace(term))
            query = query.Where(x => x.Code.Contains(term) || x.Name.Contains(term) || (x.Description ?? "").Contains(term));

        var total = await query.CountAsync();
        var results = await query.OrderBy(x => x.Code)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(x => new SeafoodSelectOptionDto
            {
                Id = x.Id,
                Text = x.Code + " — " + x.Name,
                Description = x.Description
            }).ToListAsync();
        return new SeafoodSelect2SearchResponseDto { Results = results, More = page * pageSize < total };
    }

    public async Task<List<DocumentAttachmentDto>> ListAsync(string ownerType, int ownerId)
    {
        ValidateOwnerType(ownerType);
        var rows = await _db.DocumentAttachments.Include(x => x.DocumentType)
            .Where(x => x.OwnerType == ownerType && x.OwnerId == ownerId)
            .OrderByDescending(x => x.UploadedAt).ToListAsync();
        return rows.Select(Map).ToList();
    }

    public async Task<string> GetObjectKeyAsync(long id)
        => await _db.DocumentAttachments.Where(x => x.Id == id).Select(x => x.ObjectKey).FirstOrDefaultAsync()
            ?? throw new GlobalException("Không tìm thấy file chứng từ.", HttpStatusCode.NotFound);

    public async Task<DocumentAttachmentDto> SaveMetadataAsync(DocumentAttachment document)
    {
        ValidateOwnerType(document.OwnerType);
        if (string.IsNullOrWhiteSpace(document.ObjectKey) || string.IsNullOrWhiteSpace(document.FileName))
            throw new GlobalException("Thiếu metadata của file chứng từ.", HttpStatusCode.BadRequest);

        if (document.DocumentTypeId is int typeId && !await _db.DocumentTypes.AnyAsync(x => x.Id == typeId && x.IsActive))
            throw new GlobalException("Loại chứng từ không tồn tại hoặc đã bị khóa.", HttpStatusCode.BadRequest);

        _db.DocumentAttachments.Add(document);
        if (document.OwnerType.Equals("Shipment", StringComparison.OrdinalIgnoreCase))
            _db.ShipmentDocuments.Add(new ShipmentDocument { ShipmentId = document.OwnerId, DocumentAttachment = document, IsRequired = false });
        await _db.SaveChangesAsync();
        return Map(await _db.DocumentAttachments.Include(x => x.DocumentType).FirstAsync(x => x.Id == document.Id));
    }

    public async Task EnsureOwnerExistsAsync(string ownerType, int ownerId)
    {
        ValidateOwnerType(ownerType);
        var exists = ownerType switch
        {
            "InboundPurchase" => await _db.InboundPurchases.AnyAsync(x => x.Id == ownerId),
            "RawMaterialLot" => await _db.RawMaterialLots.AnyAsync(x => x.Id == ownerId),
            "ProductionLot" => await _db.ProductionLots.AnyAsync(x => x.Id == ownerId),
            "Shipment" => await _db.ExportShipments.AnyAsync(x => x.Id == ownerId),
            _ => false
        };
        if (!exists)
            throw new GlobalException("Không tìm thấy đối tượng gắn chứng từ.", HttpStatusCode.NotFound);
    }

    public async Task<DocumentAttachmentDto> VerifyAsync(long id, bool verified, int? userId, string? note)
    {
        var row = await _db.DocumentAttachments.Include(x => x.DocumentType).FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new GlobalException("Không tìm thấy chứng từ.", HttpStatusCode.NotFound);
        row.Status = verified ? DocumentStatus.Verified : DocumentStatus.Rejected;
        row.VerifiedByUserId = userId;
        row.VerifiedAt = DateTime.UtcNow;
        row.Note = note;
        await _db.SaveChangesAsync();
        return Map(row);
    }

    public async Task<DocumentChecklistDto> GetChecklistAsync(string ownerType, int ownerId, string marketCode, DateTime? effectiveDate)
    {
        ValidateOwnerType(ownerType);
        var date = effectiveDate ?? DateTime.UtcNow;
        var requirements = await _db.DocumentRequirements.Include(x => x.DocumentType)
            .Where(x => x.MarketCode == marketCode && x.DocumentType!.IsActive)
            .OrderBy(x => x.SortOrder).ToListAsync();
        var documents = await _db.DocumentAttachments.Include(x => x.DocumentType)
            .Where(x => x.OwnerType == ownerType && x.OwnerId == ownerId).ToListAsync();
        if (ownerType.Equals("RawMaterialLot", StringComparison.OrdinalIgnoreCase))
        {
            var inboundId = await _db.RawMaterialLots.Where(x => x.Id == ownerId).Select(x => (int?)x.InboundPurchaseId).FirstOrDefaultAsync();
            if (inboundId is int purchaseId)
                documents.AddRange(await _db.DocumentAttachments.Include(x => x.DocumentType)
                    .Where(x => x.OwnerType == "InboundPurchase" && x.OwnerId == purchaseId).ToListAsync());
        }

        return new DocumentChecklistDto
        {
            MarketCode = marketCode,
            EffectiveDate = date,
            Items = requirements.Select(requirement =>
            {
                var valid = documents.Any(d => d.DocumentTypeId == requirement.DocumentTypeId
                    && d.Status == DocumentStatus.Verified
                    && (!d.ExpiryDate.HasValue || d.ExpiryDate.Value.Date >= date.Date));
                return new DocumentChecklistItemDto
                {
                    DocumentCode = requirement.DocumentType!.Code,
                    DocumentName = requirement.DocumentType.Name,
                    IsRequired = requirement.IsRequired,
                    IsValid = valid,
                    Reason = valid ? null : requirement.IsRequired ? "Chưa có file Verified còn hiệu lực tại ngày ETD." : "Chứng từ bổ sung chưa được đính kèm hoặc chưa Verified."
                };
            }).ToList()
        };
    }

    public async Task<(List<string> Required, List<string> Missing, List<string> Optional)> GetRequiredDocumentStatusAsync(
        IEnumerable<(string OwnerType, int OwnerId)> owners, string marketCode, DateTime effectiveDate)
    {
        var requirements = await _db.DocumentRequirements.Include(x => x.DocumentType)
            .Where(x => x.MarketCode == marketCode && x.DocumentType!.IsActive)
            .OrderBy(x => x.SortOrder).ToListAsync();
        var ownerPairs = owners.Distinct().ToList();
        var rawLotIds = ownerPairs.Where(owner => owner.OwnerType == "RawMaterialLot").Select(owner => owner.OwnerId).ToList();
        var inheritedInboundIds = await _db.RawMaterialLots
            .Where(x => rawLotIds.Contains(x.Id))
            .Select(x => x.InboundPurchaseId).Distinct().ToListAsync();
        var ownerTypes = ownerPairs.Select(x => x.OwnerType).Concat(inheritedInboundIds.Select(_ => "InboundPurchase")).Distinct().ToList();
        var ownerIds = ownerPairs.Select(x => x.OwnerId).Concat(inheritedInboundIds).Distinct().ToList();
        var documents = await _db.DocumentAttachments
            .Where(x => ownerTypes.Contains(x.OwnerType) && ownerIds.Contains(x.OwnerId))
            .ToListAsync();
        var rawInboundByLot = await _db.RawMaterialLots
            .Where(x => rawLotIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => x.InboundPurchaseId);

        var required = requirements.Where(x => x.IsRequired).Select(x => x.DocumentType!.Code).ToList();
        var optional = requirements.Where(x => !x.IsRequired).Select(x => x.DocumentType!.Code).ToList();
        var missing = requirements.Where(x => x.IsRequired).Where(requirement => ownerPairs.Any(owner => !HasValidDocument(owner, requirement.DocumentTypeId, effectiveDate, documents, rawInboundByLot)))
            .Select(x => x.DocumentType!.Code).ToList();
        return (required, missing, optional);
    }

    private static bool HasValidDocument((string OwnerType, int OwnerId) owner, int documentTypeId, DateTime effectiveDate,
        IEnumerable<DocumentAttachment> documents, IReadOnlyDictionary<int, int> rawInboundByLot)
    {
        var valid = documents.Where(document => document.DocumentTypeId == documentTypeId
            && document.Status == DocumentStatus.Verified
            && (!document.ExpiryDate.HasValue || document.ExpiryDate.Value.Date >= effectiveDate.Date));
        if (valid.Any(document => document.OwnerType == owner.OwnerType && document.OwnerId == owner.OwnerId)) return true;
        return owner.OwnerType == "RawMaterialLot"
            && rawInboundByLot.TryGetValue(owner.OwnerId, out var inboundId)
            && valid.Any(document => document.OwnerType == "InboundPurchase" && document.OwnerId == inboundId);
    }

    public static void ValidateOwnerType(string ownerType)
    {
        if (!new[] { "InboundPurchase", "RawMaterialLot", "ProductionLot", "Shipment" }
            .Contains(ownerType, StringComparer.OrdinalIgnoreCase))
            throw new GlobalException("OwnerType chứng từ không hợp lệ.", HttpStatusCode.BadRequest);
    }

    private static DocumentAttachmentDto Map(DocumentAttachment x) => new()
    {
        Id = x.Id,
        OwnerType = x.OwnerType,
        OwnerId = x.OwnerId,
        DocumentTypeId = x.DocumentTypeId,
        DocumentTypeCode = x.DocumentType?.Code,
        DocumentTypeName = x.DocumentType?.Name,
        DocumentNo = x.DocumentNo,
        IssueDate = x.IssueDate,
        ExpiryDate = x.ExpiryDate,
        Status = x.Status,
        FileName = x.FileName,
        FileSize = x.FileSize,
        UploadedAt = x.UploadedAt,
        Note = x.Note
    };
}

public class SeafoodAllocationService : ITransientDependency
{
    private readonly DreamContext _db;
    private readonly SeafoodDocumentService _documents;

    public SeafoodAllocationService(DreamContext db, SeafoodDocumentService documents)
    {
        _db = db;
        _documents = documents;
    }

    public async Task<AllocationPreviewDto> PreviewAsync(AllocationPreviewRequestDto request)
    {
        var contract = await _db.SalesContracts.Include(x => x.Lines).FirstOrDefaultAsync(x => x.Id == request.SalesContractId)
            ?? throw new GlobalException("Không tìm thấy hợp đồng.", HttpStatusCode.NotFound);
        var marketCode = await GetMarketCodeAsync(contract.MarketId);
        var effectiveDate = request.Etd ?? DateTime.UtcNow;
        var balances = await _db.InventoryBalances
            .Include(x => x.Lot)!.ThenInclude(x => x!.Certificates)
            .Include(x => x.Sku)
            .OrderBy(x => x.Lot!.ReceivedDate).ThenBy(x => x.LotId).ToListAsync();
        var activeReservations = await _db.StockReservations
            .Where(x => x.Status == ReservationStatus.Active)
            .GroupBy(x => x.InventoryBalanceId)
            .Select(x => new { x.Key, Quantity = x.Sum(y => y.QuantityKg) })
            .ToDictionaryAsync(x => x.Key, x => x.Quantity);

        var candidates = new List<AllocationCandidateDto>();
        var previewUsedByBalance = new Dictionary<int, decimal>();
        decimal required = 0;
        decimal allocatable = 0;
        foreach (var line in contract.Lines)
        {
            required += line.QtyKg;
            var remaining = line.QtyKg;
            foreach (var balance in balances.Where(x => x.SkuId == line.SkuId))
            {
                var reserved = Math.Max(balance.AllocatedKg, activeReservations.GetValueOrDefault(balance.Id));
                var available = Math.Max(0, balance.OnHandKg - reserved - previewUsedByBalance.GetValueOrDefault(balance.Id));
                if (available <= 0) continue;
                var owners = await GetTraceabilityOwnersAsync(balance.LotId);
                var documentStatus = await _documents.GetRequiredDocumentStatusAsync(owners, marketCode, effectiveDate);
                var suggested = Math.Min(remaining, available);
                if (documentStatus.Missing.Count == 0)
                {
                    allocatable += suggested;
                    remaining -= suggested;
                    previewUsedByBalance[balance.Id] = previewUsedByBalance.GetValueOrDefault(balance.Id) + suggested;
                }
                candidates.Add(new AllocationCandidateDto
                {
                    ContractLineId = line.Id,
                    InventoryBalanceId = balance.Id,
                    LotId = balance.LotId,
                    LotNumber = balance.Lot?.LotNumber ?? string.Empty,
                    SkuId = balance.SkuId,
                    SkuName = balance.Sku?.Name ?? string.Empty,
                    AvailableKg = available,
                    SuggestedKg = documentStatus.Missing.Count == 0 ? suggested : 0,
                    IsDocumentReady = documentStatus.Missing.Count == 0,
                    RequiredDocuments = documentStatus.Required,
                    SupplementalDocuments = documentStatus.Optional,
                    MissingDocuments = documentStatus.Missing
                });
            }
        }

        return new AllocationPreviewDto
        {
            SalesContractId = contract.Id,
            RequiredKg = required,
            AllocatableKg = allocatable,
            MissingKg = Math.Max(0, required - allocatable),
            HasRequiredDocuments = candidates.Where(x => x.SuggestedKg > 0).All(x => x.IsDocumentReady),
            Candidates = candidates
        };
    }

    public async Task<AllocationResultDto> ConfirmAsync(AllocationConfirmDto request, int? userId)
    {
        if (request.Items.Count == 0)
            throw new GlobalException("Phương án xếp lô chưa có dòng phân bổ.", HttpStatusCode.BadRequest);
        if (request.OverrideDocumentCheck && string.IsNullOrWhiteSpace(request.OverrideReason))
            throw new GlobalException("Override thiếu giấy tờ phải có lý do.", HttpStatusCode.BadRequest);

        await using var transaction = await _db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
        var contract = await _db.SalesContracts.Include(x => x.Lines).FirstOrDefaultAsync(x => x.Id == request.SalesContractId)
            ?? throw new GlobalException("Không tìm thấy hợp đồng.", HttpStatusCode.NotFound);
        var marketCode = await GetMarketCodeAsync(contract.MarketId);
        var etd = request.Etd ?? DateTime.UtcNow;
        var balances = await _db.InventoryBalances.Include(x => x.Lot)!.ThenInclude(x => x!.Inputs).ToListAsync();
        var lineById = contract.Lines.ToDictionary(x => x.Id);
        var requestedByLine = request.Items.GroupBy(x => x.SalesContractLineId).ToDictionary(x => x.Key, x => x.Sum(y => y.QuantityKg));
        var existingByLine = await _db.StockReservations
            .Where(x => x.Status == ReservationStatus.Active && x.SalesContractLine!.ContractId == contract.Id)
            .GroupBy(x => x.SalesContractLineId).Select(x => new { x.Key, Quantity = x.Sum(y => y.QuantityKg) })
            .ToDictionaryAsync(x => x.Key, x => x.Quantity);

        foreach (var group in requestedByLine)
        {
            if (!lineById.TryGetValue(group.Key, out var line))
                throw new GlobalException("Dòng phân bổ không thuộc hợp đồng.", HttpStatusCode.BadRequest);
            if (group.Value <= 0 || existingByLine.GetValueOrDefault(group.Key) + group.Value > line.QtyKg + 0.0001m)
                throw new GlobalException($"Phân bổ SKU {line.SkuId} vượt số lượng hợp đồng.", HttpStatusCode.BadRequest);
        }

        var resultItems = new List<AllocationItemDto>();
        foreach (var item in request.Items)
        {
            if (item.QuantityKg <= 0)
                throw new GlobalException("Số lượng từng dòng phân bổ phải lớn hơn 0.", HttpStatusCode.BadRequest);
            var balance = balances.FirstOrDefault(x => x.Id == item.InventoryBalanceId)
                ?? throw new GlobalException("Không tìm thấy dòng tồn kho.", HttpStatusCode.NotFound);
            var line = lineById[item.SalesContractLineId];
            if (balance.SkuId != line.SkuId)
                throw new GlobalException("SKU tồn kho không khớp với dòng hợp đồng.", HttpStatusCode.BadRequest);
            var active = await _db.StockReservations.Where(x => x.InventoryBalanceId == balance.Id && x.Status == ReservationStatus.Active).SumAsync(x => (decimal?)x.QuantityKg) ?? 0;
            if (balance.OnHandKg - Math.Max(balance.AllocatedKg, active) < item.QuantityKg - 0.0001m)
                throw new GlobalException($"Lô {balance.Lot?.LotNumber} không còn đủ hàng khả dụng.", HttpStatusCode.Conflict);
            var owners = await GetTraceabilityOwnersAsync(balance.LotId);
            var status = await _documents.GetRequiredDocumentStatusAsync(owners, marketCode, etd);
            if (status.Missing.Count > 0 && !request.OverrideDocumentCheck)
                throw new GlobalException($"Lô {balance.Lot?.LotNumber} thiếu giấy: {string.Join(", ", status.Missing)}.", HttpStatusCode.BadRequest);

            balance.AllocatedKg += item.QuantityKg;
            line.AllocatedKg += item.QuantityKg;
            _db.StockReservations.Add(new StockReservation
            {
                SalesContractLineId = item.SalesContractLineId,
                InventoryBalanceId = item.InventoryBalanceId,
                QuantityKg = item.QuantityKg,
                CreatedByUserId = userId,
                Reason = request.OverrideDocumentCheck ? request.OverrideReason : null
            });
            _db.InventoryMovements.Add(new InventoryMovement
            {
                InventoryBalanceId = item.InventoryBalanceId,
                MovementType = InventoryMovementType.Reservation,
                QuantityKg = item.QuantityKg,
                ReferenceType = "SalesContract",
                ReferenceId = contract.Id,
                CreatedByUserId = userId,
                Reason = request.Reason
            });
            _db.SalesAllocations.Add(new SalesAllocation
            {
                SalesContractLineId = item.SalesContractLineId,
                InventoryBalanceId = item.InventoryBalanceId,
                QuantityKg = item.QuantityKg
            });
            _db.TraceabilityLinks.Add(new TraceabilityLink
            {
                FromType = "ProductionLot",
                FromId = balance.LotId,
                ToType = "SalesContractLine",
                ToId = item.SalesContractLineId,
                QuantityKg = item.QuantityKg
            });
            resultItems.Add(item);
        }

        var totalAllocated = await _db.StockReservations.Where(x => x.Status == ReservationStatus.Active && x.SalesContractLine!.ContractId == contract.Id)
            .SumAsync(x => (decimal?)x.QuantityKg) ?? 0;
        var totalRequired = contract.Lines.Sum(x => x.QtyKg);
        contract.Status = totalAllocated + 0.0001m >= totalRequired ? ContractStatus.ReadyDocs : ContractStatus.ReadyStock;
        _db.DomainAuditLogs.Add(new DomainAuditLog
        {
            EntityType = "SalesContract",
            EntityId = contract.Id,
            Action = "AllocationConfirmed",
            UserId = userId,
            Reason = request.OverrideDocumentCheck ? request.OverrideReason : null
        });
        await _db.SaveChangesAsync();
        await transaction.CommitAsync();
        return new AllocationResultDto
        {
            SalesContractId = contract.Id,
            AllocatedKg = resultItems.Sum(x => x.QuantityKg),
            MissingKg = Math.Max(0, totalRequired - totalAllocated),
            Items = resultItems
        };
    }

    public async Task ReleaseAsync(AllocationReleaseDto request, int? userId)
    {
        if (request.SalesContractId <= 0)
            throw new GlobalException("Thiếu hợp đồng cần giải phóng reservation.", HttpStatusCode.BadRequest);
        await using var transaction = await _db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
        var reservations = await _db.StockReservations.Include(x => x.InventoryBalance)
            .Where(x => x.Status == ReservationStatus.Active && x.SalesContractLine!.ContractId == request.SalesContractId)
            .ToListAsync();
        if (reservations.Count == 0)
            throw new GlobalException("Hợp đồng không còn reservation đang giữ.", HttpStatusCode.NotFound);
        foreach (var reservation in reservations)
        {
            reservation.Status = ReservationStatus.Released;
            reservation.ReleasedAt = DateTime.UtcNow;
            reservation.Reason = request.Reason;
            reservation.InventoryBalance!.AllocatedKg = Math.Max(0, reservation.InventoryBalance.AllocatedKg - reservation.QuantityKg);
            _db.InventoryMovements.Add(new InventoryMovement
            {
                InventoryBalanceId = reservation.InventoryBalanceId,
                MovementType = InventoryMovementType.ReservationRelease,
                QuantityKg = -reservation.QuantityKg,
                ReferenceType = "SalesContract",
                ReferenceId = request.SalesContractId,
                CreatedByUserId = userId,
                Reason = request.Reason
            });
        }
        var allocations = await _db.SalesAllocations
            .Where(x => x.IsActive && x.ShipmentId == null && x.SalesContractLine!.ContractId == request.SalesContractId)
            .ToListAsync();
        foreach (var allocation in allocations)
        {
            allocation.IsActive = false;
            allocation.CancelledAt = DateTime.UtcNow;
        }
        var contract = await _db.SalesContracts.Include(x => x.Lines).FirstAsync(x => x.Id == request.SalesContractId);
        foreach (var line in contract.Lines)
            line.AllocatedKg = Math.Max(0, line.AllocatedKg - reservations.Where(x => x.SalesContractLineId == line.Id).Sum(x => x.QuantityKg));
        contract.Status = ContractStatus.Signed;
        _db.DomainAuditLogs.Add(new DomainAuditLog { EntityType = "SalesContract", EntityId = request.SalesContractId, Action = "AllocationReleased", UserId = userId, Reason = request.Reason });
        await _db.SaveChangesAsync();
        await transaction.CommitAsync();
    }

    private async Task<string> GetMarketCodeAsync(int? marketId)
        => await _db.CatalogLookups.Where(x => x.Id == marketId && x.Category == LookupCategory.Market)
            .Select(x => x.Code).FirstOrDefaultAsync() ?? "OTHER";

    private async Task<List<(string OwnerType, int OwnerId)>> GetTraceabilityOwnersAsync(int productionLotId)
    {
        var lot = await _db.ProductionLots.Include(x => x.Inputs).FirstAsync(x => x.Id == productionLotId);
        var owners = new List<(string, int)> { ("ProductionLot", productionLotId) };
        owners.AddRange(lot.Inputs.Select(x => ("RawMaterialLot", x.RawMaterialLotId)));
        return owners;
    }
}
