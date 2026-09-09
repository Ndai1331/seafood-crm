using System.Globalization;
using System.Text.RegularExpressions;

namespace BootstrapBlazor.Server.Helper;

/// <summary>
/// USD number format: thousands "," and decimal "."
/// </summary>
public static class UsdNumberFormatHelper
{
    private static readonly CultureInfo EnUs = CultureInfo.GetCultureInfo("en-US");

    public static string Format(decimal? value, int maxDecimals = 2)
    {
        if (!value.HasValue) return string.Empty;
        if (maxDecimals <= 0) return value.Value.ToString("#,##0", EnUs);
        return value.Value.ToString($"#,##0.{new string('0', maxDecimals)}", EnUs);
    }

    public static decimal? ParseDecimal(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return null;
        var normalized = input.Trim().Replace(",", string.Empty);
        return decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.InvariantCulture, out var result)
            ? result : null;
    }

    public static string SanitizeInput(string? input)
    {
        if (string.IsNullOrEmpty(input)) return string.Empty;
        return Regex.Replace(input, @"[^\d.]", string.Empty);
    }

    public static string FormatWhileTyping(string? raw, int maxDecimals = 2)
    {
        var cleaned = SanitizeInput(raw);
        if (string.IsNullOrEmpty(cleaned)) return string.Empty;
        var hasTrailingDot = cleaned.EndsWith('.');
        var parts = cleaned.Split('.', 2);
        var intDigits = parts[0];
        if (string.IsNullOrEmpty(intDigits))
            return hasTrailingDot && maxDecimals > 0 ? "0." : string.Empty;
        if (!ulong.TryParse(intDigits, out var intValue)) intValue = 0;
        var formatted = intValue.ToString("#,##0", EnUs);
        if (parts.Length > 1)
        {
            var dec = parts[1];
            if (maxDecimals > 0 && dec.Length > maxDecimals) dec = dec[..maxDecimals];
            formatted += "." + dec;
        }
        else if (hasTrailingDot && maxDecimals > 0)
        {
            formatted += ".";
        }
        return formatted;
    }
}
