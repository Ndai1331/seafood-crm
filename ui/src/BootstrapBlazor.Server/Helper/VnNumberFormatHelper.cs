using System.Globalization;
using System.Text.RegularExpressions;

namespace BootstrapBlazor.Server.Helper;

/// <summary>
/// Vietnamese number format: thousands "." and decimal ","
/// </summary>
public static class VnNumberFormatHelper
{
    private static readonly CultureInfo VnCulture = CultureInfo.GetCultureInfo("vi-VN");

    public static string Format(decimal? value, int maxDecimals = 2)
    {
        if (!value.HasValue)
            return string.Empty;

        if (maxDecimals <= 0)
            return value.Value.ToString("#,##0", VnCulture);

        var formatted = value.Value.ToString(
            $"#,##0.{new string('0', maxDecimals)}",
            VnCulture);

        // Show minimal decimals: 0 -> "0", 1.5 -> "1,5" (user types ",0" when needed)
        if (formatted.Contains(','))
            formatted = formatted.TrimEnd('0').TrimEnd(',');

        return formatted;
    }

    public static string FormatInt(int? value)
    {
        if (!value.HasValue)
            return string.Empty;

        return value.Value.ToString("#,##0", VnCulture);
    }

    public static decimal? ParseDecimal(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return null;

        var normalized = input.Trim()
            .Replace(".", string.Empty)
            .Replace(",", ".");

        return decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.InvariantCulture, out var result)
            ? result
            : null;
    }

    public static int? ParseInt(string? input)
    {
        var dec = ParseDecimal(input);
        return dec.HasValue ? (int)dec.Value : null;
    }

    public static string SanitizeInput(string? input)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;

        return Regex.Replace(input, @"[^\d,]", string.Empty);
    }

    public static string FormatWhileTyping(string? raw, int maxDecimals = 2, bool integerOnly = false)
    {
        var cleaned = SanitizeInput(raw);
        if (string.IsNullOrEmpty(cleaned))
            return string.Empty;

        var hasTrailingComma = cleaned.EndsWith(',');
        var parts = cleaned.Split(',', 2);
        var intDigits = parts[0];

        if (string.IsNullOrEmpty(intDigits))
            return hasTrailingComma && !integerOnly && maxDecimals > 0 ? "," : string.Empty;

        if (!ulong.TryParse(intDigits, out var intValue))
            intValue = 0;

        var formatted = intValue.ToString("#,##0", VnCulture);

        if (integerOnly)
            return formatted;

        if (parts.Length > 1)
        {
            var dec = parts[1];
            if (maxDecimals > 0 && dec.Length > maxDecimals)
                dec = dec[..maxDecimals];
            formatted += "," + dec;
        }
        else if (hasTrailingComma && maxDecimals > 0)
        {
            formatted += ",";
        }

        return formatted;
    }
}
