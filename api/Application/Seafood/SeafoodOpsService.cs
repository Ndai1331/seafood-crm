using System.IO.Compression;
using System.Net;
using Contract.Seafood;
using Core.Exceptions;
using Domain.Seafood;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using OfficeOpenXml;
using SqlServ4r.EntityFramework;
using Volo.Abp.DependencyInjection;

namespace Application.Seafood;

public class SeafoodOpsService : ITransientDependency
{
    private readonly DreamContext _db;
    public SeafoodOpsService(DreamContext db) => _db = db;

    public async Task<SeafoodPagedResult<InventoryRowDto>> UnifiedInventoryAsync(string? search, int skip, int take, StockLotKind? kind, int? warehouseId)
    {
        skip = Math.Max(0, skip);
        take = Math.Clamp(take, 1, 200);
        if (kind == StockLotKind.RawMaterial)
        {
            var query = _db.RawMaterialLots.AsNoTracking().Include(x => x.ProductionInputs).AsQueryable();
            var term = search?.Trim();
            if (!string.IsNullOrWhiteSpace(term))
                query = query.Where(x => x.LotNumber.Contains(term) || (x.SizeCode ?? "").Contains(term) || (x.FishFormCode ?? "").Contains(term));
            if (warehouseId is int wh) query = query.Where(x => x.WarehouseId == wh);
            var total = await query.CountAsync();
            var rows = await query.OrderByDescending(x => x.ReceivedDate).Skip(skip).Take(take).ToListAsync();
            var names = await WarehouseNamesAsync();
            return new SeafoodPagedResult<InventoryRowDto>
            {
                TotalCount = total,
                Items = rows.Select(x =>
                {
                    var used = x.ProductionInputs.Sum(i => i.QuantityKg);
                    return new InventoryRowDto
                    {
                        Id = x.Id, LotId = x.Id, LotNumber = x.LotNumber, SkuName = x.FishFormCode ?? "Nguyên liệu",
                        Kind = StockLotKind.RawMaterial, ReceivedKg = x.ActualKg, UsedKg = used, ShippedKg = 0,
                        OnHandKg = Math.Max(0, x.ActualKg - used), AllocatedKg = 0, AvailableKg = Math.Max(0, x.ActualKg - used),
                        WarehouseId = x.WarehouseId, WarehouseName = names.GetValueOrDefault(x.WarehouseId ?? 0), SizeCode = x.SizeCode
                    };
                }).ToList()
            };
        }

        var fg = _db.InventoryBalances.AsNoTracking()
            .Include(x => x.Lot).Include(x => x.Sku).Include(x => x.Movements).AsQueryable();
        var termFg = search?.Trim();
        if (!string.IsNullOrWhiteSpace(termFg))
            fg = fg.Where(x => (x.Lot!.LotNumber ?? "").Contains(termFg) || (x.Sku!.Name ?? "").Contains(termFg));
        if (warehouseId is int warehouse) fg = fg.Where(x => x.WarehouseId == warehouse);
        if (kind is StockLotKind filterKind)
            fg = SeafoodYield.WhereKind(fg, filterKind);
        var totalFg = await fg.CountAsync();
        var items = await fg.OrderBy(x => x.Lot!.ReceivedDate).Skip(skip).Take(take).ToListAsync();
        var namesFg = await WarehouseNamesAsync();
        return new SeafoodPagedResult<InventoryRowDto>
        {
            TotalCount = totalFg,
            Items = items.Select(row =>
            {
                var shipped = row.Movements.Where(m => m.MovementType == InventoryMovementType.Shipment).Sum(m => Math.Abs(m.QuantityKg));
                var received = row.Movements.Where(m => m.MovementType == InventoryMovementType.ProductionOutput || m.MovementType == InventoryMovementType.TransferIn).Sum(m => Math.Max(0, m.QuantityKg));
                return new InventoryRowDto
                {
                    Id = row.Id, LotId = row.LotId, LotNumber = row.Lot?.LotNumber ?? "", SkuId = row.SkuId,
                    SkuName = row.Sku?.Name ?? "", Kind = SeafoodYield.KindOf(row.Sku),
                    ReceivedKg = received > 0 ? received : row.OnHandKg + shipped,
                    UsedKg = 0, ShippedKg = shipped, OnHandKg = row.OnHandKg, AllocatedKg = row.AllocatedKg,
                    AvailableKg = row.AvailableKg, WarehouseId = row.WarehouseId,
                    WarehouseName = namesFg.GetValueOrDefault(row.WarehouseId ?? 0),
                    Movements = row.Movements.OrderByDescending(x => x.OccurredAt).Take(10).Select(x => new InventoryMovementDto
                    {
                        Id = x.Id, MovementType = x.MovementType, QuantityKg = x.QuantityKg, ReferenceType = x.ReferenceType,
                        ReferenceId = x.ReferenceId, OccurredAt = x.OccurredAt, Reason = x.Reason
                    }).ToList()
                };
            }).ToList()
        };
    }

