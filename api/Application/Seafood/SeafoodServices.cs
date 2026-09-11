using Contract.Seafood;
using Core.Helper;
using Core.Exceptions;
using Domain.Seafood;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SqlServ4r.EntityFramework;
using System.Net;
using Volo.Abp.DependencyInjection;

namespace Application.Seafood
{
    public class InboundService : ITransientDependency
    {
        private readonly DreamContext _db;
        public InboundService(DreamContext db) => _db = db;

        public async Task<List<InboundPurchaseDto>> ListAsync(string? search = null, int skip = 0, int take = 100)
        {
            take = Math.Clamp(take, 1, 200);
            skip = Math.Max(0, skip);
            var query = _db.InboundPurchases
                .Include(x => x.Customer).Include(x => x.SupplierPartner).Include(x => x.Vessel)
                .Include(x => x.Lines).ThenInclude(x => x.RawMaterialLot)
                .AsQueryable();
            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(x => (x.ContractNo ?? "").Contains(term) || (x.BlNumber ?? "").Contains(term)
                    || (x.ContainerNo ?? "").Contains(term) || (x.Purchaser ?? "").Contains(term)
                    || (x.CustomerCode ?? "").Contains(term));
            }
            var rows = await query.OrderByDescending(x => x.Id).Skip(skip).Take(take).ToListAsync();
            var documents = await _db.DocumentAttachments.Include(x => x.DocumentType)
                .Where(x => x.OwnerType == "InboundPurchase")
                .ToListAsync();
            return rows.Select(x => Map(x, documents.Where(d => d.OwnerId == x.Id))).ToList();
        }

        public async Task<SeafoodPagedResult<InboundPurchaseDto>> PageAsync(string? search, int skip, int take)
        {
            var query = _db.InboundPurchases.AsNoTracking();
            var term = search?.Trim();
            if (!string.IsNullOrWhiteSpace(term))
                query = query.Where(x => (x.ContractNo ?? "").Contains(term) || (x.BlNumber ?? "").Contains(term)
                    || (x.ContainerNo ?? "").Contains(term) || (x.Purchaser ?? "").Contains(term) || (x.CustomerCode ?? "").Contains(term));
            var total = await query.CountAsync();
            var items = await ListAsync(search, skip, take);
            return new SeafoodPagedResult<InboundPurchaseDto> { Items = items, TotalCount = total };
        }

        public async Task<InboundPurchaseDto> SaveAsync(InboundPurchaseDto dto)
        {
            if (dto.Lines.Count == 0)
                throw new GlobalException("Phiếu nhập phải có ít nhất một dòng hàng.", HttpStatusCode.BadRequest);
            if (dto.Lines.Any(x => x.QtyKg < 0 || x.QtyKgLongLine < 0 || x.QtyKgHandline < 0 || x.QtyKgPs < 0 || x.QtyKgLand < 0))
                throw new GlobalException("Khối lượng nhập không được âm.", HttpStatusCode.BadRequest);
            var duplicate = await _db.InboundPurchases.AnyAsync(x => x.Id != dto.Id &&
                ((!string.IsNullOrWhiteSpace(dto.BlNumber) && x.BlNumber == dto.BlNumber)
                || (!string.IsNullOrWhiteSpace(dto.ContainerNo) && x.ContainerNo == dto.ContainerNo)
                || (!string.IsNullOrWhiteSpace(dto.PurchaseInvoiceNo) && x.PurchaseInvoiceNo == dto.PurchaseInvoiceNo)));
            if (duplicate)
                throw new GlobalException("BL, container hoặc invoice đã tồn tại ở một phiếu nhập khác.", HttpStatusCode.Conflict);

            InboundPurchase entity;
            if (dto.Id == 0)
            {
                entity = new InboundPurchase { CreatedAt = DateTime.UtcNow };
                _db.InboundPurchases.Add(entity);
            }
            else
            {
                entity = await _db.InboundPurchases.Include(x => x.Lines).FirstOrDefaultAsync(x => x.Id == dto.Id)
                    ?? throw new GlobalException("Không tìm thấy phiếu nhập.", HttpStatusCode.NotFound);
                var hasUsedLot = await _db.RawMaterialLots.AnyAsync(x => x.InboundPurchaseId == dto.Id && x.ProductionInputs.Any());
                if (hasUsedLot && dto.Lines.Any(x => x.Id == 0))
                    throw new GlobalException("Phiếu nhập đã đưa vào sản xuất, không được thêm dòng mới; hãy lập phiếu điều chỉnh.", HttpStatusCode.Conflict);
            }
            entity.CustomerId = dto.CustomerId;
            entity.SupplierPartnerId = dto.SupplierPartnerId;
            entity.VesselId = dto.VesselId;
            entity.CustomerCode = dto.CustomerCode;
            entity.ContractNo = dto.ContractNo;
            entity.Purchaser = dto.Purchaser;
            entity.MissingDocuments = dto.MissingDocuments;
            entity.Eta = dto.Eta;
            entity.WarehouseDate = dto.WarehouseDate;
            entity.WarehouseId = dto.WarehouseId;
            entity.TargetMarketId = dto.TargetMarketId;
            entity.PaymentTermId = dto.PaymentTermId;
            entity.EstimatePaymentDate = dto.EstimatePaymentDate;
            entity.BlNumber = dto.BlNumber;
            entity.PurchaseInvoiceNo = dto.PurchaseInvoiceNo;
            entity.ActualWeightKg = dto.ActualWeightKg;
            entity.ReceivedWeightKg = dto.ReceivedWeightKg;
            entity.ReleaseOrderFeeUsd = dto.ReleaseOrderFeeUsd;
            entity.ColdStorageFeeUsd = dto.ColdStorageFeeUsd;
            entity.HandlingFeeUsd = dto.HandlingFeeUsd;
            entity.CustomsFeeUsd = dto.CustomsFeeUsd;
            entity.InfrastructureFeeUsd = dto.InfrastructureFeeUsd;
            entity.ContainerNo = dto.ContainerNo;
            entity.ContainerType = dto.ContainerType;
            entity.Note = dto.Note;
            var incomingIds = dto.Lines.Where(x => x.Id > 0).Select(x => x.Id).ToHashSet();
            var lineInputs = new List<(InboundLine Entity, InboundLineDto Dto)>();
            foreach (var oldLine in entity.Lines.Where(x => !incomingIds.Contains(x.Id)).ToList())
            {
                if (await _db.ProductionInputs.AnyAsync(x => x.RawMaterialLot!.InboundLineId == oldLine.Id))
                    throw new GlobalException($"Dòng nhập {oldLine.Id} đã được sử dụng trong sản xuất và không thể xóa.", HttpStatusCode.Conflict);
                var oldRawLot = await _db.RawMaterialLots.FirstOrDefaultAsync(x => x.InboundLineId == oldLine.Id);
                if (oldRawLot != null) _db.RawMaterialLots.Remove(oldRawLot);
                _db.InboundLines.Remove(oldLine);
            }
            foreach (var lineDto in dto.Lines)
            {
                var line = lineDto.Id > 0 ? entity.Lines.FirstOrDefault(x => x.Id == lineDto.Id) : null;
                if (line == null)
                {
                    line = new InboundLine { Purchase = entity };
                    entity.Lines.Add(line);
                }
                line.Commodity = lineDto.Commodity;
                line.FishSpeciesCode = lineDto.FishSpeciesCode;
                line.FishFormCode = lineDto.FishFormCode;
                line.SizeCode = lineDto.SizeCode;
                line.SensoryCode = lineDto.SensoryCode;
                line.CatchMethodCode = lineDto.CatchMethodCode;
                line.FreezeMethodCode = lineDto.FreezeMethodCode;
                line.OriginCode = lineDto.OriginCode;
                line.QtyKgLongLine = lineDto.QtyKgLongLine;
                line.QtyKgHandline = lineDto.QtyKgHandline;
                line.QtyKgPs = lineDto.QtyKgPs;
                line.QtyKgLand = lineDto.QtyKgLand;
                line.MahiKg = lineDto.MahiKg;
                line.FinishedLbs = lineDto.FinishedLbs;
                line.QtyKg = lineDto.QtyKg > 0 ? lineDto.QtyKg : lineDto.QtyKgLongLine + lineDto.QtyKgHandline + lineDto.QtyKgPs + lineDto.QtyKgLand;
                line.ActualWeightKg = lineDto.ActualWeightKg > 0 ? lineDto.ActualWeightKg : line.QtyKg;
                line.PriceUsd = lineDto.PriceUsd;
                line.InvoiceAmountUsd = lineDto.InvoiceAmountUsd;
                line.ContainerQty = lineDto.ContainerQty;
                lineInputs.Add((line, lineDto));
            }
            await _db.SaveChangesAsync();
            foreach (var pair in lineInputs)
            {
                var line = pair.Entity;
                var input = pair.Dto;
                var raw = await _db.RawMaterialLots.FirstOrDefaultAsync(x => x.InboundLineId == line.Id);
                if (raw == null)
                {
                    raw = new RawMaterialLot { InboundPurchaseId = entity.Id, InboundLineId = line.Id };
                    _db.RawMaterialLots.Add(raw);
                }
                raw.LotNumber = string.IsNullOrWhiteSpace(input.RawMaterialLotNumber) ? raw.LotNumber : input.RawMaterialLotNumber.Trim();
                raw.LotNumber = string.IsNullOrWhiteSpace(raw.LotNumber) ? $"RM-{entity.Id:000000}-{line.Id:0000}" : raw.LotNumber;
                if (await _db.RawMaterialLots.AnyAsync(x => x.Id != raw.Id && x.LotNumber == raw.LotNumber))
                    throw new GlobalException($"Mã lô nguyên liệu {raw.LotNumber} đã tồn tại.", HttpStatusCode.Conflict);
                raw.SupplierPartnerId = entity.SupplierPartnerId;
                raw.VesselId = entity.VesselId;
                raw.ReceivedDate = entity.WarehouseDate ?? entity.CreatedAt;
                raw.WarehouseId = entity.WarehouseId;
                raw.TargetMarketId = entity.TargetMarketId;
                raw.FishSpeciesCode = line.FishSpeciesCode;
                raw.FishFormCode = line.FishFormCode;
                raw.SizeCode = line.SizeCode;
                raw.SensoryCode = line.SensoryCode;
                raw.CatchMethodCode = line.CatchMethodCode;
                raw.FreezeMethodCode = line.FreezeMethodCode;
                raw.OriginCode = line.OriginCode;
                raw.DeclaredKg = line.QtyKg;
                raw.ActualKg = line.ActualWeightKg;
            }
            await _db.SaveChangesAsync();
            var saved = await _db.InboundPurchases.Include(x => x.Customer).Include(x => x.SupplierPartner).Include(x => x.Vessel)
                .Include(x => x.Lines).ThenInclude(x => x.RawMaterialLot).FirstAsync(x => x.Id == entity.Id);
            var docs = await _db.DocumentAttachments.Include(x => x.DocumentType).Where(x => x.OwnerType == "InboundPurchase" && x.OwnerId == entity.Id).ToListAsync();
            return Map(saved, docs);
        }

