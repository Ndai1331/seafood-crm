using System.Text.Json;
using System.Text.Json.Serialization;

namespace BootstrapBlazor.Server.Data;

/// <summary>
/// IC SEO Resource Daily Check DTO
/// </summary>
public class IcSeoResourceDailyCheckDto
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("checkDate")]
    public DateTime CheckDate { get; set; }

    [JsonPropertyName("pic")]
    public string? Pic { get; set; }

    [JsonPropertyName("costTypeName")]
    public string? costTypeName { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("resourceUrl")]
    public string? ResourceUrl { get; set; }

    [JsonPropertyName("domain")]
    public string? Domain { get; set; }

    [JsonPropertyName("errorMessage")]
    public string? ErrorMessage { get; set; }

    [JsonPropertyName("createdAt")]
    public DateTime? CreatedAt { get; set; }

    [JsonPropertyName("updatedAt")]
    public DateTime? UpdatedAt { get; set; }

    [JsonPropertyName("costTypeId")]
    public int? CostTypeId { get; set; }
    [JsonPropertyName("linkDetail")]
    public string? LinkDetail { get; set; }
    [JsonPropertyName("anchor")]
    public string? Anchor { get; set; }
    [JsonPropertyName("linkAnchor")]
    public string? LinkAnchor { get; set; }
    [JsonPropertyName("orderDate")]
    public DateTime? OrderDate { get; set; }
    [JsonPropertyName("expiryDate")]
    public DateTime? ExpiryDate { get; set; }
    [JsonPropertyName("os")]
    public string? Os { get; set; }
}

/// <summary>
/// Filter DTO for IC SEO Resource Daily Check GetList
/// </summary>
public class IcSeoResourceDailyCheckFilterDto : FilterPagingBase
{
    [JsonPropertyName("fromDate")]
    public DateTime? FromDate { get; set; }

    [JsonPropertyName("toDate")]
    public DateTime? ToDate { get; set; }

    [JsonPropertyName("pic")]
    public string? Pic { get; set; }

    [JsonPropertyName("costType")]
    public int? CostTypeId { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("domain")]
    public string? Domain { get; set; }

    [JsonPropertyName("filterIsLive")]
    public int? FilterIsLive { get; set; }

    [JsonPropertyName("filterIsMobile")]
    public int? FilterIsMobile { get; set; }

    [JsonPropertyName("filterIsPC")]
    public int? FilterIsPC { get; set; }

    [JsonPropertyName("filterIsProxy")]
    public int? FilterIsProxy { get; set; }
}

/// <summary>
/// Error Statistics Filter DTO
/// </summary>
public class ErrorStatisticsFilterDto
{
    [JsonPropertyName("fromDate")]
    public DateTime? FromDate { get; set; }

    [JsonPropertyName("toDate")]
    public DateTime? ToDate { get; set; }
}

/// <summary>
/// Error Statistics Response DTO - Statistics grouped by PIC and CostType
/// </summary>
public class ErrorStatisticsResponseDto
{
    [JsonPropertyName("statisticsByPic")]
    public List<ErrorStatisticsByPicDto>? StatisticsByPic { get; set; }

    [JsonPropertyName("statisticsByCostType")]
    public List<ErrorStatisticsByOsDto>? StatisticsByOs { get; set; }
}

/// <summary>
/// Error Statistics by PIC
/// </summary>
public class ErrorStatisticsByPicDto
{
    [JsonPropertyName("pic")]
    public string? Pic { get; set; }

    [JsonPropertyName("errorCount")]
    public int ErrorCount { get; set; }

    [JsonPropertyName("costTypeBreakdown")]
    public List<ErrorStatisticsCostTypeBreakdownDto>? CostTypeBreakdown { get; set; }
}

/// <summary>
/// Error Statistics Cost Type Breakdown DTO
/// </summary>
public class ErrorStatisticsCostTypeBreakdownDto
{
    [JsonPropertyName("costTypeId")]
    public int CostTypeId { get; set; }

    [JsonPropertyName("costTypeCode")]
    public string? CostTypeCode { get; set; }

    [JsonPropertyName("costTypeName")]
    public string? CostTypeName { get; set; }

    [JsonPropertyName("errorCount")]
    public int ErrorCount { get; set; }
}

/// <summary>
/// Error Statistics by Cost Type
/// </summary>
public class ErrorStatisticsByOsDto
{

    [JsonPropertyName("os")]
    public string? Os { get; set; }

    [JsonPropertyName("errorCount")]
    public int ErrorCount { get; set; }

    [JsonPropertyName("picBreakdown")]
    public List<ErrorStatisticsCostTypeBreakdownDto>? CostTypeBreakdown { get; set; }
}


/// <summary>
/// Create/Update DTO (for reference, not used in current implementation)
/// </summary>
public class CreateUpdateIcSeoResourceDailyCheckDto
{
    [JsonPropertyName("checkDate")]
    public DateTime CheckDate { get; set; }