    public async Task TransferAsync(InventoryTransferDto dto, int? userId)
    {
        if (dto.QuantityKg <= 0 || dto.ToWarehouseId <= 0 || string.IsNullOrWhiteSpace(dto.Reason))
            throw new GlobalException("Chuyển kho cần số kg, kho đích và lý do.", HttpStatusCode.BadRequest);
        if (dto.Kind == StockLotKind.RawMaterial)
        {
            var lot = await _db.RawMaterialLots.Include(x => x.ProductionInputs).FirstOrDefaultAsync(x => x.Id == dto.SourceId)
                ?? throw new GlobalException("Không tìm thấy lô nguyên liệu.", HttpStatusCode.NotFound);
            var remaining = lot.ActualKg - lot.ProductionInputs.Sum(x => x.QuantityKg);
            if (dto.QuantityKg > remaining + 0.0001m)
                throw new GlobalException("Không thể chuyển quá lượng nguyên liệu còn lại.", HttpStatusCode.Conflict);
            if (lot.ProductionInputs.Count > 0 && dto.QuantityKg + 0.0001m < remaining)
                throw new GlobalException("Lô đã đưa vào sản xuất; chỉ chuyển toàn bộ phần còn lại sang kho khác.", HttpStatusCode.Conflict);
            lot.WarehouseId = dto.ToWarehouseId;
            _db.DomainAuditLogs.Add(new DomainAuditLog
            {
                EntityType = "RawMaterialLot", EntityId = lot.Id, Action = "Transferred", UserId = userId,
                Reason = dto.Reason, ChangesJson = JsonConvert.SerializeObject(new { dto.ToWarehouseId, dto.QuantityKg })
            });
            await _db.SaveChangesAsync();
            return;
        }

        await SeafoodTransactions.ExecuteAsync(_db, async () =>
        {
        var source = await _db.InventoryBalances.Include(x => x.Sku).FirstOrDefaultAsync(x => x.Id == dto.SourceId)
            ?? throw new GlobalException("Không tìm thấy dòng tồn.", HttpStatusCode.NotFound);
        if (source.WarehouseId == dto.ToWarehouseId)
            throw new GlobalException("Kho đích trùng kho nguồn.", HttpStatusCode.BadRequest);
        if (source.AvailableKg + 0.0001m < dto.QuantityKg)
            throw new GlobalException("Không đủ hàng khả dụng để chuyển kho.", HttpStatusCode.Conflict);

        source.OnHandKg -= dto.QuantityKg;
        _db.InventoryMovements.Add(new InventoryMovement
        {
            InventoryBalanceId = source.Id, MovementType = InventoryMovementType.TransferOut,
            QuantityKg = -dto.QuantityKg, ReferenceType = "WarehouseTransfer", CreatedByUserId = userId, Reason = dto.Reason
        });

        var dest = await _db.InventoryBalances.FirstOrDefaultAsync(x => x.LotId == source.LotId && x.SkuId == source.SkuId && x.WarehouseId == dto.ToWarehouseId);
        if (dest == null)
        {
            dest = new InventoryBalance { LotId = source.LotId, SkuId = source.SkuId, WarehouseId = dto.ToWarehouseId, OnHandKg = 0 };
            _db.InventoryBalances.Add(dest);
            await _db.SaveChangesAsync();
        }
        dest.OnHandKg += dto.QuantityKg;
        _db.InventoryMovements.Add(new InventoryMovement
        {
            InventoryBalanceId = dest.Id, MovementType = InventoryMovementType.TransferIn,
            QuantityKg = dto.QuantityKg, ReferenceType = "WarehouseTransfer", CreatedByUserId = userId, Reason = dto.Reason
        });
        _db.DomainAuditLogs.Add(new DomainAuditLog
        {
            EntityType = "InventoryBalance", EntityId = source.Id, Action = "Transferred", UserId = userId,
            Reason = dto.Reason, ChangesJson = JsonConvert.SerializeObject(new { from = source.Id, to = dest.Id, dto.QuantityKg })
        });
        await _db.SaveChangesAsync();
        });
    }