        private static InboundPurchaseDto Map(InboundPurchase x, IEnumerable<DocumentAttachment>? docs = null) => new()
        {
            Id = x.Id,
            CustomerId = x.CustomerId,
            CustomerName = x.Customer?.Name,
            SupplierPartnerId = x.SupplierPartnerId,
            SupplierPartnerName = x.SupplierPartner?.Name,
            VesselId = x.VesselId,
            VesselName = x.Vessel?.Name,
            CustomerCode = x.CustomerCode,
            ContractNo = x.ContractNo,
            Purchaser = x.Purchaser,
            MissingDocuments = x.MissingDocuments,
            Eta = x.Eta,
            WarehouseDate = x.WarehouseDate,
            WarehouseId = x.WarehouseId,
            TargetMarketId = x.TargetMarketId,
            PaymentTermId = x.PaymentTermId,
            EstimatePaymentDate = x.EstimatePaymentDate,
            BlNumber = x.BlNumber,
            PurchaseInvoiceNo = x.PurchaseInvoiceNo,
            ActualWeightKg = x.ActualWeightKg,
            ReceivedWeightKg = x.ReceivedWeightKg,
            ReleaseOrderFeeUsd = x.ReleaseOrderFeeUsd,
            ColdStorageFeeUsd = x.ColdStorageFeeUsd,
            HandlingFeeUsd = x.HandlingFeeUsd,
            CustomsFeeUsd = x.CustomsFeeUsd,
            InfrastructureFeeUsd = x.InfrastructureFeeUsd,
            ContainerNo = x.ContainerNo,
            ContainerType = x.ContainerType,
            Note = x.Note,
            TotalKg = x.Lines.Sum(l => l.QtyKg > 0 ? l.QtyKg : l.QtyKgLongLine + l.QtyKgHandline + l.QtyKgPs + l.QtyKgLand),
            TotalAmountUsd = x.Lines.Sum(l => l.InvoiceAmountUsd),
            Documents = docs?.Select(MapDocument).ToList() ?? new(),
            Lines = x.Lines.Select(l => new InboundLineDto
            {
                Id = l.Id, Commodity = l.Commodity, QtyKgLongLine = l.QtyKgLongLine, QtyKgHandline = l.QtyKgHandline,
                QtyKgPs = l.QtyKgPs, QtyKgLand = l.QtyKgLand, PriceUsd = l.PriceUsd, InvoiceAmountUsd = l.InvoiceAmountUsd,
                ContainerQty = l.ContainerQty, FishSpeciesCode = l.FishSpeciesCode, FishFormCode = l.FishFormCode,
                SizeCode = l.SizeCode, SensoryCode = l.SensoryCode, CatchMethodCode = l.CatchMethodCode,
                FreezeMethodCode = l.FreezeMethodCode, OriginCode = l.OriginCode, QtyKg = l.QtyKg,
                ActualWeightKg = l.ActualWeightKg, RawMaterialLotId = l.RawMaterialLot?.Id,
                RawMaterialLotNumber = l.RawMaterialLot?.LotNumber
            }).ToList()
        };