    [JsonPropertyName("pic")]
    public string? Pic { get; set; }

    [JsonPropertyName("costType")]
    public string? CostType { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("resourceUrl")]
    public string? ResourceUrl { get; set; }

    [JsonPropertyName("domain")]
    public string? Domain { get; set; }

    [JsonPropertyName("errorMessage")]
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Import IC SEO Resource Request DTO
/// </summary>
public class ImportIcSeoResourceRequest
{
    [JsonPropertyName("excelBytes")]
    public byte[]? ExcelBytes { get; set; }
    
    [JsonPropertyName("fileName")]
    public string? FileName { get; set; }
    
    [JsonPropertyName("checkDate")]
    public DateTime? CheckDate { get; set; } // Optional: nếu null thì dùng ngày hiện tại
    
    /// <summary>
    /// JSON data from n8n (after AI validation and parsing)
    /// </summary>
    [JsonPropertyName("resources")]
    public List<IcSeoResourceImportItem>? Resources { get; set; }
}

/// <summary>
/// IC SEO Resource Import Item (parsed from Excel by n8n AI)
/// </summary>
public class IcSeoResourceImportItem
{
    [JsonPropertyName("pic")]
    public string? Pic { get; set; }
    
    [JsonPropertyName("costTypeId")]
    public int? CostTypeId { get; set; }
    
    [JsonPropertyName("costTypeName")]
    public string? CostTypeName { get; set; }
    
    [JsonPropertyName("resourceUrl")]
    public string? ResourceUrl { get; set; }
    
    [JsonPropertyName("domain")]
    public string? Domain { get; set; }
    
    [JsonPropertyName("linkDetail")]
    public string? LinkDetail { get; set; }
    
    [JsonPropertyName("anchor")]
    public string? Anchor { get; set; }
    
    [JsonPropertyName("linkAnchor")]
    public string? LinkAnchor { get; set; }
    
    [JsonPropertyName("orderDate")]
    public DateTime? OrderDate { get; set; }
    
    [JsonPropertyName("expiryDate")]
    public DateTime? ExpiryDate { get; set; }
    
    [JsonPropertyName("os")]
    public string? Os { get; set; }
    
    /// <summary>
    /// Row number in Excel (for error reporting)
    /// </summary>
    [JsonPropertyName("rowNumber")]
    public int RowNumber { get; set; }
    
    /// <summary>
    /// Validation errors for this row
    /// </summary>
    [JsonPropertyName("validationErrors")]
    public List<string>? ValidationErrors { get; set; }
}

/// <summary>
/// Import IC SEO Resource Response DTO
/// </summary>
public class ImportIcSeoResourceResponse
{
    [JsonPropertyName("totalRows")]
    public int TotalRows { get; set; }
    
    [JsonPropertyName("successCount")]
    public int SuccessCount { get; set; }
    
    [JsonPropertyName("errorCount")]
    public int ErrorCount { get; set; }
    
    [JsonPropertyName("errors")]
    public List<string>? Errors { get; set; }
    
    [JsonPropertyName("importedIds")]
    public List<long>? ImportedIds { get; set; }
}

/// <summary>
/// N8n WebHook Request DTO for Excel validation and import
/// </summary>
public class N8nExcelValidationRequest
{
    [JsonPropertyName("fileName")]
    public string? FileName { get; set; }
    
    [JsonPropertyName("fileBase64")]
    public string? FileBase64 { get; set; }
    
    [JsonPropertyName("checkDate")]
    public DateTime? CheckDate { get; set; }
    
    [JsonPropertyName("expectedColumns")]
    public List<string>? ExpectedColumns { get; set; }
    
    /// <summary>
    /// PIC (Person In Charge) - will be applied to all imported rows
    /// </summary>
    [JsonPropertyName("pic")]
    public string? Pic { get; set; }
    
    /// <summary>
    /// If true, n8n will insert data into DB directly. If false, only validate and return data.
    /// </summary>
    [JsonPropertyName("autoInsert")]
    public bool AutoInsert { get; set; } = false;
}

/// <summary>
/// N8n WebHook Response DTO (includes validation and insert results)
/// </summary>
public class N8nExcelValidationResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }
    
    [JsonPropertyName("message")]
    public string? Message { get; set; }
    
    [JsonPropertyName("totalRows")]
    public int TotalRows { get; set; }

    [JsonPropertyName("validRows")]
    public int ValidRows { get; set; }

    [JsonPropertyName("invalidRows")]
    [JsonConverter(typeof(InvalidRowsConverter))]
    public int InvalidRows { get; set; }

    /// <summary>
    /// Invalid rows detail (when n8n returns array instead of count)
    /// </summary>
    [JsonPropertyName("invalidRowsDetail")]
    public List<object>? InvalidRowsDetail { get; set; }

    /// <summary>
    /// Resources data (before insert, for preview)
    /// </summary>
    [JsonPropertyName("resources")]
    public List<IcSeoResourceImportItem>? Resources { get; set; }

    [JsonPropertyName("validationErrors")]
    public List<string>? ValidationErrors { get; set; }
    
    [JsonPropertyName("missingColumns")]
    public List<string>? MissingColumns { get; set; }
    
    [JsonPropertyName("detectedColumns")]
    public List<string>? DetectedColumns { get; set; }
    
    /// <summary>
    /// Insert results (after n8n inserts into DB)
    /// </summary>
    [JsonPropertyName("insertResult")]
    public N8nInsertResult? InsertResult { get; set; }
}

