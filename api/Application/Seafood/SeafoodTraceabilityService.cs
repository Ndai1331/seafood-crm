using System.Net;
using Contract.Seafood;
using Core.Exceptions;
using Domain.Seafood;
using Microsoft.EntityFrameworkCore;
using SqlServ4r.EntityFramework;
using Volo.Abp.DependencyInjection;

namespace Application.Seafood;

public class SeafoodTraceabilityService : ITransientDependency
{
    private readonly DreamContext _db;

    public SeafoodTraceabilityService(DreamContext db) => _db = db;

    public async Task<TraceabilityDto> GetAsync(string rootType, int rootId)
    {
        rootType = NormalizeRoot(rootType);
        ValidateRoot(rootType);
        var links = await _db.TraceabilityLinks.AsNoTracking().ToListAsync();
        var inputs = await _db.ProductionInputs.AsNoTracking().ToListAsync();
        var allocations = await _db.SalesAllocations.AsNoTracking().ToListAsync();
        var balances = await _db.InventoryBalances.AsNoTracking().ToListAsync();
        var productionLots = await _db.ProductionLots.AsNoTracking().ToDictionaryAsync(x => x.Id, x => x.LotNumber);
        var rawLots = await _db.RawMaterialLots.AsNoTracking().ToDictionaryAsync(x => x.Id, x => x.LotNumber);
        var contracts = await _db.SalesContracts.AsNoTracking().ToDictionaryAsync(x => x.Id, x => x.ContractNo);
        var lines = await _db.SalesContractLines.AsNoTracking().ToDictionaryAsync(x => x.Id, x => $"SKU {x.SkuId} / dòng {x.Id}");
        var shipments = await _db.ExportShipments.AsNoTracking().ToDictionaryAsync(x => x.Id, x => $"Shipment #{x.Id}{(string.IsNullOrWhiteSpace(x.InvoiceNo) ? "" : $" / {x.InvoiceNo}")}");

        var edges = new List<TraceabilityEdgeDto>();
        foreach (var link in links)
            AddEdge(edges, link.FromType, link.FromId, link.ToType, link.ToId, link.QuantityKg, "domain link");
        foreach (var input in inputs)
            AddEdge(edges, "RawMaterialLot", input.RawMaterialLotId, "ProductionLot", input.ProductionLotId, input.QuantityKg, "production input");
        foreach (var allocation in allocations.Where(x => x.IsActive || x.ShipmentId != null))
        {
            AddEdge(edges, "SalesContractLine", allocation.SalesContractLineId, "SalesAllocation", allocation.Id, allocation.QuantityKg, "sales allocation");
            var balance = balances.FirstOrDefault(x => x.Id == allocation.InventoryBalanceId);
            if (balance != null)
                AddEdge(edges, "SalesAllocation", allocation.Id, "ProductionLot", balance.LotId, allocation.QuantityKg, "finished lot allocation");
            if (allocation.ShipmentId is int shipmentId)
                AddEdge(edges, "SalesAllocation", allocation.Id, "Shipment", shipmentId, allocation.QuantityKg, "shipment");
        }
        var contractLines = await _db.SalesContractLines.AsNoTracking().ToListAsync();
        foreach (var line in contractLines)
            AddEdge(edges, "SalesContract", line.ContractId, "SalesContractLine", line.Id, line.QtyKg, "contract line");
        foreach (var balance in balances)
            AddEdge(edges, "ProductionLot", balance.LotId, "InventoryBalance", balance.Id, balance.OnHandKg, "finished stock");
        var inboundLots = await _db.RawMaterialLots.AsNoTracking().ToListAsync();
        foreach (var lot in inboundLots)
            AddEdge(edges, "InboundPurchase", lot.InboundPurchaseId, "RawMaterialLot", lot.Id, lot.ActualKg, "inbound lot");
        var invoices = await _db.SalesInvoices.AsNoTracking().ToListAsync();
        foreach (var invoice in invoices)
        {
            if (invoice.SalesContractId is int contractId)
                AddEdge(edges, "SalesContract", contractId, "SalesInvoice", invoice.Id, 0, "invoice");
            if (invoice.ShipmentId is int shipmentId)
                AddEdge(edges, "Shipment", shipmentId, "SalesInvoice", invoice.Id, 0, "invoice");
        }

        var root = (rootType, (long)rootId);
        var adjacency = edges.GroupBy(x => (x.FromType, x.FromId)).ToDictionary(x => x.Key, x => x.ToList());
        var reverse = edges.GroupBy(x => (x.ToType, x.ToId)).ToDictionary(x => x.Key, x => x.ToList());
        var visited = new HashSet<(string Type, long Id)> { root };
        var queue = new Queue<(string Type, long Id)>();
        queue.Enqueue(root);
        var selectedEdges = new List<TraceabilityEdgeDto>();
        while (queue.Count > 0 && visited.Count < 500)
        {
            var current = queue.Dequeue();
            foreach (var edge in adjacency.GetValueOrDefault(current, new()))
            {
                selectedEdges.Add(edge);
                if (visited.Add((edge.ToType, edge.ToId))) queue.Enqueue((edge.ToType, edge.ToId));
            }
            foreach (var edge in reverse.GetValueOrDefault(current, new()))
            {
                selectedEdges.Add(edge);
                if (visited.Add((edge.FromType, edge.FromId))) queue.Enqueue((edge.FromType, edge.FromId));
            }
        }

        var nodes = visited.Select(node => new TraceabilityNodeDto
        {
            Type = node.Type,
            Id = node.Id,
            Label = Label(node.Type, node.Id, productionLots, rawLots, contracts, lines, shipments)
        }).OrderBy(x => x.Type).ThenBy(x => x.Id).ToList();
        return new TraceabilityDto
        {
            RootType = rootType,
            RootId = rootId,
            Nodes = nodes,
            Edges = selectedEdges.DistinctBy(x => $"{x.FromType}:{x.FromId}>{x.ToType}:{x.ToId}:{x.Relation}").ToList()
        };
    }

