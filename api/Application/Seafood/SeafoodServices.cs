using Contract.Seafood;
using Core.Helper;
using Domain.Seafood;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SqlServ4r.EntityFramework;
using Volo.Abp.DependencyInjection;

namespace Application.Seafood
{
    public class InboundService : ITransientDependency
    {
        private readonly DreamContext _db;
        public InboundService(DreamContext db) => _db = db;

        public async Task<List<InboundPurchaseDto>> ListAsync()
        {
            var rows = await _db.InboundPurchases.Include(x => x.Customer).Include(x => x.Lines).OrderByDescending(x => x.Id).ToListAsync();
            return rows.Select(Map).ToList();
        }

        public async Task<InboundPurchaseDto> SaveAsync(InboundPurchaseDto dto)
        {
            InboundPurchase entity;
            if (dto.Id == 0)
            {
                entity = new InboundPurchase { CreatedAt = DateTime.UtcNow };
                _db.InboundPurchases.Add(entity);
            }
            else
            {
                entity = await _db.InboundPurchases.Include(x => x.Lines).FirstAsync(x => x.Id == dto.Id);
                _db.InboundLines.RemoveRange(entity.Lines);
            }
            entity.CustomerId = dto.CustomerId;
            entity.CustomerCode = dto.CustomerCode;
            entity.ContractNo = dto.ContractNo;
            entity.Purchaser = dto.Purchaser;
            entity.MissingDocuments = dto.MissingDocuments;
            entity.Eta = dto.Eta;
            entity.WarehouseDate = dto.WarehouseDate;
            entity.BlNumber = dto.BlNumber;
            entity.ContainerNo = dto.ContainerNo;
            entity.ContainerType = dto.ContainerType;
            entity.Note = dto.Note;
            entity.Lines = dto.Lines.Select(l => new InboundLine
            {
                Commodity = l.Commodity,
                QtyKgLongLine = l.QtyKgLongLine,
                QtyKgHandline = l.QtyKgHandline,
                QtyKgPs = l.QtyKgPs,
                QtyKgLand = l.QtyKgLand,
                PriceUsd = l.PriceUsd,
                InvoiceAmountUsd = l.InvoiceAmountUsd,
                ContainerQty = l.ContainerQty
            }).ToList();
            await _db.SaveChangesAsync();
            return Map(await _db.InboundPurchases.Include(x => x.Customer).Include(x => x.Lines).FirstAsync(x => x.Id == entity.Id));
        }

        private static InboundPurchaseDto Map(InboundPurchase x) => new()
        {
            Id = x.Id,
            CustomerId = x.CustomerId,
            CustomerName = x.Customer?.Name,
            CustomerCode = x.CustomerCode,
            ContractNo = x.ContractNo,
            Purchaser = x.Purchaser,
            MissingDocuments = x.MissingDocuments,
            Eta = x.Eta,
            WarehouseDate = x.WarehouseDate,
            BlNumber = x.BlNumber,
            ContainerNo = x.ContainerNo,
            ContainerType = x.ContainerType,
            Note = x.Note,
            TotalKg = x.Lines.Sum(l => l.QtyKgLongLine + l.QtyKgHandline + l.QtyKgPs + l.QtyKgLand),
            TotalAmountUsd = x.Lines.Sum(l => l.InvoiceAmountUsd),
            Lines = x.Lines.Select(l => new InboundLineDto
            {
                Id = l.Id, Commodity = l.Commodity, QtyKgLongLine = l.QtyKgLongLine, QtyKgHandline = l.QtyKgHandline,
                QtyKgPs = l.QtyKgPs, QtyKgLand = l.QtyKgLand, PriceUsd = l.PriceUsd, InvoiceAmountUsd = l.InvoiceAmountUsd,
                ContainerQty = l.ContainerQty
            }).ToList()
        };
    }

    public class ProductionService : ITransientDependency
    {
        private readonly DreamContext _db;
        public ProductionService(DreamContext db) => _db = db;

        public async Task<List<ProductionLotDto>> ListAsync()
        {
            var lots = await _db.ProductionLots.Include(x => x.Outputs).ThenInclude(o => o.Sku)
                .Include(x => x.Certificates).OrderByDescending(x => x.Id).ToListAsync();
            return lots.Select(Map).ToList();
        }

