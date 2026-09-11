using System.Globalization;

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
        if (string.IsNullOrWhiteSpace(input))
            return null;

        var normalized = input.Trim()
            .Replace(".", string.Empty)
            .Replace(",", string.Empty);

        return int.TryParse(normalized, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result)
            ? result
            : null;
    }

    public static string SanitizeInput(string? input)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;

        var trimmed = input.Trim();
        var hasLeadingMinus = trimmed.StartsWith('-');
        var hasComma = false;
        var sanitized = new System.Text.StringBuilder(trimmed.Length);

        foreach (var character in trimmed)
        {
            if (char.IsDigit(character))
            {
                sanitized.Append(character);
            }
            else if (character == ',' && !hasComma)
            {
                hasComma = true;
                sanitized.Append(character);
            }
        }

        return hasLeadingMinus ? $"-{sanitized}" : sanitized.ToString();
    }

    public static string FormatWhileTyping(string? raw, int maxDecimals = 2, bool integerOnly = false)
    {
        var cleaned = SanitizeInput(raw);
        if (string.IsNullOrEmpty(cleaned))
            return string.Empty;

        var hasTrailingComma = cleaned.EndsWith(',');
        var parts = cleaned.Split(',', 2);
        var isNegative = parts[0].StartsWith('-');
        var intDigits = isNegative ? parts[0][1..] : parts[0];

        if (string.IsNullOrEmpty(intDigits))
        {
            if (isNegative)
                return hasTrailingComma && !integerOnly && maxDecimals > 0 ? "-," : "-";

            return hasTrailingComma && !integerOnly && maxDecimals > 0 ? "," : string.Empty;
        }

        if (!decimal.TryParse(intDigits, NumberStyles.None, CultureInfo.InvariantCulture, out var intValue))
            intValue = 0;

        var formatted = intValue.ToString("#,##0", VnCulture);
        if (isNegative)
            formatted = $"-{formatted}";

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