        private static DocumentAttachmentDto MapDocument(DocumentAttachment x) => new()
        {
            Id = x.Id, OwnerType = x.OwnerType, OwnerId = x.OwnerId, DocumentTypeId = x.DocumentTypeId,
            DocumentTypeCode = x.DocumentType?.Code, DocumentTypeName = x.DocumentType?.Name, DocumentNo = x.DocumentNo,
            IssueDate = x.IssueDate, ExpiryDate = x.ExpiryDate, Status = x.Status, FileName = x.FileName,
            FileSize = x.FileSize, UploadedAt = x.UploadedAt, Note = x.Note
        };
    }

    public class ProductionService : ITransientDependency
    {
        private readonly DreamContext _db;
        public ProductionService(DreamContext db) => _db = db;

        public async Task<List<ProductionLotDto>> ListAsync()
        {
            var lots = await _db.ProductionLots.Include(x => x.Outputs).ThenInclude(o => o.Sku)
                .Include(x => x.Certificates).Include(x => x.Inputs).ThenInclude(i => i.RawMaterialLot)
                .OrderByDescending(x => x.Id).ToListAsync();
            var documents = await _db.DocumentAttachments.Include(x => x.DocumentType)
                .Where(x => x.OwnerType == "ProductionLot").ToListAsync();
            return lots.Select(x => Map(x, documents.Where(d => d.OwnerId == x.Id))).ToList();
        }

        public async Task<SeafoodPagedResult<ProductionLotDto>> PageAsync(string? search, int skip, int take)
        {
            var query = _db.ProductionLots.AsNoTracking();
            var term = search?.Trim();
            if (!string.IsNullOrWhiteSpace(term))
                query = query.Where(x => x.LotNumber.Contains(term) || (x.Note ?? "").Contains(term));
            var total = await query.CountAsync();
            var lots = await _db.ProductionLots.Include(x => x.Outputs).ThenInclude(o => o.Sku)
                .Include(x => x.Certificates).Include(x => x.Inputs).ThenInclude(i => i.RawMaterialLot)
                .Where(x => string.IsNullOrWhiteSpace(term) || x.LotNumber.Contains(term) || (x.Note ?? "").Contains(term))
                .OrderByDescending(x => x.Id).Skip(Math.Max(0, skip)).Take(Math.Clamp(take, 1, 100)).ToListAsync();
            var ids = lots.Select(x => x.Id).ToList();
            var documents = await _db.DocumentAttachments.Include(x => x.DocumentType)
                .Where(x => x.OwnerType == "ProductionLot" && ids.Contains(x.OwnerId)).ToListAsync();
            return new SeafoodPagedResult<ProductionLotDto> { Items = lots.Select(x => Map(x, documents.Where(d => d.OwnerId == x.Id))).ToList(), TotalCount = total };
        }

        public async Task<ProductionLotDto> SaveAsync(ProductionLotDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.LotNumber))
                throw new GlobalException("Số lot sản xuất là bắt buộc.", HttpStatusCode.BadRequest);
            if (dto.RawMaterialKg < 0 || dto.Outputs.Any(x => x.RecoveredKg < 0 || x.RawUsedKg < 0))
                throw new GlobalException("Khối lượng sản xuất không được âm.", HttpStatusCode.BadRequest);
            if (dto.Outputs.Count == 0)
                throw new GlobalException("Mẻ sản xuất phải có ít nhất một thành phẩm hoặc phụ phẩm.", HttpStatusCode.BadRequest);
            if (await _db.ProductionLots.AnyAsync(x => x.Id != dto.Id && x.LotNumber == dto.LotNumber))
                throw new GlobalException("Số lot sản xuất đã tồn tại.", HttpStatusCode.Conflict);

            await using var transaction = await _db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
            ProductionLot entity;
            var previousOutputBySku = new Dictionary<int, decimal>();
            if (dto.Id == 0)
            {
                entity = new ProductionLot();
                _db.ProductionLots.Add(entity);
            }
            else
            {
                entity = await _db.ProductionLots.Include(x => x.Outputs).Include(x => x.Certificates)
                    .Include(x => x.Inputs).FirstOrDefaultAsync(x => x.Id == dto.Id)
                    ?? throw new GlobalException("Không tìm thấy mẻ sản xuất.", HttpStatusCode.NotFound);
                previousOutputBySku = entity.Outputs.GroupBy(x => x.SkuId).ToDictionary(x => x.Key, x => x.Sum(y => y.RecoveredKg));
                var hasReservation = await _db.StockReservations.AnyAsync(x => x.InventoryBalance!.LotId == entity.Id && x.Status == ReservationStatus.Active);
                if (hasReservation)
                    throw new GlobalException("Mẻ sản xuất đã được giữ hàng cho đơn, không thể sửa thành phẩm.", HttpStatusCode.Conflict);
                _db.ProductionOutputs.RemoveRange(entity.Outputs);
                _db.LotCertificates.RemoveRange(entity.Certificates);
                _db.ProductionInputs.RemoveRange(entity.Inputs);
                entity.Inputs.Clear();
            }
            entity.LotNumber = dto.LotNumber;
            entity.InboundPurchaseId = dto.InboundPurchaseId;
            entity.ReceivedDate = dto.ReceivedDate == default ? DateTime.UtcNow : dto.ReceivedDate;
            entity.RawMaterialKg = dto.RawMaterialKg;
            entity.TargetMarketId = dto.TargetMarketId;
            entity.Note = dto.Note;
            entity.Outputs = dto.Outputs.Select(o => new ProductionOutput
            {
                SkuId = o.SkuId,
                RecoveredKg = o.RecoveredKg,
                RawUsedKg = o.RawUsedKg,
                YieldRatio = o.RawUsedKg == 0 ? 0 : decimal.Round(o.RecoveredKg / o.RawUsedKg, 4),
                ExportMarket = o.ExportMarket,
                CostUsd = o.CostUsd
            }).ToList();
            entity.Certificates = dto.CertificateCodes.Distinct().Select(c => new LotCertificate { CertificateCode = c }).ToList();
            var inputDtos = dto.Inputs;
            if (inputDtos.Count == 0 && dto.InboundPurchaseId is int purchaseId)
            {
                var legacyLots = await _db.RawMaterialLots.Where(x => x.InboundPurchaseId == purchaseId).OrderBy(x => x.ReceivedDate).ToListAsync();
                inputDtos = legacyLots.Take(1).Select(x => new ProductionInputDto { RawMaterialLotId = x.Id, QuantityKg = dto.RawMaterialKg }).ToList();
            }
            var inputIds = inputDtos.GroupBy(x => x.RawMaterialLotId).ToDictionary(x => x.Key, x => x.Sum(y => y.QuantityKg));
            if (inputIds.Any(x => x.Value <= 0))
                throw new GlobalException("Nguyên liệu đầu vào phải lớn hơn 0.", HttpStatusCode.BadRequest);
            var totalInputKg = inputIds.Values.Sum();
            if (entity.RawMaterialKg <= 0) entity.RawMaterialKg = totalInputKg;
            if (totalInputKg > entity.RawMaterialKg + 0.0001m)
                throw new GlobalException("Tổng nguyên liệu các lô đầu vào vượt khối lượng nguyên liệu của mẻ.", HttpStatusCode.BadRequest);
            foreach (var input in inputIds)
            {
                var raw = await _db.RawMaterialLots.Include(x => x.ProductionInputs).FirstOrDefaultAsync(x => x.Id == input.Key)
                    ?? throw new GlobalException($"Không tìm thấy lô nguyên liệu {input.Key}.", HttpStatusCode.NotFound);
                var alreadyConsumed = raw.ProductionInputs.Where(x => x.ProductionLotId != entity.Id).Sum(x => x.QuantityKg);
                if (alreadyConsumed + input.Value > raw.ActualKg + 0.0001m)
                    throw new GlobalException($"Lô nguyên liệu {raw.LotNumber} không đủ khả dụng (còn {raw.ActualKg - alreadyConsumed:N2} kg).", HttpStatusCode.Conflict);
                entity.Inputs.Add(new ProductionInput { RawMaterialLotId = raw.Id, QuantityKg = input.Value, Reason = inputDtos.First(x => x.RawMaterialLotId == raw.Id).Reason });
            }
            await _db.SaveChangesAsync();

            var newOutputBySku = entity.Outputs.GroupBy(x => x.SkuId).ToDictionary(x => x.Key, x => x.Sum(y => y.RecoveredKg));
            var allSkuIds = previousOutputBySku.Keys.Union(newOutputBySku.Keys).ToList();
            foreach (var skuId in allSkuIds)
            {
                var bal = await _db.InventoryBalances.FirstOrDefaultAsync(b => b.LotId == entity.Id && b.SkuId == skuId);
                var oldOutput = previousOutputBySku.GetValueOrDefault(skuId);
                var newOutput = newOutputBySku.GetValueOrDefault(skuId);
                if (bal == null)
                {
                    bal = new InventoryBalance
                    {
                        LotId = entity.Id, SkuId = skuId, OnHandKg = newOutput, AllocatedKg = 0, WarehouseId = null
                    };
                    _db.InventoryBalances.Add(bal);
                    await _db.SaveChangesAsync();
                }
                else
                {
                    var newOnHand = bal.OnHandKg - oldOutput + newOutput;
                    if (newOnHand + 0.0001m < bal.AllocatedKg)
                        throw new GlobalException($"Không thể giảm thành phẩm SKU {skuId} vì đã có hàng được giữ.", HttpStatusCode.Conflict);
                    bal.OnHandKg = newOnHand;
                }
                var delta = newOutput - oldOutput;
                if (Math.Abs(delta) > 0.0001m)
                    _db.InventoryMovements.Add(new InventoryMovement
                    {
                        InventoryBalanceId = bal.Id,
                        MovementType = delta >= 0 ? InventoryMovementType.ProductionOutput : InventoryMovementType.Adjustment,
                        QuantityKg = delta,
                        ReferenceType = "ProductionLot",
                        ReferenceId = entity.Id,
                        Reason = dto.Note
                    });
            }
            foreach (var input in entity.Inputs)
                _db.TraceabilityLinks.Add(new TraceabilityLink { FromType = "RawMaterialLot", FromId = input.RawMaterialLotId, ToType = "ProductionLot", ToId = entity.Id, QuantityKg = input.QuantityKg });
            _db.DomainAuditLogs.Add(new DomainAuditLog { EntityType = "ProductionLot", EntityId = entity.Id, Action = dto.Id == 0 ? "Created" : "Adjusted", Reason = dto.Note });
            await _db.SaveChangesAsync();
            await transaction.CommitAsync();
            var saved = await _db.ProductionLots.Include(x => x.Outputs).ThenInclude(o => o.Sku)
                .Include(x => x.Certificates).Include(x => x.Inputs).ThenInclude(i => i.RawMaterialLot).FirstAsync(x => x.Id == entity.Id);
            var docs = await _db.DocumentAttachments.Include(x => x.DocumentType).Where(x => x.OwnerType == "ProductionLot" && x.OwnerId == entity.Id).ToListAsync();
            return Map(saved, docs);
        }

        private static ProductionLotDto Map(ProductionLot x, IEnumerable<DocumentAttachment>? docs = null)
        {
            var recovered = x.Outputs.Sum(o => o.RecoveredKg);
            return new ProductionLotDto
            {
                Id = x.Id, LotNumber = x.LotNumber, InboundPurchaseId = x.InboundPurchaseId,
                ReceivedDate = x.ReceivedDate, RawMaterialKg = x.RawMaterialKg, TargetMarketId = x.TargetMarketId,
                RecoveryRatio = x.RawMaterialKg == 0 ? 0 : decimal.Round(recovered / x.RawMaterialKg, 4),
                Note = x.Note,
                CertificateCodes = x.Certificates.Select(c => c.CertificateCode).ToList(),
                Documents = docs?.Select(MapDocument).ToList() ?? new(),
                Inputs = x.Inputs.Select(i => new ProductionInputDto { RawMaterialLotId = i.RawMaterialLotId, RawMaterialLotNumber = i.RawMaterialLot?.LotNumber, QuantityKg = i.QuantityKg, Reason = i.Reason }).ToList(),
                Outputs = x.Outputs.Select(o => new ProductionOutputDto
                {
                    Id = o.Id, SkuId = o.SkuId, SkuName = o.Sku?.Name, RecoveredKg = o.RecoveredKg,
                    RawUsedKg = o.RawUsedKg, YieldRatio = o.YieldRatio, ExportMarket = o.ExportMarket, CostUsd = o.CostUsd
                }).ToList()
            };
        }

        private static DocumentAttachmentDto MapDocument(DocumentAttachment x) => new()
        {
            Id = x.Id, OwnerType = x.OwnerType, OwnerId = x.OwnerId, DocumentTypeId = x.DocumentTypeId,
            DocumentTypeCode = x.DocumentType?.Code, DocumentTypeName = x.DocumentType?.Name, DocumentNo = x.DocumentNo,
            IssueDate = x.IssueDate, ExpiryDate = x.ExpiryDate, Status = x.Status, FileName = x.FileName,
            FileSize = x.FileSize, UploadedAt = x.UploadedAt, Note = x.Note
        };
    }

    public class InventoryService : ITransientDependency
    {
        private readonly DreamContext _db;
        private readonly SeafoodDocumentService _documents;
        public InventoryService(DreamContext db, SeafoodDocumentService documents)
        {
            _db = db;
            _documents = documents;
        }

        public async Task<List<InventoryRowDto>> ListAsync(string? search = null, int skip = 0, int take = 100)
        {
            take = Math.Clamp(take, 1, 200);
            skip = Math.Max(0, skip);
            var query = _db.InventoryBalances
                .Include(x => x.Lot)!.ThenInclude(l => l!.Certificates)
                .Include(x => x.Lot)!.ThenInclude(l => l!.Inputs)
                .Include(x => x.Sku)
                .Include(x => x.Movements).AsQueryable();
            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(x => (x.Lot!.LotNumber ?? "").Contains(term) || (x.Sku!.Name ?? "").Contains(term));
            }
            var rows = await query.OrderBy(x => x.Lot!.ReceivedDate).Skip(skip).Take(take).ToListAsync();

            var openLines = await _db.SalesContractLines
                .Include(l => l.Contract)
                .Where(l => l.Contract!.Status != ContractStatus.Cancelled && l.Contract.Status != ContractStatus.Paid && l.Contract.Status != ContractStatus.Shipped)
                .ToListAsync();

            var needBySku = openLines.GroupBy(l => l.SkuId).ToDictionary(g => g.Key, g => g.Sum(x => x.QtyKg));
            var contracts = await _db.SalesContracts.Include(c => c.Lines).Include(c => c.Customer).ToListAsync();
            var markets = await _db.CatalogLookups.Where(l => l.Category == LookupCategory.Market).ToListAsync();
            var rules = await _db.MarketCertificateRules.ToListAsync();

            var result = new List<InventoryRowDto>();
            foreach (var row in rows)
            {
                var certs = row.Lot?.Certificates.Select(c => c.CertificateCode).ToList() ?? new List<string>();
                var warning = (string?)null;
                if (needBySku.TryGetValue(row.SkuId, out var need) && row.AvailableKg + 0.001m < need)
                {
                    warning = $"Thiếu hàng cho HĐ mở (cần {need:N0} kg, còn {row.AvailableKg:N0} kg)";
                }
                else if (needBySku.ContainsKey(row.SkuId))
                {
                    var related = contracts.Where(c => c.Lines.Any(l => l.SkuId == row.SkuId)).ToList();
                    var marketId = related.Select(c => c.MarketId).FirstOrDefault();
                    var market = markets.FirstOrDefault(m => m.Id == marketId);
                    var rule = market == null ? null : rules.FirstOrDefault(r => r.MarketCode == market.Code);
                    var owners = new List<(string OwnerType, int OwnerId)> { ("ProductionLot", row.LotId) };
                    owners.AddRange(row.Lot?.Inputs.Select(x => ("RawMaterialLot", x.RawMaterialLotId)) ?? []);
                    var documentStatus = market == null ? (Required: new List<string>(), Missing: new List<string>(), Optional: new List<string>())
                        : await _documents.GetRequiredDocumentStatusAsync(owners, market.Code, DateTime.UtcNow);
                    if (documentStatus.Missing.Count > 0)
                    {
                        warning = $"Đủ hàng nhưng thiếu chứng từ {string.Join(", ", documentStatus.Missing)} cho thị trường {market?.Name}.";
                    }
                    else if (rule?.MustHaveCertificateCode != null && !certs.Contains(rule.MustHaveCertificateCode) && documentStatus.Required.Count == 0)
                    {
                        warning = $"Đủ hàng nhưng chưa xác nhận chứng từ {rule.MustHaveCertificateCode} cho thị trường {market?.Name}.";
                    }
                }
                result.Add(new InventoryRowDto
                {
                    Id = row.Id, LotId = row.LotId, LotNumber = row.Lot?.LotNumber ?? "",
                    SkuId = row.SkuId, SkuName = row.Sku?.Name ?? "",
                    OnHandKg = row.OnHandKg, AllocatedKg = row.AllocatedKg, AvailableKg = row.AvailableKg,
                    WarehouseId = row.WarehouseId, Certificates = certs, Warning = warning,
                    Movements = row.Movements.OrderByDescending(x => x.OccurredAt).Take(25).Select(x => new InventoryMovementDto
                    {
                        Id = x.Id, MovementType = x.MovementType, QuantityKg = x.QuantityKg,
                        ReferenceType = x.ReferenceType, ReferenceId = x.ReferenceId, OccurredAt = x.OccurredAt, Reason = x.Reason
                    }).ToList()
                });
            }
            return result;
        }

        public async Task<SeafoodPagedResult<InventoryRowDto>> PageAsync(string? search, int skip, int take)
        {
            var query = _db.InventoryBalances.AsNoTracking().Include(x => x.Lot).Include(x => x.Sku).AsQueryable();
            var term = search?.Trim();
            if (!string.IsNullOrWhiteSpace(term))
                query = query.Where(x => (x.Lot!.LotNumber ?? "").Contains(term) || (x.Sku!.Name ?? "").Contains(term));
            var total = await query.CountAsync();
            var items = await ListAsync(search, skip, take);
            return new SeafoodPagedResult<InventoryRowDto> { Items = items, TotalCount = total };
        }

        public async Task<InventoryRowDto> AdjustAsync(InventoryAdjustmentDto dto, int? userId)
        {
            if (dto.InventoryBalanceId <= 0 || dto.QuantityKg == 0 || string.IsNullOrWhiteSpace(dto.Reason))
                throw new GlobalException("Điều chỉnh kho phải có dòng tồn, số lượng khác 0 và lý do.", HttpStatusCode.BadRequest);
            await using var transaction = await _db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
            var balance = await _db.InventoryBalances.Include(x => x.Lot).Include(x => x.Sku).Include(x => x.Movements)
                .FirstOrDefaultAsync(x => x.Id == dto.InventoryBalanceId)
                ?? throw new GlobalException("Không tìm thấy dòng tồn kho.", HttpStatusCode.NotFound);
            if (balance.OnHandKg + dto.QuantityKg + 0.0001m < balance.AllocatedKg)
                throw new GlobalException("Không thể giảm tồn thấp hơn lượng đã giữ cho đơn.", HttpStatusCode.Conflict);
            balance.OnHandKg += dto.QuantityKg;
            _db.InventoryMovements.Add(new InventoryMovement
            {
                InventoryBalanceId = balance.Id,
                MovementType = InventoryMovementType.Adjustment,
                QuantityKg = dto.QuantityKg,
                ReferenceType = "InventoryAdjustment",
                ReferenceId = balance.Id,
                CreatedByUserId = userId,
                Reason = dto.Reason.Trim()
            });
            _db.DomainAuditLogs.Add(new DomainAuditLog { EntityType = "InventoryBalance", EntityId = balance.Id, Action = "Adjusted", UserId = userId, Reason = dto.Reason.Trim() });
            await _db.SaveChangesAsync();
            await transaction.CommitAsync();
            return new InventoryRowDto
            {
                Id = balance.Id, LotId = balance.LotId, LotNumber = balance.Lot?.LotNumber ?? "", SkuId = balance.SkuId,
                SkuName = balance.Sku?.Name ?? "", OnHandKg = balance.OnHandKg, AllocatedKg = balance.AllocatedKg,
                AvailableKg = balance.AvailableKg, WarehouseId = balance.WarehouseId,
                Movements = balance.Movements.OrderByDescending(x => x.OccurredAt).Take(25).Select(x => new InventoryMovementDto
                {
                    Id = x.Id, MovementType = x.MovementType, QuantityKg = x.QuantityKg, ReferenceType = x.ReferenceType,
                    ReferenceId = x.ReferenceId, OccurredAt = x.OccurredAt, Reason = x.Reason
                }).ToList()
            };
        }
    }

    public class ExportService : ITransientDependency
    {
        private readonly DreamContext _db;
        private readonly SeafoodDocumentService _documents;
        public ExportService(DreamContext db, SeafoodDocumentService documents)
        {
            _db = db;
            _documents = documents;
        }

        public async Task<PriceSuggestionDto> SuggestPriceAsync(int customerId, int skuId)
        {
            var last = await _db.SalesContractLines
                .Include(l => l.Contract)
                .Where(l => l.SkuId == skuId && l.Contract!.CustomerId == customerId && l.Contract.Status != ContractStatus.Cancelled)
                .OrderByDescending(l => l.Contract!.CreatedAt)
                .FirstOrDefaultAsync();
            var since = DateTime.UtcNow.AddMonths(-2);
            var peers = await _db.SalesContractLines
                .Include(l => l.Contract)
                .Where(l => l.SkuId == skuId && l.Contract!.CustomerId != customerId && l.Contract.CreatedAt >= since && l.Contract.Status != ContractStatus.Cancelled)
                .Select(l => l.UnitPriceUsd)
                .ToListAsync();
            return new PriceSuggestionDto
            {
                LastPriceForCustomer = last?.UnitPriceUsd,
                LastContractNo = last?.Contract?.ContractNo,
                PeerAvgLastTwoMonths = peers.Count == 0 ? null : decimal.Round(peers.Average(), 2)
            };
        }

        public async Task<List<SalesContractDto>> ListContractsAsync()
        {
            var rows = await _db.SalesContracts.Include(x => x.Customer).Include(x => x.Lines).ThenInclude(l => l.Sku)
                .OrderByDescending(x => x.Id).ToListAsync();
            return rows.Select(MapContract).ToList();
        }

        public async Task<SeafoodPagedResult<SalesContractDto>> PageContractsAsync(string? search, int skip, int take)
        {
            var query = _db.SalesContracts.AsNoTracking().Include(x => x.Customer).Include(x => x.Lines).ThenInclude(l => l.Sku).AsQueryable();
            var term = search?.Trim();
            if (!string.IsNullOrWhiteSpace(term))
                query = query.Where(x => x.ContractNo.Contains(term) || (x.Customer != null && x.Customer.Name.Contains(term))
                    || x.Lines.Any(l => l.Sku != null && l.Sku.Name.Contains(term)));
            var total = await query.CountAsync();
            var rows = await query.OrderByDescending(x => x.Id).Skip(Math.Max(0, skip)).Take(Math.Clamp(take, 1, 100)).ToListAsync();
            return new SeafoodPagedResult<SalesContractDto> { Items = rows.Select(MapContract).ToList(), TotalCount = total };
        }

        public async Task<SalesContractDto> SaveContractAsync(SalesContractDto dto)
        {
            if (dto.CustomerId <= 0 || dto.Lines.Count == 0 || dto.Lines.Any(x => x.QtyKg <= 0 && x.QtyLbs <= 0 || x.UnitPriceUsd < 0))
                throw new GlobalException("Hợp đồng phải có khách hàng, dòng hàng và số lượng hợp lệ.", HttpStatusCode.BadRequest);
            if (!await _db.Customers.AnyAsync(x => x.Id == dto.CustomerId && x.IsActive))
                throw new GlobalException("Khách hàng không tồn tại hoặc đã khóa.", HttpStatusCode.BadRequest);
            var skuIds = dto.Lines.Select(x => x.SkuId).Distinct().ToList();
            if (skuIds.Count == 0 || await _db.ProductSkus.CountAsync(x => skuIds.Contains(x.Id) && x.IsActive) != skuIds.Count)
                throw new GlobalException("Hợp đồng có SKU không tồn tại hoặc đã khóa.", HttpStatusCode.BadRequest);
            if (await _db.SalesContracts.AnyAsync(x => x.Id != dto.Id && x.ContractNo == dto.ContractNo && !string.IsNullOrWhiteSpace(dto.ContractNo)))
                throw new GlobalException("Số hợp đồng đã tồn tại.", HttpStatusCode.Conflict);
            SalesContract entity;
            if (dto.Id == 0)
            {
                entity = new SalesContract { CreatedAt = DateTime.UtcNow, Status = ContractStatus.Draft };
                _db.SalesContracts.Add(entity);
            }
            else
            {
                entity = await _db.SalesContracts.Include(x => x.Lines).FirstAsync(x => x.Id == dto.Id);
                if (await _db.StockReservations.AnyAsync(x => x.SalesContractLine!.ContractId == entity.Id && x.Status == ReservationStatus.Active))
                    throw new GlobalException("Hợp đồng đã có hàng được giữ, không thể thay đổi dòng hàng.", HttpStatusCode.Conflict);
                if (await _db.SalesAllocations.AnyAsync(x => x.SalesContractLine!.ContractId == entity.Id))
                    throw new GlobalException("Hợp đồng đã có lịch sử phân bổ/xuất, không thể thay đổi dòng hàng.", HttpStatusCode.Conflict);
                _db.SalesContractLines.RemoveRange(entity.Lines);
            }
            entity.ContractNo = string.IsNullOrWhiteSpace(dto.ContractNo) ? $"SC-{DateTime.UtcNow:yyMMddHHmm}" : dto.ContractNo;
            entity.CustomerContractNo = dto.CustomerContractNo;
            entity.CustomerId = dto.CustomerId;
            entity.MarketId = dto.MarketId;
            entity.Lines = dto.Lines.Select(l =>
            {
                var kg = l.QtyKg > 0 ? l.QtyKg : WeightUnits.LbsToKg(l.QtyLbs);
                var lbs = l.QtyLbs > 0 ? l.QtyLbs : WeightUnits.KgToLbs(l.QtyKg);
                return new SalesContractLine
                {
                    SkuId = l.SkuId, QtyKg = kg, QtyLbs = lbs,
                    UnitPriceUsd = l.UnitPriceUsd, AmountUsd = decimal.Round(kg * l.UnitPriceUsd, 2)
                };
            }).ToList();

            var firstLine = entity.Lines.FirstOrDefault();
            if (firstLine != null)
            {
                var suggestion = await SuggestPriceAsync(entity.CustomerId, firstLine.SkuId);
                entity.SuggestedUnitPriceUsd = suggestion.LastPriceForCustomer ?? firstLine.UnitPriceUsd;
                if (suggestion.LastPriceForCustomer is decimal last && last != 0)
                    entity.VarianceVsLastPct = decimal.Round((firstLine.UnitPriceUsd - last) / last * 100, 2);
                if (suggestion.PeerAvgLastTwoMonths is decimal peer && peer != 0)
                    entity.VarianceVsPeersPct = decimal.Round((firstLine.UnitPriceUsd - peer) / peer * 100, 2);
            }
            await _db.SaveChangesAsync();
            return MapContract(await _db.SalesContracts.Include(x => x.Customer).Include(x => x.Lines).ThenInclude(l => l.Sku)
                .FirstAsync(x => x.Id == entity.Id));
        }

        public async Task<SalesContractDto> ApproveAsync(int id, string? note, int? userId)
        {
            var entity = await _db.SalesContracts.Include(x => x.Customer).Include(x => x.Lines).ThenInclude(l => l.Sku)
                .FirstAsync(x => x.Id == id);
            if (entity.Status is not ContractStatus.Draft and not ContractStatus.PendingApproval)
                throw new GlobalException("Chỉ hợp đồng Draft hoặc PendingApproval mới được duyệt.", HttpStatusCode.Conflict);
            if (entity.Lines.Count == 0)
                throw new GlobalException("Hợp đồng chưa có dòng hàng.", HttpStatusCode.BadRequest);
            entity.Status = ContractStatus.Signed;
            entity.ApprovalNote = note;
            entity.ApprovedBy = userId;
            entity.ApprovedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return MapContract(entity);
        }

        public async Task<List<ExportShipmentDto>> ListShipmentsAsync()
        {
            var rows = await _db.ExportShipments.Include(x => x.Customer).Include(x => x.PaymentTerm)
                .Include(x => x.Containers).Include(x => x.Documents).ThenInclude(x => x.DocumentAttachment)
                .OrderByDescending(x => x.Id).ToListAsync();
            var docs = await _db.DocumentAttachments.Include(x => x.DocumentType).Where(x => x.OwnerType == "Shipment").ToListAsync();
            return rows.Select(x => MapShip(x, docs.Where(d => d.OwnerId == x.Id))).ToList();
        }

        public async Task<SeafoodPagedResult<ExportShipmentDto>> PageShipmentsAsync(string? search, int skip, int take)
        {
            var query = _db.ExportShipments.AsNoTracking().Include(x => x.Customer).Include(x => x.PaymentTerm).AsQueryable();
            var term = search?.Trim();
            if (!string.IsNullOrWhiteSpace(term))
                query = query.Where(x => (x.InvoiceNo ?? "").Contains(term) || (x.ProductionNoticeNo ?? "").Contains(term)
                    || (x.ContainerNo ?? "").Contains(term) || (x.Route ?? "").Contains(term)
                    || (x.Customer != null && x.Customer.Name.Contains(term)) || (x.PaymentTerm != null && x.PaymentTerm.Name.Contains(term)));
            var total = await query.CountAsync();
            var rows = await _db.ExportShipments.Include(x => x.Customer).Include(x => x.PaymentTerm).Include(x => x.Containers)
                .Where(x => string.IsNullOrWhiteSpace(term) || (x.InvoiceNo ?? "").Contains(term) || (x.ProductionNoticeNo ?? "").Contains(term)
                    || (x.ContainerNo ?? "").Contains(term) || (x.Route ?? "").Contains(term)
                    || (x.Customer != null && x.Customer.Name.Contains(term)) || (x.PaymentTerm != null && x.PaymentTerm.Name.Contains(term)))
                .OrderByDescending(x => x.Id).Skip(Math.Max(0, skip)).Take(Math.Clamp(take, 1, 100)).ToListAsync();
            var ids = rows.Select(x => x.Id).ToList();
            var docs = await _db.DocumentAttachments.Include(x => x.DocumentType).Where(x => x.OwnerType == "Shipment" && ids.Contains(x.OwnerId)).ToListAsync();
            return new SeafoodPagedResult<ExportShipmentDto> { Items = rows.Select(x => MapShip(x, docs.Where(d => d.OwnerId == x.Id))).ToList(), TotalCount = total };
        }

        public async Task<ExportShipmentDto> SaveShipmentAsync(ExportShipmentDto dto)
        {
            if (dto.QtyKg <= 0 && dto.QtyLbs <= 0)
                throw new GlobalException("Shipment phải có khối lượng lớn hơn 0.", HttpStatusCode.BadRequest);
            if (dto.Containers.GroupBy(x => x.ContainerNo.Trim(), StringComparer.OrdinalIgnoreCase).Any(x => string.IsNullOrWhiteSpace(x.Key) || x.Count() > 1))
                throw new GlobalException("Số container không được trống hoặc trùng trong cùng shipment.", HttpStatusCode.BadRequest);
            if (dto.SalesContractId is not int contractId)
                throw new GlobalException("Shipment phải gắn với hợp đồng.", HttpStatusCode.BadRequest);
            var contract = await _db.SalesContracts.FirstOrDefaultAsync(x => x.Id == contractId)
                ?? throw new GlobalException("Không tìm thấy hợp đồng của shipment.", HttpStatusCode.NotFound);
            var customerId = dto.CustomerId ?? contract.CustomerId;
            if (dto.CustomerId is int shipmentCustomerId && shipmentCustomerId != contract.CustomerId)
                throw new GlobalException("Khách hàng shipment không khớp với hợp đồng.", HttpStatusCode.BadRequest);
            if (dto.PaymentTermId is int paymentTermId && !await _db.PaymentTerms.AnyAsync(x => x.Id == paymentTermId && x.IsActive))
                throw new GlobalException("Điều khoản thanh toán không tồn tại hoặc đã khóa.", HttpStatusCode.BadRequest);
            if (!string.IsNullOrWhiteSpace(dto.InvoiceNo) && await _db.SalesInvoices.AnyAsync(x => x.InvoiceNo == dto.InvoiceNo.Trim() && x.ShipmentId != dto.Id))
                throw new GlobalException("Số invoice đã tồn tại.", HttpStatusCode.Conflict);

            ExportShipment entity;
            if (dto.Id == 0)
            {
                entity = new ExportShipment();
                _db.ExportShipments.Add(entity);
            }
            else
            {
                entity = await _db.ExportShipments.Include(x => x.Containers).FirstOrDefaultAsync(x => x.Id == dto.Id)
                    ?? throw new GlobalException("Không tìm thấy shipment.", HttpStatusCode.NotFound);
                if (entity.Status == ShipmentStatus.Shipped)
                    throw new GlobalException("Shipment đã xuất hàng, không được sửa.", HttpStatusCode.Conflict);
                _db.ShipmentContainers.RemoveRange(entity.Containers);
                entity.Containers.Clear();
            }
            entity.SalesContractId = contractId;
            entity.CustomerId = customerId;
            entity.ProductionNoticeNo = dto.ProductionNoticeNo;
            entity.PackingDate = dto.PackingDate;
            entity.Etd = dto.Etd;
            entity.Eta = dto.Eta;
            entity.InvoiceNo = dto.InvoiceNo;
            entity.PaymentTermId = dto.PaymentTermId;
            entity.Cartons = dto.Cartons;
            entity.QtyKg = dto.QtyKg > 0 ? dto.QtyKg : WeightUnits.LbsToKg(dto.QtyLbs);
            entity.QtyLbs = dto.QtyLbs > 0 ? dto.QtyLbs : WeightUnits.KgToLbs(dto.QtyKg);
            entity.AmountUsd = dto.AmountUsd;
            entity.ContainerNo = dto.ContainerNo;
            entity.ContainerType = dto.ContainerType;
            entity.Route = dto.Route;
            entity.Status = dto.Status == ShipmentStatus.Shipped ? ShipmentStatus.Draft : dto.Status;
            await _db.SaveChangesAsync();

            var containers = dto.Containers.Count > 0 ? dto.Containers : new List<ShipmentContainerDto>
            {
                new() { ContainerNo = dto.ContainerNo ?? string.Empty, ContainerType = dto.ContainerType, Cartons = dto.Cartons, QtyKg = entity.QtyKg, QtyLbs = entity.QtyLbs }
            };
            foreach (var container in containers)
            {
                if (string.IsNullOrWhiteSpace(container.ContainerNo)) continue;
                entity.Containers.Add(new ShipmentContainer
                {
                    ShipmentId = entity.Id, ContainerNo = container.ContainerNo.Trim(), ContainerType = container.ContainerType,
                    SealNo = container.SealNo, Cartons = container.Cartons, QtyKg = container.QtyKg > 0 ? container.QtyKg : WeightUnits.LbsToKg(container.QtyLbs),
                    QtyLbs = container.QtyLbs > 0 ? container.QtyLbs : WeightUnits.KgToLbs(container.QtyKg), LoadingDate = container.LoadingDate,
                    IsConfirmed = container.IsConfirmed
                });
            }

            SalesInvoice? invoice = null;
            if (!string.IsNullOrWhiteSpace(dto.InvoiceNo) && dto.Id == 0)
            {
                invoice = new SalesInvoice
                {
                    InvoiceNo = dto.InvoiceNo.Trim(), SalesContractId = contractId, ShipmentId = entity.Id,
                    CustomerId = customerId, AmountUsd = entity.AmountUsd, IssueDate = DateTime.UtcNow, Status = InvoiceStatus.Issued
                };
                _db.SalesInvoices.Add(invoice);
            }

            if (dto.PaymentTermId is int termId && dto.Id == 0)
            {
                var term = await _db.PaymentTerms.FindAsync(termId);
                var ratios = JsonConvert.DeserializeObject<List<decimal>>(term?.RatiosJson ?? "[]") ?? new List<decimal>();
                if (ratios.Count > 0 && Math.Abs(ratios.Sum() - 1m) > 0.0001m)
                    throw new GlobalException("Tổng tỷ lệ thanh toán phải bằng 100%.", HttpStatusCode.BadRequest);
                var seq = 1;
                var running = 0m;
                foreach (var ratio in ratios)
                {
                    var installmentAmount = seq == ratios.Count
                        ? decimal.Round(entity.AmountUsd - running, 2)
                        : decimal.Round(entity.AmountUsd * ratio, 2);
                    running += installmentAmount;
                    _db.PaymentInstallments.Add(new PaymentInstallment
                    {
                        ExportShipmentId = entity.Id,
                        Sequence = seq++,
                        Ratio = ratio,
                        AmountUsd = installmentAmount,
                        DueDate = term?.DueDays is int days ? (entity.Etd ?? DateTime.UtcNow).AddDays(days) : entity.Eta,
                        SalesInvoice = invoice
                    });
                }
            }
            await _db.SaveChangesAsync();
            return MapShip(await _db.ExportShipments.Include(x => x.Customer).Include(x => x.PaymentTerm).Include(x => x.Containers).FirstAsync(x => x.Id == entity.Id));
        }

        public async Task<ExportShipmentDto> ConfirmShipmentAsync(int id, int? userId)
        {
            await using var transaction = await _db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
            var entity = await _db.ExportShipments.Include(x => x.SalesContract).ThenInclude(x => x!.Lines)
                .Include(x => x.Containers).FirstOrDefaultAsync(x => x.Id == id)
                ?? throw new GlobalException("Không tìm thấy shipment.", HttpStatusCode.NotFound);
            if (entity.Status == ShipmentStatus.Shipped)
                return MapShip(entity);
            if (entity.SalesContractId is not int salesContractId)
                throw new GlobalException("Shipment phải gắn với hợp đồng trước khi xác nhận xuất.", HttpStatusCode.BadRequest);
            if (entity.Containers.Count == 0 || entity.Containers.Any(x => string.IsNullOrWhiteSpace(x.ContainerNo)))
                throw new GlobalException("Shipment phải có ít nhất một container hợp lệ.", HttpStatusCode.BadRequest);
            if (entity.Containers.Sum(x => x.QtyKg) + 0.0001m < entity.QtyKg)
                throw new GlobalException("Tổng khối lượng container nhỏ hơn khối lượng shipment.", HttpStatusCode.BadRequest);
            if (string.IsNullOrWhiteSpace(entity.InvoiceNo) || entity.AmountUsd <= 0)
                throw new GlobalException("Shipment phải có invoice và giá trị invoice hợp lệ.", HttpStatusCode.BadRequest);
            if (entity.SalesContract != null && entity.CustomerId != entity.SalesContract.CustomerId)
                throw new GlobalException("Khách hàng shipment không khớp với khách hàng của hợp đồng.", HttpStatusCode.BadRequest);

            var allocations = await _db.SalesAllocations
                .Include(x => x.InventoryBalance)!.ThenInclude(x => x!.Lot)!.ThenInclude(x => x!.Inputs)
                .Include(x => x.SalesContractLine)
                .Where(x => x.SalesContractLine!.ContractId == salesContractId)
                .Where(x => x.ShipmentId == null && x.IsActive).OrderBy(x => x.InventoryBalance!.Lot!.ReceivedDate).ToListAsync();
            if (allocations.Sum(x => x.QuantityKg) + 0.0001m < entity.QtyKg)
                throw new GlobalException("Chưa xếp đủ lô cho shipment. Hãy preview và xác nhận phân bổ trước.", HttpStatusCode.Conflict);

            var marketCode = entity.SalesContract?.MarketId is int marketId
                ? await _db.CatalogLookups.Where(x => x.Id == marketId).Select(x => x.Code).FirstOrDefaultAsync() ?? "OTHER"
                : "OTHER";
            var effectiveDate = entity.Etd ?? DateTime.UtcNow;
            var selected = new List<(SalesAllocation Allocation, decimal QuantityKg)>();
            var remaining = entity.QtyKg;
            foreach (var allocation in allocations)
            {
                var owners = new List<(string OwnerType, int OwnerId)> { ("ProductionLot", allocation.InventoryBalance!.LotId) };
                owners.AddRange(allocation.InventoryBalance.Lot!.Inputs.Select(x => ("RawMaterialLot", x.RawMaterialLotId)));
                var status = await _documents.GetRequiredDocumentStatusAsync(owners, marketCode, effectiveDate);
                if (status.Missing.Count > 0)
                    throw new GlobalException($"Lô {allocation.InventoryBalance.Lot.LotNumber} thiếu giấy: {string.Join(", ", status.Missing)}.", HttpStatusCode.BadRequest);
                var quantity = Math.Min(allocation.QuantityKg, remaining);
                if (quantity <= 0.0001m) continue;
                selected.Add((allocation, quantity));
                remaining -= quantity;
                if (remaining <= 0.0001m) break;
            }
            if (remaining > 0.0001m)
                throw new GlobalException("Không thể chọn đủ phân bổ có chứng từ hợp lệ cho shipment.", HttpStatusCode.BadRequest);

            foreach (var (allocation, quantity) in selected)
            {
                var balance = allocation.InventoryBalance!;
                if (balance.OnHandKg < quantity || balance.AllocatedKg < quantity)
                    throw new GlobalException($"Tồn kho lô {balance.Lot?.LotNumber} đã thay đổi, vui lòng preview lại.", HttpStatusCode.Conflict);
                balance.OnHandKg -= quantity;
                balance.AllocatedKg -= quantity;
                if (allocation.QuantityKg <= quantity + 0.0001m)
                {
                    allocation.ShipmentId = entity.Id;
                }
                else
                {
                    allocation.QuantityKg -= quantity;
                    _db.SalesAllocations.Add(new SalesAllocation
                    {
                        SalesContractLineId = allocation.SalesContractLineId,
                        InventoryBalanceId = allocation.InventoryBalanceId,
                        ShipmentId = entity.Id,
                        QuantityKg = quantity
                    });
                }
                _db.InventoryMovements.Add(new InventoryMovement
                {
                    InventoryBalanceId = balance.Id, MovementType = InventoryMovementType.Shipment,
                    QuantityKg = -quantity, ReferenceType = "Shipment", ReferenceId = entity.Id
                });
                var reservation = await _db.StockReservations.Where(x => x.Status == ReservationStatus.Active
                    && x.SalesContractLineId == allocation.SalesContractLineId && x.InventoryBalanceId == allocation.InventoryBalanceId
                    && x.ShipmentId == null).OrderBy(x => x.Id).FirstOrDefaultAsync();
                if (reservation == null || reservation.QuantityKg + 0.0001m < quantity)
                    throw new GlobalException("Không tìm thấy reservation đủ để đối chiếu phân bổ shipment.", HttpStatusCode.Conflict);
                if (reservation.QuantityKg <= quantity + 0.0001m)
                {
                    reservation.Status = ReservationStatus.Fulfilled;
                    reservation.ShipmentId = entity.Id;
                    reservation.ReleasedAt = DateTime.UtcNow;
                }
                else
                {
                    reservation.QuantityKg -= quantity;
                    _db.StockReservations.Add(new StockReservation
                    {
                        SalesContractLineId = reservation.SalesContractLineId,
                        InventoryBalanceId = reservation.InventoryBalanceId,
                        ShipmentId = entity.Id,
                        QuantityKg = quantity,
                        Status = ReservationStatus.Fulfilled,
                        CreatedByUserId = userId,
                        ReleasedAt = DateTime.UtcNow,
                        Reason = "Partial shipment"
                    });
                }
            }
            entity.Status = ShipmentStatus.Shipped;
            if (entity.SalesContractId is int contractId)
            {
                var contract = await _db.SalesContracts.Include(x => x.Lines).FirstAsync(x => x.Id == contractId);
                var shipped = await _db.SalesAllocations.Where(x => x.SalesContractLine!.ContractId == contractId && x.ShipmentId != null).SumAsync(x => (decimal?)x.QuantityKg) ?? 0;
                contract.Status = shipped + 0.0001m >= contract.Lines.Sum(x => x.QtyKg) ? ContractStatus.Shipped : ContractStatus.ReadyDocs;
            }
            _db.DomainAuditLogs.Add(new DomainAuditLog { EntityType = "ExportShipment", EntityId = entity.Id, Action = "Confirmed", UserId = userId });
            await _db.SaveChangesAsync();
            await transaction.CommitAsync();
            return MapShip(await _db.ExportShipments.Include(x => x.Customer).Include(x => x.PaymentTerm).Include(x => x.Containers).FirstAsync(x => x.Id == entity.Id));
        }

        private static SalesContractDto MapContract(SalesContract x) => new()
        {
            Id = x.Id, ContractNo = x.ContractNo, CustomerContractNo = x.CustomerContractNo,
            CustomerId = x.CustomerId, CustomerName = x.Customer?.Name, MarketId = x.MarketId,
            Status = x.Status, SuggestedUnitPriceUsd = x.SuggestedUnitPriceUsd,
            VarianceVsLastPct = x.VarianceVsLastPct, VarianceVsPeersPct = x.VarianceVsPeersPct,
            ApprovalNote = x.ApprovalNote,
            AllocatedKg = x.Lines.Sum(l => l.AllocatedKg),
            Lines = x.Lines.Select(l => new SalesContractLineDto
            {
                SkuId = l.SkuId, SkuName = l.Sku?.Name, QtyKg = l.QtyKg, QtyLbs = l.QtyLbs,
                UnitPriceUsd = l.UnitPriceUsd, AmountUsd = l.AmountUsd, AllocatedKg = l.AllocatedKg
            }).ToList()
        };

        private static ExportShipmentDto MapShip(ExportShipment x, IEnumerable<DocumentAttachment>? docs = null) => new()
        {
            Id = x.Id, SalesContractId = x.SalesContractId, CustomerId = x.CustomerId,
            CustomerName = x.Customer?.Name, ProductionNoticeNo = x.ProductionNoticeNo,
            PackingDate = x.PackingDate, Etd = x.Etd, Eta = x.Eta, InvoiceNo = x.InvoiceNo,
            PaymentTermId = x.PaymentTermId, PaymentTermName = x.PaymentTerm?.Name,
            Cartons = x.Cartons, QtyLbs = x.QtyLbs, QtyKg = x.QtyKg, AmountUsd = x.AmountUsd,
            ContainerNo = x.ContainerNo, ContainerType = x.ContainerType, Route = x.Route, Status = x.Status,
            Containers = x.Containers.Select(c => new ShipmentContainerDto { Id = c.Id, ContainerNo = c.ContainerNo, ContainerType = c.ContainerType, SealNo = c.SealNo, Cartons = c.Cartons, QtyKg = c.QtyKg, QtyLbs = c.QtyLbs, LoadingDate = c.LoadingDate, IsConfirmed = c.IsConfirmed }).ToList(),
            Documents = docs?.Select(InboundDocumentMap).ToList() ?? new()
        };

        private static DocumentAttachmentDto InboundDocumentMap(DocumentAttachment x) => new()
        {
            Id = x.Id, OwnerType = x.OwnerType, OwnerId = x.OwnerId, DocumentTypeId = x.DocumentTypeId,
            DocumentTypeCode = x.DocumentType?.Code, DocumentTypeName = x.DocumentType?.Name, DocumentNo = x.DocumentNo,
            IssueDate = x.IssueDate, ExpiryDate = x.ExpiryDate, Status = x.Status, FileName = x.FileName,
            FileSize = x.FileSize, UploadedAt = x.UploadedAt, Note = x.Note
        };
    }

    public class FinanceService : ITransientDependency
    {
        private readonly DreamContext _db;
        public FinanceService(DreamContext db) => _db = db;

        public async Task<List<PaymentInstallmentDto>> ListPaymentsAsync()
        {
            var rows = await _db.PaymentInstallments.Include(x => x.ExportShipment)!.ThenInclude(s => s!.Customer)
                .OrderByDescending(x => x.Id).ToListAsync();
            return rows.Select(x => new PaymentInstallmentDto
            {
                Id = x.Id, ExportShipmentId = x.ExportShipmentId, InvoiceNo = x.ExportShipment?.InvoiceNo,
                CustomerName = x.ExportShipment?.Customer?.Name, Sequence = x.Sequence, Ratio = x.Ratio,
                AmountUsd = x.AmountUsd, DueDate = x.DueDate, ReceivedDate = x.ReceivedDate,
                ReceivedAmountUsd = x.ReceivedAmountUsd
            }).ToList();
        }

        public async Task<SeafoodPagedResult<PaymentInstallmentDto>> PagePaymentsAsync(string? search, int skip, int take)
        {
            var query = _db.PaymentInstallments.AsNoTracking().Include(x => x.ExportShipment)!.ThenInclude(s => s!.Customer).AsQueryable();
            var term = search?.Trim();
            if (!string.IsNullOrWhiteSpace(term))
                query = query.Where(x => (x.ExportShipment!.InvoiceNo ?? "").Contains(term) || (x.ExportShipment.Customer != null && x.ExportShipment.Customer.Name.Contains(term)));
            var total = await query.CountAsync();
            var rows = await query.OrderByDescending(x => x.Id).Skip(Math.Max(0, skip)).Take(Math.Clamp(take, 1, 100)).ToListAsync();
            return new SeafoodPagedResult<PaymentInstallmentDto>
            {
                TotalCount = total,
                Items = rows.Select(x => new PaymentInstallmentDto
                {
                    Id = x.Id, ExportShipmentId = x.ExportShipmentId, InvoiceNo = x.ExportShipment?.InvoiceNo,
                    CustomerName = x.ExportShipment?.Customer?.Name, Sequence = x.Sequence, Ratio = x.Ratio,
                    AmountUsd = x.AmountUsd, DueDate = x.DueDate, ReceivedDate = x.ReceivedDate, ReceivedAmountUsd = x.ReceivedAmountUsd
                }).ToList()
            };
        }

        public async Task<PaymentInstallmentDto> ReceiveAsync(int id, decimal amount, DateTime? date)
        {
            if (amount <= 0)
                throw new GlobalException("Số tiền nhận phải lớn hơn 0.", HttpStatusCode.BadRequest);
            await using var transaction = await _db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
            var row = await _db.PaymentInstallments.Include(x => x.ExportShipment)!.ThenInclude(s => s!.Customer)
                .Include(x => x.SalesInvoice)
                .FirstOrDefaultAsync(x => x.Id == id)
                ?? throw new GlobalException("Không tìm thấy đợt thanh toán.", HttpStatusCode.NotFound);
            if (row.ReceivedAmountUsd + amount > row.AmountUsd + 0.01m)
                throw new GlobalException("Số tiền nhận vượt số tiền còn phải thu của đợt.", HttpStatusCode.Conflict);
            row.ReceivedAmountUsd += amount;
            row.ReceivedDate = date ?? DateTime.UtcNow;
            var payment = new PaymentTransaction
            {
                CustomerId = row.ExportShipment?.CustomerId,
                InvoiceId = row.SalesInvoiceId,
                AmountUsd = amount,
                ReceivedDate = row.ReceivedDate.Value,
                Note = $"Payment installment #{row.Sequence}"
            };
            _db.PaymentTransactions.Add(payment);
            await _db.SaveChangesAsync();
            _db.PaymentAllocations.Add(new PaymentAllocation { PaymentTransactionId = payment.Id, PaymentInstallmentId = row.Id, InvoiceId = row.SalesInvoiceId, AmountUsd = amount });
            await _db.SaveChangesAsync();
            if (row.SalesInvoice is SalesInvoice invoice)
            {
                var received = await _db.PaymentAllocations.Where(x => x.InvoiceId == invoice.Id)
                    .SumAsync(x => (decimal?)x.AmountUsd) ?? 0;
                invoice.Status = received + 0.01m >= invoice.AmountUsd
                    ? InvoiceStatus.Paid
                    : InvoiceStatus.PartiallyPaid;
                if (row.ExportShipment?.SalesContractId is int contractId)
                {
                    var contractInvoices = await _db.SalesInvoices.Where(x => x.SalesContractId == contractId).Select(x => x.Id).ToListAsync();
                    var totalDue = await _db.SalesInvoices.Where(x => contractInvoices.Contains(x.Id)).SumAsync(x => (decimal?)x.AmountUsd) ?? 0;
                    var totalReceived = await _db.PaymentAllocations.Where(x => x.InvoiceId.HasValue && contractInvoices.Contains(x.InvoiceId.Value)).SumAsync(x => (decimal?)x.AmountUsd) ?? 0;
                    var contract = await _db.SalesContracts.FirstAsync(x => x.Id == contractId);
                    contract.Status = totalDue > 0 && totalReceived + 0.01m >= totalDue
                        ? ContractStatus.Paid
                        : totalReceived > 0 ? ContractStatus.PartiallyPaid : contract.Status;
                }
                await _db.SaveChangesAsync();
            }
            await transaction.CommitAsync();
            return new PaymentInstallmentDto
            {
                Id = row.Id, ExportShipmentId = row.ExportShipmentId, InvoiceNo = row.ExportShipment?.InvoiceNo,
                CustomerName = row.ExportShipment?.Customer?.Name, Sequence = row.Sequence, Ratio = row.Ratio,
                AmountUsd = row.AmountUsd, DueDate = row.DueDate, ReceivedDate = row.ReceivedDate,
                ReceivedAmountUsd = row.ReceivedAmountUsd
            };
        }

        public async Task<List<CustomerDepositDto>> ListDepositsAsync()
        {
            var rows = await _db.CustomerDeposits.Include(x => x.Customer).Include(x => x.Allocations).ThenInclude(a => a.SalesContract)
                .OrderByDescending(x => x.Id).ToListAsync();
            return rows.Select(MapDeposit).ToList();
        }

        public async Task<SeafoodPagedResult<CustomerDepositDto>> PageDepositsAsync(string? search, int skip, int take)
        {
            var query = _db.CustomerDeposits.AsNoTracking().Include(x => x.Customer).AsQueryable();
            var term = search?.Trim();
            if (!string.IsNullOrWhiteSpace(term))
                query = query.Where(x => (x.Customer != null && x.Customer.Name.Contains(term)) || (x.Note ?? "").Contains(term));
            var total = await query.CountAsync();
            var rows = await _db.CustomerDeposits.Include(x => x.Customer).Include(x => x.Allocations).ThenInclude(a => a.SalesContract)
                .Where(x => string.IsNullOrWhiteSpace(term) || (x.Customer != null && x.Customer.Name.Contains(term)) || (x.Note ?? "").Contains(term))
                .OrderByDescending(x => x.Id).Skip(Math.Max(0, skip)).Take(Math.Clamp(take, 1, 100)).ToListAsync();
            return new SeafoodPagedResult<CustomerDepositDto> { Items = rows.Select(MapDeposit).ToList(), TotalCount = total };
        }

        public async Task<CustomerDepositDto> SaveDepositAsync(CustomerDepositDto dto)
        {
            if (dto.AmountUsd <= 0)
                throw new GlobalException("Deposit phải lớn hơn 0.", HttpStatusCode.BadRequest);
            CustomerDeposit entity;
            if (dto.Id == 0)
            {
                entity = new CustomerDeposit();
                _db.CustomerDeposits.Add(entity);
            }
            else entity = await _db.CustomerDeposits.Include(x => x.Allocations).FirstAsync(x => x.Id == dto.Id);
            entity.CustomerId = dto.CustomerId;
            entity.AmountUsd = dto.AmountUsd;
            entity.ReceivedDate = dto.ReceivedDate == default ? DateTime.UtcNow : dto.ReceivedDate;
            entity.Note = dto.Note;
            await _db.SaveChangesAsync();
            return MapDeposit(await _db.CustomerDeposits.Include(x => x.Customer).Include(x => x.Allocations).ThenInclude(a => a.SalesContract)
                .FirstAsync(x => x.Id == entity.Id));
        }

        public async Task AllocateAsync(int depositId, int contractId, decimal amount)
        {
            if (amount <= 0)
                throw new GlobalException("Số tiền phân bổ phải lớn hơn 0.", HttpStatusCode.BadRequest);
            var deposit = await _db.CustomerDeposits.Include(x => x.Allocations).FirstOrDefaultAsync(x => x.Id == depositId)
                ?? throw new GlobalException("Không tìm thấy deposit.", HttpStatusCode.NotFound);
            var contract = await _db.SalesContracts.FirstOrDefaultAsync(x => x.Id == contractId)
                ?? throw new GlobalException("Không tìm thấy hợp đồng.", HttpStatusCode.NotFound);
            if (deposit.CustomerId != contract.CustomerId)
                throw new GlobalException("Không thể phân bổ deposit cho khách hàng khác.", HttpStatusCode.BadRequest);
            if (deposit.Allocations.Any(x => x.SalesContractId == contractId))
                throw new GlobalException("Deposit đã được phân bổ cho hợp đồng này.", HttpStatusCode.Conflict);
            if (deposit.Allocations.Sum(x => x.AmountUsd) + amount > deposit.AmountUsd + 0.01m)
                throw new GlobalException("Phân bổ vượt số dư deposit.", HttpStatusCode.Conflict);
            _db.DepositAllocations.Add(new DepositAllocation
            {
                DepositId = depositId, SalesContractId = contractId, AmountUsd = amount
            });
            await _db.SaveChangesAsync();
        }

        private static CustomerDepositDto MapDeposit(CustomerDeposit x) => new()
        {
            Id = x.Id, CustomerId = x.CustomerId, CustomerName = x.Customer?.Name,
            AmountUsd = x.AmountUsd, ReceivedDate = x.ReceivedDate, Note = x.Note,
            AllocatedUsd = x.Allocations.Sum(a => a.AmountUsd),
            Allocations = x.Allocations.Select(a => new DepositAllocationDto
            {
                Id = a.Id, SalesContractId = a.SalesContractId, ContractNo = a.SalesContract?.ContractNo,
                AmountUsd = a.AmountUsd, IsExported = a.IsExported
            }).ToList()
        };
    }

    public class DashboardService : ITransientDependency
    {
        private readonly DreamContext _db;
        public DashboardService(DreamContext db) => _db = db;

        public async Task<DashboardDto> GetAsync()
        {
            var start = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var inbound = await _db.InboundLines.Include(x => x.Purchase)
                .Where(x => x.Purchase!.CreatedAt >= start)
                .ToListAsync();
            var produced = await _db.ProductionOutputs.Include(x => x.Lot)
                .Where(x => x.Lot!.ReceivedDate >= start).SumAsync(x => (decimal?)x.RecoveredKg) ?? 0;
            var exported = await _db.ExportShipments.Where(x => (x.Etd ?? x.PackingDate) >= start)
                .SumAsync(x => (decimal?)x.QtyKg) ?? 0;
            var onHand = await _db.InventoryBalances.SumAsync(x => (decimal?)x.OnHandKg) ?? 0;
            var openUsd = await _db.SalesContractLines.Include(x => x.Contract)
                .Where(x => x.Contract!.Status != ContractStatus.Cancelled && x.Contract.Status != ContractStatus.Paid)
                .SumAsync(x => (decimal?)x.AmountUsd) ?? 0;
            var outstanding = await _db.PaymentInstallments
                .SumAsync(x => (decimal?)(x.AmountUsd - x.ReceivedAmountUsd)) ?? 0;

            var stock = await _db.InventoryBalances.Include(x => x.Sku)
                .GroupBy(x => x.Sku!.Name)
                .Select(g => new PieSliceDto { Label = g.Key ?? "SKU", Value = g.Sum(x => x.OnHandKg) })
                .ToListAsync();
            var markets = await _db.ExportShipments.Include(x => x.SalesContract)
                .Where(x => x.Etd >= start)
                .GroupBy(x => x.Route ?? "Khác")
                .Select(g => new PieSliceDto { Label = g.Key ?? "Khác", Value = g.Sum(x => x.QtyKg) })
                .ToListAsync();

            return new DashboardDto
            {
                InboundKgThisMonth = inbound.Sum(x => x.QtyKgLongLine + x.QtyKgHandline + x.QtyKgPs + x.QtyKgLand),
                ProducedKgThisMonth = produced,
                ExportKgThisMonth = exported,
                OnHandKg = onHand,
                OpenContractUsd = openUsd,
                OutstandingPaymentUsd = outstanding,
                StockBySku = stock,
                ExportByMarket = markets
            };
        }
    }
}