        public async Task<ProductionLotDto> SaveAsync(ProductionLotDto dto)
        {
            ProductionLot entity;
            if (dto.Id == 0)
            {
                entity = new ProductionLot();
                _db.ProductionLots.Add(entity);
            }
            else
            {
                entity = await _db.ProductionLots.Include(x => x.Outputs).Include(x => x.Certificates)
                    .FirstAsync(x => x.Id == dto.Id);
                _db.ProductionOutputs.RemoveRange(entity.Outputs);
                _db.LotCertificates.RemoveRange(entity.Certificates);
            }
            entity.LotNumber = dto.LotNumber;
            entity.InboundPurchaseId = dto.InboundPurchaseId;
            entity.ReceivedDate = dto.ReceivedDate == default ? DateTime.UtcNow : dto.ReceivedDate;
            entity.RawMaterialKg = dto.RawMaterialKg;
            entity.Note = dto.Note;
            entity.Outputs = dto.Outputs.Select(o => new ProductionOutput
            {
                SkuId = o.SkuId,
                RecoveredKg = o.RecoveredKg,
                RawUsedKg = o.RawUsedKg,
                YieldRatio = o.RawUsedKg == 0 ? 0 : decimal.Round(o.RecoveredKg / o.RawUsedKg, 4),
                ExportMarket = o.ExportMarket
            }).ToList();
            entity.Certificates = dto.CertificateCodes.Distinct().Select(c => new LotCertificate { CertificateCode = c }).ToList();
            await _db.SaveChangesAsync();

            foreach (var output in entity.Outputs)
            {
                var bal = await _db.InventoryBalances.FirstOrDefaultAsync(b => b.LotId == entity.Id && b.SkuId == output.SkuId);
                if (bal == null)
                {
                    _db.InventoryBalances.Add(new InventoryBalance
                    {
                        LotId = entity.Id, SkuId = output.SkuId, OnHandKg = output.RecoveredKg, AllocatedKg = 0
                    });
                }
                else
                {
                    bal.OnHandKg = output.RecoveredKg;
                }
            }
            await _db.SaveChangesAsync();
            return Map(await _db.ProductionLots.Include(x => x.Outputs).ThenInclude(o => o.Sku)
                .Include(x => x.Certificates).FirstAsync(x => x.Id == entity.Id));
        }

        private static ProductionLotDto Map(ProductionLot x)
        {
            var recovered = x.Outputs.Sum(o => o.RecoveredKg);
            return new ProductionLotDto
            {
                Id = x.Id, LotNumber = x.LotNumber, InboundPurchaseId = x.InboundPurchaseId,
                ReceivedDate = x.ReceivedDate, RawMaterialKg = x.RawMaterialKg,
                RecoveryRatio = x.RawMaterialKg == 0 ? 0 : decimal.Round(recovered / x.RawMaterialKg, 4),
                Note = x.Note,
                CertificateCodes = x.Certificates.Select(c => c.CertificateCode).ToList(),
                Outputs = x.Outputs.Select(o => new ProductionOutputDto
                {
                    Id = o.Id, SkuId = o.SkuId, SkuName = o.Sku?.Name, RecoveredKg = o.RecoveredKg,
                    RawUsedKg = o.RawUsedKg, YieldRatio = o.YieldRatio, ExportMarket = o.ExportMarket
                }).ToList()
            };
        }
    }

    public class InventoryService : ITransientDependency
    {
        private readonly DreamContext _db;
        public InventoryService(DreamContext db) => _db = db;

