namespace BootstrapBlazor.Server.Data.SeoRequests;

/// <summary>
/// Hides brand identity in the brand-code picker. Security rule: a brand value is never shown in
/// full on screen — only its last 4 characters (BC226701 → ****6701).
///
/// Two pairs in live data differ only in their second character (BC126375/BF126375,
/// BC199365/BF199365), which this mask hides, so each pair reads identically in the picker.
/// That is accepted deliberately: hiding the brand outranks telling those four apart. None of
/// them is used by a ticket today. Revealing the differing character was the alternative, and
/// it was turned down.
///
/// The API keeps its own copy of this rule (BoktBrandMasker) because UI and API are separate
/// repositories; the short-value behaviour here mirrors it deliberately.
/// </summary>
public static class SeoBrandCodeMasker
{
    /// <summary>Characters left visible at the end of a code.</summary>
    public const int VisibleTail = 4;

    /// <summary>Mask one code, keeping its last <see cref="VisibleTail"/> characters.</summary>
    public static string Mask(string? code)
    {
        var value = code?.Trim();
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        // A value no longer than the visible tail carries no prefix to hide. "K" is the
        // domain-purchase bucket rather than a brand identity, same as "Khác" on the API side.
        return value.Length <= VisibleTail
            ? "****" + value
            : "****" + value[^VisibleTail..];
    }
}