    private static void ValidateRoot(string rootType)
    {
        if (!new[] { "Shipment", "SalesContract", "SalesContractLine", "ProductionLot", "RawMaterialLot", "InventoryBalance", "InboundPurchase", "SalesInvoice" }
            .Contains(rootType, StringComparer.OrdinalIgnoreCase))
            throw new GlobalException("Loại đối tượng truy xuất không hợp lệ.", HttpStatusCode.BadRequest);
    }

    private static string NormalizeRoot(string rootType)
        => new[] { "Shipment", "SalesContract", "SalesContractLine", "ProductionLot", "RawMaterialLot", "InventoryBalance", "InboundPurchase", "SalesInvoice" }
            .FirstOrDefault(x => x.Equals(rootType, StringComparison.OrdinalIgnoreCase)) ?? rootType;

    private static void AddEdge(List<TraceabilityEdgeDto> edges, string fromType, long fromId, string toType, long toId, decimal quantityKg, string relation)
        => edges.Add(new TraceabilityEdgeDto { FromType = fromType, FromId = fromId, ToType = toType, ToId = toId, QuantityKg = quantityKg, Relation = relation });

    private static string Label(string type, long id, IReadOnlyDictionary<int, string> productionLots,
        IReadOnlyDictionary<int, string> rawLots, IReadOnlyDictionary<int, string> contracts,
        IReadOnlyDictionary<int, string> lines, IReadOnlyDictionary<int, string> shipments)
        => type switch
        {
            "ProductionLot" when productionLots.TryGetValue((int)id, out var lot) => $"Lô thành phẩm {lot}",
            "RawMaterialLot" when rawLots.TryGetValue((int)id, out var raw) => $"Lô nguyên liệu {raw}",
            "SalesContract" when contracts.TryGetValue((int)id, out var contract) => $"Hợp đồng {contract}",
            "SalesContractLine" when lines.TryGetValue((int)id, out var line) => line,
            "Shipment" when shipments.TryGetValue((int)id, out var shipment) => shipment,
            _ => $"{type} #{id}"
        };

    public async Task<TraceabilityDto> LookupAsync(TraceabilityLookupDto request)
    {
        if (request.OwnerId is int id && !string.IsNullOrWhiteSpace(request.OwnerType))
            return await GetAsync(request.OwnerType, id);
        var code = request.Code?.Trim();
        if (string.IsNullOrWhiteSpace(code))
            throw new GlobalException("Nhập mã lô, số hợp đồng, số container hoặc số hóa đơn.", HttpStatusCode.BadRequest);
        var raw = await _db.RawMaterialLots.AsNoTracking().FirstOrDefaultAsync(x => x.LotNumber == code);
        if (raw != null) return await GetAsync("RawMaterialLot", raw.Id);
        var prod = await _db.ProductionLots.AsNoTracking().FirstOrDefaultAsync(x => x.LotNumber == code);
        if (prod != null) return await GetAsync("ProductionLot", prod.Id);
        var contract = await _db.SalesContracts.AsNoTracking().FirstOrDefaultAsync(x => x.ContractNo == code);
        if (contract != null) return await GetAsync("SalesContract", contract.Id);
        var shipment = await _db.ExportShipments.AsNoTracking().FirstOrDefaultAsync(x => x.InvoiceNo == code || x.ContainerNo == code);
        if (shipment == null)
        {
            var container = await _db.ShipmentContainers.AsNoTracking().FirstOrDefaultAsync(c => c.ContainerNo == code);
            if (container != null) shipment = await _db.ExportShipments.AsNoTracking().FirstOrDefaultAsync(x => x.Id == container.ShipmentId);
        }
        if (shipment != null) return await GetAsync("Shipment", shipment.Id);
        var invoice = await _db.SalesInvoices.AsNoTracking().FirstOrDefaultAsync(x => x.InvoiceNo == code);
        if (invoice != null) return await GetAsync("SalesInvoice", invoice.Id);
        throw new GlobalException("Không tìm thấy đối tượng với mã đã nhập.", HttpStatusCode.NotFound);
    }
}