        public async Task<List<InventoryRowDto>> ListAsync()
        {
            var rows = await _db.InventoryBalances
                .Include(x => x.Lot)!.ThenInclude(l => l!.Certificates)
                .Include(x => x.Sku)
                .ToListAsync();

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
                    if (rule?.MustHaveCertificateCode != null && !certs.Contains(rule.MustHaveCertificateCode))
                    {
                        warning = $"Đủ hàng nhưng thiếu chứng từ {rule.MustHaveCertificateCode} cho thị trường {market?.Name}. Cần mua thêm NL đúng giấy tờ.";
                    }
                    else if (!certs.Contains("COA") && !certs.Contains("EUCC") && !certs.Contains("MSC"))
                    {
                        warning = "Đủ hàng trong kho, nhưng chưa có chứng từ để xuất, cần mua thêm NL có chứng từ phù hợp";
                    }
                }
                result.Add(new InventoryRowDto
                {
                    Id = row.Id, LotId = row.LotId, LotNumber = row.Lot?.LotNumber ?? "",
                    SkuId = row.SkuId, SkuName = row.Sku?.Name ?? "",
                    OnHandKg = row.OnHandKg, AllocatedKg = row.AllocatedKg, AvailableKg = row.AvailableKg,
                    Certificates = certs, Warning = warning
                });
            }
            return result;
        }
    }

    public class ExportService : ITransientDependency
    {
        private readonly DreamContext _db;
        public ExportService(DreamContext db) => _db = db;

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

        public async Task<SalesContractDto> SaveContractAsync(SalesContractDto dto)
        {
            SalesContract entity;
            if (dto.Id == 0)
            {
                entity = new SalesContract { CreatedAt = DateTime.UtcNow, Status = ContractStatus.Draft };
                _db.SalesContracts.Add(entity);
            }
            else
            {
                entity = await _db.SalesContracts.Include(x => x.Lines).FirstAsync(x => x.Id == dto.Id);
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
            entity.Status = ContractStatus.Signed;
            entity.ApprovalNote = note;
            entity.ApprovedBy = userId;
            entity.ApprovedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return MapContract(entity);
        }

        public async Task<List<ExportShipmentDto>> ListShipmentsAsync()
        {
            var rows = await _db.ExportShipments.Include(x => x.Customer).Include(x => x.PaymentTerm).OrderByDescending(x => x.Id).ToListAsync();
            return rows.Select(MapShip).ToList();
        }

        public async Task<ExportShipmentDto> SaveShipmentAsync(ExportShipmentDto dto)
        {
            ExportShipment entity;
            if (dto.Id == 0)
            {
                entity = new ExportShipment();
                _db.ExportShipments.Add(entity);
            }
            else entity = await _db.ExportShipments.FirstAsync(x => x.Id == dto.Id);
            entity.SalesContractId = dto.SalesContractId;
            entity.CustomerId = dto.CustomerId;
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
            await _db.SaveChangesAsync();

            if (dto.PaymentTermId is int termId && dto.Id == 0)
            {
                var term = await _db.PaymentTerms.FindAsync(termId);
                var ratios = JsonConvert.DeserializeObject<List<decimal>>(term?.RatiosJson ?? "[]") ?? new List<decimal>();
                var seq = 1;
                foreach (var ratio in ratios)
                {
                    _db.PaymentInstallments.Add(new PaymentInstallment
                    {
                        ExportShipmentId = entity.Id,
                        Sequence = seq++,
                        Ratio = ratio,
                        AmountUsd = decimal.Round(entity.AmountUsd * ratio, 2),
                        DueDate = term?.DueDays is int days ? (entity.Etd ?? DateTime.UtcNow).AddDays(days) : entity.Eta
                    });
                }
                await _db.SaveChangesAsync();
            }

            if (entity.SalesContractId is int scId)
            {
                var contract = await _db.SalesContracts.FindAsync(scId);
                if (contract != null)
                {
                    contract.Status = ContractStatus.Shipped;
                    await _db.SaveChangesAsync();
                }
            }

            return MapShip(await _db.ExportShipments.Include(x => x.Customer).Include(x => x.PaymentTerm).FirstAsync(x => x.Id == entity.Id));
        }

        private static SalesContractDto MapContract(SalesContract x) => new()
        {
            Id = x.Id, ContractNo = x.ContractNo, CustomerContractNo = x.CustomerContractNo,
            CustomerId = x.CustomerId, CustomerName = x.Customer?.Name, MarketId = x.MarketId,
            Status = x.Status, SuggestedUnitPriceUsd = x.SuggestedUnitPriceUsd,
            VarianceVsLastPct = x.VarianceVsLastPct, VarianceVsPeersPct = x.VarianceVsPeersPct,
            ApprovalNote = x.ApprovalNote,
            Lines = x.Lines.Select(l => new SalesContractLineDto
            {
                SkuId = l.SkuId, SkuName = l.Sku?.Name, QtyKg = l.QtyKg, QtyLbs = l.QtyLbs,
                UnitPriceUsd = l.UnitPriceUsd, AmountUsd = l.AmountUsd
            }).ToList()
        };

        private static ExportShipmentDto MapShip(ExportShipment x) => new()
        {
            Id = x.Id, SalesContractId = x.SalesContractId, CustomerId = x.CustomerId,
            CustomerName = x.Customer?.Name, ProductionNoticeNo = x.ProductionNoticeNo,
            PackingDate = x.PackingDate, Etd = x.Etd, Eta = x.Eta, InvoiceNo = x.InvoiceNo,
            PaymentTermId = x.PaymentTermId, PaymentTermName = x.PaymentTerm?.Name,
            Cartons = x.Cartons, QtyLbs = x.QtyLbs, QtyKg = x.QtyKg, AmountUsd = x.AmountUsd,
            ContainerNo = x.ContainerNo, ContainerType = x.ContainerType, Route = x.Route
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

        public async Task<PaymentInstallmentDto> ReceiveAsync(int id, decimal amount, DateTime? date)
        {
            var row = await _db.PaymentInstallments.Include(x => x.ExportShipment)!.ThenInclude(s => s!.Customer)
                .FirstAsync(x => x.Id == id);
            row.ReceivedAmountUsd = amount;
            row.ReceivedDate = date ?? DateTime.UtcNow;
            await _db.SaveChangesAsync();
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

        public async Task<CustomerDepositDto> SaveDepositAsync(CustomerDepositDto dto)
        {
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
