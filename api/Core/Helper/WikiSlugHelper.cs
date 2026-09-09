using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Core.Helper
{
    /// <summary>
    /// Slug generation for wiki notes. Vietnamese-aware: strips diacritics, maps đ→d,
    /// lowercases, collapses non-alphanumerics to single hyphens. Pure + deterministic
    /// so the same title always yields the same base slug (links stay resolvable).
    /// Lives in Core so both SqlServ4r (repository) and Application (service) can reach it.
    /// </summary>
    public static class WikiSlugHelper
    {
        public static string ToSlug(string? input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;

            // đ/Đ have no decomposed form — map before NFD.
            var src = input.Replace('Đ', 'D').Replace('đ', 'd');

            var normalized = src.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder(normalized.Length);
            foreach (var ch in normalized)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(ch) == UnicodeCategory.NonSpacingMark)
                    continue; // drop combining accents
                sb.Append(ch);
            }

            var ascii = sb.ToString().Normalize(NormalizationForm.FormC).ToLowerInvariant();

            // Replace any run of non [a-z0-9] with a single hyphen, then trim hyphens.
            ascii = Regex.Replace(ascii, "[^a-z0-9]+", "-");
            ascii = ascii.Trim('-');

            return ascii;
        }
    }
}
