using System.Text.Json;

namespace BootstrapBlazor.Server.Services;

/// <summary>
/// Pulls the human sentence out of a failed API call.
///
/// RequestClient throws with the raw response body as the exception message, and its ResponseApi
/// shape expects a "message" property while our API sends "error" — so the friendly sentence never
/// gets unwrapped on its own and a fragment of JSON reaches the screen instead.
///
/// The parsing is done by the JSON reader rather than by cutting the string at quotes. Vietnamese
/// arrives escaped (á, đ…), and a substring lifted straight out of the body shows those
/// escapes verbatim: "Khoá này chưa..." is what the login screen displayed before
/// this existed.
/// </summary>
public static class ApiErrorMessage
{
    public static string Describe(Exception ex, string fallback = "Có lỗi xảy ra.")
    {
        var raw = ex.Message ?? string.Empty;

        var start = raw.IndexOf('{');
        var end = raw.LastIndexOf('}');
        if (start >= 0 && end > start)
        {
            try
            {
                using var document = JsonDocument.Parse(raw[start..(end + 1)]);
                foreach (var name in new[] { "error", "message", "title", "detail" })
                {
                    if (document.RootElement.TryGetProperty(name, out var value)
                        && value.ValueKind == JsonValueKind.String)
                    {
                        var text = value.GetString();
                        if (!string.IsNullOrWhiteSpace(text)) return text!;
                    }
                }
            }
            catch (JsonException)
            {
                // Not JSON after all — fall through to the raw text, which is still better than
                // swallowing whatever the server tried to say.
            }
        }

        return string.IsNullOrWhiteSpace(raw) ? fallback : raw;
    }
}