    public async Task<SeafoodSelect2SearchResponseDto> SearchInvoicesAsync(string? search, int page = 1, int pageSize = 20)
    {
        var query = _db.SalesInvoices.AsNoTracking().Include(x => x.Customer).AsQueryable();
        var term = search?.Trim();
        if (!string.IsNullOrWhiteSpace(term))
            query = query.Where(x => x.InvoiceNo.Contains(term) || (x.Customer != null && x.Customer.Name.Contains(term)));
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);
        var total = await query.CountAsync();
        var results = await query.OrderByDescending(x => x.Id).Skip((page - 1) * pageSize).Take(pageSize)
            .Select(x => new SeafoodSelectOptionDto
            {
                Id = x.Id,
                Text = x.InvoiceNo + " — " + (x.Customer != null ? x.Customer.Name : ""),
                Description = x.AmountUsd.ToString("N2"),
                Code = x.InvoiceNo
            }).ToListAsync();
        return new SeafoodSelect2SearchResponseDto { Results = results, More = page * pageSize < total };
    }

    public async Task<SeafoodPagedResult<SalesInvoiceDto>> PageInvoicesAsync(string? search, int skip, int take)
    {
        var query = _db.SalesInvoices.AsNoTracking().Include(x => x.Customer).Include(x => x.SalesContract).AsQueryable();
        var term = search?.Trim();
        if (!string.IsNullOrWhiteSpace(term))
            query = query.Where(x => x.InvoiceNo.Contains(term) || (x.Customer != null && x.Customer.Name.Contains(term))
                || (x.SalesContract != null && x.SalesContract.ContractNo.Contains(term)));
        var total = await query.CountAsync();
        var rows = await query.OrderByDescending(x => x.Id).Skip(Math.Max(0, skip)).Take(Math.Clamp(take, 1, 100)).ToListAsync();
        var ids = rows.Select(x => x.Id).ToList();
        var received = await _db.PaymentAllocations.Where(x => x.InvoiceId.HasValue && ids.Contains(x.InvoiceId.Value))
            .GroupBy(x => x.InvoiceId!.Value).Select(g => new { g.Key, Sum = g.Sum(x => x.AmountUsd) }).ToDictionaryAsync(x => x.Key, x => x.Sum);
        var deposits = await _db.DepositAllocations.Where(x => x.SalesInvoiceId.HasValue && ids.Contains(x.SalesInvoiceId.Value))
            .GroupBy(x => x.SalesInvoiceId!.Value).Select(g => new { g.Key, Sum = g.Sum(x => x.AmountUsd) }).ToDictionaryAsync(x => x.Key, x => x.Sum);
        return new SeafoodPagedResult<SalesInvoiceDto>
        {
            TotalCount = total,
            Items = rows.Select(x => MapInvoice(x, received.GetValueOrDefault(x.Id) + deposits.GetValueOrDefault(x.Id))).ToList()
        };
    }

    public async Task AllocatePaymentAsync(PaymentAllocateRequestDto request, int? userId)
    {
        if (request.AmountUsd <= 0 || request.Items.Count == 0)
            throw new GlobalException("Cần số tiền và ít nhất một dòng phân bổ.", HttpStatusCode.BadRequest);
        if (Math.Abs(request.Items.Sum(x => x.AmountUsd) - request.AmountUsd) > 0.01m)
            throw new GlobalException("Tổng phân bổ phải bằng số tiền nhận.", HttpStatusCode.BadRequest);
        await SeafoodTransactions.ExecuteAsync(_db, async () =>
        {
        var payment = new PaymentTransaction
        {
            CustomerId = request.CustomerId, AmountUsd = request.AmountUsd,
            ReceivedDate = request.ReceivedDate ?? DateTime.UtcNow, ReferenceNo = request.ReferenceNo, Note = request.Note
        };
        _db.PaymentTransactions.Add(payment);
        await _db.SaveChangesAsync();
        foreach (var item in request.Items)
        {
            if (item.AmountUsd <= 0) throw new GlobalException("Dòng phân bổ phải > 0.", HttpStatusCode.BadRequest);
            PaymentInstallment? installment = null;
            if (item.PaymentInstallmentId is int instId)
            {
                installment = await _db.PaymentInstallments.Include(x => x.SalesInvoice).FirstOrDefaultAsync(x => x.Id == instId)
                    ?? throw new GlobalException("Không tìm thấy đợt thanh toán.", HttpStatusCode.NotFound);
                if (installment.ReceivedAmountUsd + item.AmountUsd > installment.AmountUsd + 0.01m)
                    throw new GlobalException("Vượt số còn phải thu của đợt.", HttpStatusCode.Conflict);
                installment.ReceivedAmountUsd += item.AmountUsd;
                installment.ReceivedDate = payment.ReceivedDate;
            }
            var invoiceId = item.InvoiceId ?? installment?.SalesInvoiceId;
            _db.PaymentAllocations.Add(new PaymentAllocation
            {
                PaymentTransactionId = payment.Id, PaymentInstallmentId = item.PaymentInstallmentId,
                InvoiceId = invoiceId, AmountUsd = item.AmountUsd
            });
            if (invoiceId is int invId)
            {
                var invoice = await _db.SalesInvoices.FirstAsync(x => x.Id == invId);
                var paid = await _db.PaymentAllocations.Where(x => x.InvoiceId == invId).SumAsync(x => (decimal?)x.AmountUsd) ?? 0;
                paid += item.AmountUsd;
                invoice.Status = paid + 0.01m >= invoice.AmountUsd ? InvoiceStatus.Paid : InvoiceStatus.PartiallyPaid;
            }
        }
        _db.DomainAuditLogs.Add(new DomainAuditLog
        {
            EntityType = "PaymentTransaction", EntityId = (int)Math.Min(payment.Id, int.MaxValue), Action = "Allocated",
            UserId = userId, ChangesJson = JsonConvert.SerializeObject(request)
        });
        await _db.SaveChangesAsync();
        });
    }

    public async Task<SeafoodPagedResult<DomainAuditLogDto>> PageAuditAsync(string? search, int skip, int take)
    {
        var query = _db.DomainAuditLogs.AsNoTracking().AsQueryable();
        var term = search?.Trim();
        if (!string.IsNullOrWhiteSpace(term))
            query = query.Where(x => x.EntityType.Contains(term) || x.Action.Contains(term) || (x.Reason ?? "").Contains(term));
        var total = await query.CountAsync();
        var rows = await query.OrderByDescending(x => x.CreatedAt).Skip(Math.Max(0, skip)).Take(Math.Clamp(take, 1, 100)).ToListAsync();
        return new SeafoodPagedResult<DomainAuditLogDto>
        {
            TotalCount = total,
            Items = rows.Select(x => new DomainAuditLogDto
            {
                Id = x.Id, EntityType = x.EntityType, EntityId = x.EntityId, Action = x.Action,
                UserId = x.UserId, CreatedAt = x.CreatedAt, ChangesJson = x.ChangesJson, Reason = x.Reason
            }).ToList()
        };
    }

    public async Task<ReportTableDto> GetReportAsync(ReportQueryDto query)
    {
        var from = query.From ?? DateTime.UtcNow.AddMonths(-1);
        var to = query.To ?? DateTime.UtcNow.AddDays(1);
        return query.Kind.ToLowerInvariant() switch
        {
            "inventory" => await InventoryReportAsync(),
            "production" => await ProductionReportAsync(from, to),
            "orders" => await OrdersReportAsync(from, to),
            "shipments" => await ShipmentsReportAsync(from, to),
            "ar" => await ArReportAsync(),
            _ => await InboundReportAsync(from, to)
        };
    }

    public async Task<byte[]> ExportReportExcelAsync(ReportQueryDto query)
    {
        var table = await GetReportAsync(query);
        ExcelPackage.License.SetNonCommercialOrganization("Seafood CRM");
        using var package = new ExcelPackage();
        var sheet = package.Workbook.Worksheets.Add(table.Title);
        for (var c = 0; c < table.Columns.Count; c++) sheet.Cells[1, c + 1].Value = table.Columns[c];
        for (var r = 0; r < table.Rows.Count; r++)
            for (var c = 0; c < table.Rows[r].Count; c++)
                sheet.Cells[r + 2, c + 1].Value = table.Rows[r][c];
        sheet.Cells[1, 1, 1, Math.Max(1, table.Columns.Count)].Style.Font.Bold = true;
        sheet.Cells.AutoFitColumns();
        return package.GetAsByteArray();
    }

    public async Task<byte[]> BackupZipAsync()
    {
        ExcelPackage.License.SetNonCommercialOrganization("Seafood CRM");
        using var zipStream = new MemoryStream();
        using (var zip = new ZipArchive(zipStream, ZipArchiveMode.Create, true))
        {
            await AddSheetAsync(zip, "danh-muc.xlsx", "Danh mục",
                new[] { "Nhóm", "Mã", "Tên" },
                (await _db.CatalogLookups.AsNoTracking().ToListAsync()).Select(x => new[] { x.Category.ToString(), x.Code, x.Name }));
            await AddSheetAsync(zip, "nhap-hang.xlsx", "Nhập hàng",
                new[] { "Id", "HĐ", "BL", "Container", "NCC" },
                (await _db.InboundPurchases.AsNoTracking().Include(x => x.SupplierPartner).ToListAsync())
                    .Select(x => new[] { x.Id.ToString(), x.ContractNo ?? "", x.BlNumber ?? "", x.ContainerNo ?? "", x.SupplierPartner?.Name ?? "" }));
            await AddSheetAsync(zip, "lo-nl.xlsx", "Lô NL",
                new[] { "Mã lô", "Kg", "Còn" },
                (await _db.RawMaterialLots.AsNoTracking().Include(x => x.ProductionInputs).ToListAsync())
                    .Select(x => new[] { x.LotNumber, x.ActualKg.ToString("N2"), Math.Max(0, x.ActualKg - x.ProductionInputs.Sum(i => i.QuantityKg)).ToString("N2") }));
            await AddSheetAsync(zip, "san-xuat.xlsx", "Sản xuất",
                new[] { "Lot", "NL", "Thu hồi" },
                (await _db.ProductionLots.AsNoTracking().Include(x => x.Outputs).ToListAsync())
                    .Select(x => new[] { x.LotNumber, x.RawMaterialKg.ToString("N2"), x.Outputs.Sum(o => o.RecoveredKg).ToString("N2") }));
            await AddSheetAsync(zip, "ton-kho.xlsx", "Tồn kho",
                new[] { "Lot", "SKU", "Tồn" },
                (await _db.InventoryBalances.AsNoTracking().Include(x => x.Lot).Include(x => x.Sku).ToListAsync())
                    .Select(x => new[] { x.Lot?.LotNumber ?? "", x.Sku?.Name ?? "", x.OnHandKg.ToString("N2") }));
            await AddSheetAsync(zip, "hop-dong.xlsx", "Hợp đồng",
                new[] { "Số HĐ", "Khách", "Trạng thái" },
                (await _db.SalesContracts.AsNoTracking().Include(x => x.Customer).ToListAsync())
                    .Select(x => new[] { x.ContractNo, x.Customer?.Name ?? "", x.Status.ToString() }));
            await AddSheetAsync(zip, "xuat-hang.xlsx", "Xuất hàng",
                new[] { "Invoice", "Container", "Kg" },
                (await _db.ExportShipments.AsNoTracking().ToListAsync())
                    .Select(x => new[] { x.InvoiceNo ?? "", x.ContainerNo ?? "", x.QtyKg.ToString("N2") }));
            await AddSheetAsync(zip, "cong-no.xlsx", "Công nợ",
                new[] { "Invoice", "Giá trị", "Trạng thái" },
                (await _db.SalesInvoices.AsNoTracking().ToListAsync())
                    .Select(x => new[] { x.InvoiceNo, x.AmountUsd.ToString("N2"), x.Status.ToString() }));
        }
        return zipStream.ToArray();
    }

    private async Task AddSheetAsync(ZipArchive zip, string fileName, string sheetName, IReadOnlyList<string> columns, IEnumerable<string[]> rows)
    {
        using var package = new ExcelPackage();
        var sheet = package.Workbook.Worksheets.Add(sheetName);
        for (var c = 0; c < columns.Count; c++) sheet.Cells[1, c + 1].Value = columns[c];
        var r = 2;
        foreach (var row in rows)
        {
            for (var c = 0; c < row.Length; c++) sheet.Cells[r, c + 1].Value = row[c];
            r++;
        }
        var entry = zip.CreateEntry(fileName);
        await using var stream = entry.Open();
        await package.SaveAsAsync(stream);
    }

    private async Task<ReportTableDto> InboundReportAsync(DateTime from, DateTime to)
    {
        var rows = await _db.InboundPurchases.AsNoTracking().Include(x => x.Lines).Include(x => x.SupplierPartner)
            .Where(x => x.CreatedAt >= from && x.CreatedAt < to).OrderByDescending(x => x.Id).ToListAsync();
        return new ReportTableDto
        {
            Title = "Nhập nguyên liệu",
            Columns = new() { "Id", "HĐ", "BL", "NCC", "Kg", "USD" },
            Rows = rows.Select(x => new List<string>
            {
                x.Id.ToString(), x.ContractNo ?? "", x.BlNumber ?? "", x.SupplierPartner?.Name ?? "",
                x.Lines.Sum(l => l.QtyKg).ToString("N2"), x.Lines.Sum(l => l.InvoiceAmountUsd).ToString("N2")
            }).ToList()
        };
    }

    private async Task<ReportTableDto> InventoryReportAsync()
    {
        var rows = await _db.InventoryBalances.AsNoTracking().Include(x => x.Lot).Include(x => x.Sku).ToListAsync();
        return new ReportTableDto
        {
            Title = "Tồn kho",
            Columns = new() { "Lot", "SKU", "Tồn", "Giữ", "Còn" },
            Rows = rows.Select(x => new List<string>
            {
                x.Lot?.LotNumber ?? "", x.Sku?.Name ?? "", x.OnHandKg.ToString("N2"), x.AllocatedKg.ToString("N2"), x.AvailableKg.ToString("N2")
            }).ToList()
        };
    }

    private async Task<ReportTableDto> ProductionReportAsync(DateTime from, DateTime to)
    {
        var rows = await _db.ProductionLots.AsNoTracking().Include(x => x.Outputs)
            .Where(x => x.ReceivedDate >= from && x.ReceivedDate < to).ToListAsync();
        return new ReportTableDto
        {
            Title = "Sản xuất & thu hồi",
            Columns = new() { "Lot", "NL", "Thu hồi", "Hao hụt", "Tỷ lệ" },
            Rows = rows.Select(x =>
            {
                var recovered = x.Outputs.Sum(o => o.RecoveredKg);
                var waste = Math.Max(0, x.RawMaterialKg - recovered);
                var ratio = x.RawMaterialKg == 0 ? 0 : recovered / x.RawMaterialKg;
                return new List<string> { x.LotNumber, x.RawMaterialKg.ToString("N2"), recovered.ToString("N2"), waste.ToString("N2"), ratio.ToString("P1") };
            }).ToList()
        };
    }

    private async Task<ReportTableDto> OrdersReportAsync(DateTime from, DateTime to)
    {
        var rows = await _db.SalesContracts.AsNoTracking().Include(x => x.Customer).Include(x => x.Lines)
            .Where(x => x.CreatedAt >= from && x.CreatedAt < to).ToListAsync();
        return new ReportTableDto
        {
            Title = "Đơn hàng",
            Columns = new() { "Số HĐ", "Khách", "Cần giao", "Đã xếp", "Trạng thái" },
            Rows = rows.Select(x => new List<string>
            {
                x.ContractNo, x.Customer?.Name ?? "", x.Lines.Sum(l => l.QtyKg).ToString("N2"),
                x.Lines.Sum(l => l.AllocatedKg).ToString("N2"), x.Status.ToString()
            }).ToList()
        };
    }

    private async Task<ReportTableDto> ShipmentsReportAsync(DateTime from, DateTime to)
    {
        var rows = await _db.ExportShipments.AsNoTracking().Include(x => x.Customer)
            .Where(x => (x.Etd ?? x.CreatedAtFallback()) >= from && (x.Etd ?? DateTime.UtcNow) < to).ToListAsync();
        return new ReportTableDto
        {
            Title = "Xuất hàng",
            Columns = new() { "Invoice", "Khách", "Kg", "Container", "Trạng thái" },
            Rows = rows.Select(x => new List<string>
            {
                x.InvoiceNo ?? "", x.Customer?.Name ?? "", x.QtyKg.ToString("N2"), x.ContainerNo ?? "", x.Status.ToString()
            }).ToList()
        };
    }

    private async Task<ReportTableDto> ArReportAsync()
    {
        var rows = await _db.PaymentInstallments.AsNoTracking().Include(x => x.ExportShipment)!.ThenInclude(s => s!.Customer).ToListAsync();
        return new ReportTableDto
        {
            Title = "Công nợ",
            Columns = new() { "Khách", "Invoice", "Phải thu", "Đã thu", "Còn", "Hạn" },
            Rows = rows.Select(x => new List<string>
            {
                x.ExportShipment?.Customer?.Name ?? "", x.ExportShipment?.InvoiceNo ?? "",
                x.AmountUsd.ToString("N2"), x.ReceivedAmountUsd.ToString("N2"),
                (x.AmountUsd - x.ReceivedAmountUsd).ToString("N2"), x.DueDate?.ToString("yyyy-MM-dd") ?? ""
            }).ToList()
        };
    }

    private static SalesInvoiceDto MapInvoice(SalesInvoice x, decimal received)
    {
        var outstanding = Math.Max(0, x.AmountUsd - received);
        var dueSoon = x.DueDate is DateTime d && d.Date >= DateTime.UtcNow.Date && d.Date <= DateTime.UtcNow.Date.AddDays(7) && outstanding > 0;
        var overdue = x.DueDate is DateTime due && due.Date < DateTime.UtcNow.Date && outstanding > 0.01m;
        return new SalesInvoiceDto
        {
            Id = x.Id, InvoiceNo = x.InvoiceNo, SalesContractId = x.SalesContractId, ContractNo = x.SalesContract?.ContractNo,
            ShipmentId = x.ShipmentId, CustomerId = x.CustomerId, CustomerName = x.Customer?.Name,
            AmountUsd = x.AmountUsd, ReceivedUsd = received, OutstandingUsd = outstanding,
            IssueDate = x.IssueDate, DueDate = x.DueDate, Status = x.Status, IsOverdue = overdue, IsDueSoon = dueSoon
        };
    }

    private async Task<Dictionary<int, string>> WarehouseNamesAsync()
        => await _db.CatalogLookups.Where(x => x.Category == LookupCategory.Warehouse)
            .ToDictionaryAsync(x => x.Id, x => x.Name);
}

internal static class SeafoodShipmentDate
{
    public static DateTime CreatedAtFallback(this ExportShipment x) => x.Etd ?? x.PackingDate ?? DateTime.UtcNow;
}
