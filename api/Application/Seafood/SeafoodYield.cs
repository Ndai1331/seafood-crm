using Domain.Seafood;

namespace Application.Seafood;

internal static class SeafoodYield
{
    public static (decimal RawEquivalentKg, decimal YieldRatio) ReverseFromOutput(ProductionOutput? output, decimal finishedKg)
    {
        if (output == null || output.RecoveredKg <= 0)
            return (finishedKg, 1m);
        var yield = output.RawUsedKg <= 0 ? 0m : decimal.Round(output.RecoveredKg / output.RawUsedKg, 4);
        var raw = output.RecoveredKg == 0 ? 0m : decimal.Round(finishedKg * output.RawUsedKg / output.RecoveredKg, 3);
        return (raw, yield);
    }

    public static StockLotKind KindOf(ProductSku? sku) =>
        sku == null ? StockLotKind.FinishedGoods
        : sku.Kind == ProductKind.Wip ? StockLotKind.Wip
        : sku.Kind == ProductKind.Byproduct || sku.IsByproduct ? StockLotKind.Byproduct
        : StockLotKind.FinishedGoods;

    // Filter in SQL (KindOf cannot be translated by EF).
    public static IQueryable<InventoryBalance> WhereKind(IQueryable<InventoryBalance> query, StockLotKind kind) => kind switch
    {
        StockLotKind.Wip => query.Where(x => x.Sku != null && x.Sku.Kind == ProductKind.Wip),
        StockLotKind.Byproduct => query.Where(x => x.Sku != null && (x.Sku.Kind == ProductKind.Byproduct || x.Sku.IsByproduct)),
        StockLotKind.FinishedGoods => query.Where(x => x.Sku != null && x.Sku.Kind != ProductKind.Wip && x.Sku.Kind != ProductKind.Byproduct && !x.Sku.IsByproduct),
        _ => query
    };
}
