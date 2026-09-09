namespace Core.Helper
{
    /// <summary>US pound &lt;-&gt; kilogram conversion used on export orders.</summary>
    public static class WeightUnits
    {
        public const decimal KgPerPound = 0.45359237m;

        public static decimal LbsToKg(decimal pounds) => decimal.Round(pounds * KgPerPound, 3);

        public static decimal KgToLbs(decimal kilograms) =>
            kilograms == 0 ? 0 : decimal.Round(kilograms / KgPerPound, 3);
    }
}
