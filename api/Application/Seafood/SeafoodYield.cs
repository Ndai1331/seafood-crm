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
}