/// <summary>
/// N8n Insert Result DTO (result after n8n inserts into database)
/// </summary>
public class N8nInsertResult
{
    [JsonPropertyName("successCount")]
    public int SuccessCount { get; set; }
    
    [JsonPropertyName("errorCount")]
    public int ErrorCount { get; set; }
    
    [JsonPropertyName("insertedIds")]
    [JsonConverter(typeof(InsertedIdsConverter))]
    public List<long>? InsertedIds { get; set; }
    
    [JsonPropertyName("errors")]
    public List<string>? Errors { get; set; }
    
    [JsonPropertyName("insertedAt")]
    public DateTime? InsertedAt { get; set; }
}

/// <summary>
/// Custom JSON converter to handle insertedIds as either numbers or strings
/// </summary>
public class InsertedIdsConverter : JsonConverter<List<long>?>
{
    public override List<long>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        if (reader.TokenType != JsonTokenType.StartArray)
        {
            throw new JsonException("Expected array");
        }

        var list = new List<long>();
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndArray)
            {
                return list;
            }

            if (reader.TokenType == JsonTokenType.Number)
            {
                list.Add(reader.GetInt64());
            }
            else if (reader.TokenType == JsonTokenType.String)
            {
                var stringValue = reader.GetString();
                if (string.IsNullOrEmpty(stringValue))
                {
                    continue;
                }
                if (long.TryParse(stringValue, out var longValue))
                {
                    list.Add(longValue);
                }
            }
            else if (reader.TokenType == JsonTokenType.Null)
            {
                // Skip null values
                continue;
            }
        }

        throw new JsonException("Unexpected end of array");
    }

    public override void Write(Utf8JsonWriter writer, List<long>? value, JsonSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNullValue();
            return;
        }

        writer.WriteStartArray();
        foreach (var id in value)
        {
            writer.WriteNumberValue(id);
        }
        writer.WriteEndArray();
    }
}

/// <summary>
/// N8n WebHook Request DTO for Grid data validation and import
/// </summary>
public class N8nGridDataRequest
{
    [JsonPropertyName("checkDate")]
    public DateTime? CheckDate { get; set; }
    
    [JsonPropertyName("pic")]
    public string? Pic { get; set; }

    [JsonPropertyName("os")]
    public string? Os { get; set; }
    
    /// <summary>
    /// Resource Type ID: 3 = Textlink (TL), 4 = Guestpost (GP)
    /// </summary>
    [JsonPropertyName("resourceTypeId")]
    public int? ResourceTypeId { get; set; }

    /// <summary>
    /// Order Date (Ngày đặt) - applies to all rows if provided
    /// </summary>
    [JsonPropertyName("orderDate")]
    public DateTime? OrderDate { get; set; }

    /// <summary>
    /// Expiry Date (Ngày hết hạn) - applies to all rows if provided
    /// </summary>
    [JsonPropertyName("expiryDate")]
    public DateTime? ExpiryDate { get; set; }

    /// <summary>
    /// Grid data as JSON objects (array of dictionaries with column names as keys)
    /// </summary>
    [JsonPropertyName("data")]
    public List<Dictionary<string, object?>>? Data { get; set; }
    
    /// <summary>
    /// If true, n8n will insert data into DB directly. If false, only validate and return data.
    /// </summary>
    [JsonPropertyName("autoInsert")]
    public bool AutoInsert { get; set; } = true;
}

/// <summary>
/// Custom JSON converter to handle invalidRows as either int or array
/// N8n may return "invalidRows": 5 (int) or "invalidRows": [{...}] (array)
/// </summary>
public class InvalidRowsConverter : JsonConverter<int>
{
    public override int Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Number)
        {
            return reader.GetInt32();
        }

        if (reader.TokenType == JsonTokenType.StartArray)
        {
            // Count array elements
            int count = 0;
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndArray)
                {
                    return count;
                }

                count++;
                // Skip the value (could be object, string, number, etc.)
                if (reader.TokenType == JsonTokenType.StartObject)
                {
                    reader.Skip();
                }
            }
            return count;
        }

        if (reader.TokenType == JsonTokenType.Null)
        {
            return 0;
        }

        if (reader.TokenType == JsonTokenType.String)
        {
            var str = reader.GetString();
            if (int.TryParse(str, out var val)) return val;
            return 0;
        }

        return 0;
    }

    public override void Write(Utf8JsonWriter writer, int value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(value);
    }
}

