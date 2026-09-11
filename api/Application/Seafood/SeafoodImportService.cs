using System.Net;
using Contract.Seafood;
using Core.Exceptions;
using Domain.Seafood;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using OfficeOpenXml;
using SqlServ4r.EntityFramework;
using Volo.Abp.DependencyInjection;

namespace Application.Seafood;

public class SeafoodImportService : ITransientDependency
{
    private static readonly string[] SupportedSheets =
    {
        "Theo dõi hàng nhập hằng ngày", "đưa vào sản xuất", "Xuất khẩu - đơn hàng", "Xuất khẩu -PAYMENT TRACKING", "Xuất khẩu _ DEPOSIT"
    };
    private readonly DreamContext _db;

    public SeafoodImportService(DreamContext db) => _db = db;

    public async Task<ImportPreviewDto> PreviewAsync(Stream stream, string fileName, string? sourceObjectKey, int? userId)
    {
        if (stream.Length == 0 || stream.Length > 25 * 1024 * 1024)
            throw new GlobalException("File Excel phải lớn hơn 0 và không vượt quá 25 MB.", HttpStatusCode.BadRequest);
        ExcelPackage.License.SetNonCommercialOrganization("Seafood CRM");
        using var package = new ExcelPackage(stream);
        var batch = new ImportBatch { FileName = Path.GetFileName(fileName), SourceObjectKey = sourceObjectKey, CreatedByUserId = userId };
        var seenKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var sheet in package.Workbook.Worksheets.Where(x => SupportedSheets.Contains(x.Name, StringComparer.OrdinalIgnoreCase)))
        {
            if (sheet.Dimension == null) continue;
            var headerRow = FindHeaderRow(sheet);
            if (headerRow == 0) continue;
            var headers = Enumerable.Range(1, sheet.Dimension.Columns)
                .Select(c => (Column: c, Name: sheet.Cells[headerRow, c].Text.Trim()))
                .ToDictionary(x => x.Column, x => string.IsNullOrWhiteSpace(x.Name) ? $"Column{x.Column}" : x.Name);
            for (var row = headerRow + 1; row <= sheet.Dimension.End.Row; row++)
            {
                var values = headers.ToDictionary(x => x.Value, x => sheet.Cells[row, x.Key].Text.Trim());
                if (values.Values.All(string.IsNullOrWhiteSpace)) continue;
                var error = ValidateRow(sheet.Name, values, seenKeys);
                batch.Rows.Add(new ImportStagingRow
                {
                    SheetName = sheet.Name,
                    RowNumber = row,
                    PayloadJson = JsonConvert.SerializeObject(values),
                    IsValid = string.IsNullOrWhiteSpace(error),
                    ErrorMessage = error
                });
            }
        }
        if (batch.Rows.Count == 0)
            throw new GlobalException("Không tìm thấy sheet hoặc dòng dữ liệu được hỗ trợ trong file Excel.", HttpStatusCode.BadRequest);
        batch.TotalRows = batch.Rows.Count;
        batch.ValidRows = batch.Rows.Count(x => x.IsValid);
        batch.ErrorRows = batch.Rows.Count(x => !x.IsValid);
        _db.ImportBatches.Add(batch);
        _db.DomainAuditLogs.Add(new DomainAuditLog { EntityType = "ImportBatch", Action = "Previewed", UserId = userId, Reason = $"{batch.FileName}: {batch.TotalRows} dòng" });
        await _db.SaveChangesAsync();
        return Map(batch);
    }

    public async Task<ImportPreviewDto> GetAsync(int id)
    {
        var batch = await _db.ImportBatches.Include(x => x.Rows).FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new GlobalException("Không tìm thấy batch import.", HttpStatusCode.NotFound);
        return Map(batch);
    }

    public async Task<ImportPreviewDto> ConfirmAsync(int id, ImportConfirmDto request, int? userId)
    {
        var batch = await _db.ImportBatches.Include(x => x.Rows).FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new GlobalException("Không tìm thấy batch import.", HttpStatusCode.NotFound);
        if (!request.Confirm) throw new GlobalException("Chưa xác nhận import.", HttpStatusCode.BadRequest);
        if (batch.ErrorRows > 0)
            throw new GlobalException($"Batch còn {batch.ErrorRows} dòng lỗi; hãy sửa file hoặc loại bỏ dòng lỗi trước khi confirm.", HttpStatusCode.BadRequest);
        if (batch.Status != ImportBatchStatus.Preview)
            throw new GlobalException("Batch import đã được xử lý trước đó.", HttpStatusCode.Conflict);
        batch.Status = ImportBatchStatus.Confirmed;
        _db.DomainAuditLogs.Add(new DomainAuditLog { EntityType = "ImportBatch", EntityId = id, Action = "Confirmed", UserId = userId, Reason = request.Note });
        await _db.SaveChangesAsync();
        return Map(batch);
    }

    private static int FindHeaderRow(ExcelWorksheet sheet)
    {
        for (var row = sheet.Dimension!.Start.Row; row <= Math.Min(sheet.Dimension.End.Row, sheet.Dimension.Start.Row + 8); row++)
        {
            var nonEmpty = Enumerable.Range(sheet.Dimension.Start.Column, sheet.Dimension.Columns)
                .Count(column => !string.IsNullOrWhiteSpace(sheet.Cells[row, column].Text));
            if (nonEmpty >= 2) return row;
        }
        return 0;
    }

    private static string? ValidateRow(string sheetName, IReadOnlyDictionary<string, string> values, HashSet<string> seenKeys)
    {
        var normalized = values.ToDictionary(x => x.Key.ToUpperInvariant(), x => x.Value);
        var key = FirstValue(normalized, "BL", "BILL", "CONTAINER", "INVOICE", "MÃ LÔ", "LOT");
        if (!string.IsNullOrWhiteSpace(key))
        {
            var dedupeKey = $"{sheetName}|{key}";
            if (!seenKeys.Add(dedupeKey)) return "BL/container/invoice/mã lô bị trùng trong file staging.";
        }
        if (sheetName.Contains("nhập", StringComparison.OrdinalIgnoreCase) && !values.Values.Any(IsPositiveNumber))
            return "Dòng nhập chưa có khối lượng số để đối chiếu.";
        return null;
    }

    private static string? FirstValue(IReadOnlyDictionary<string, string> values, params string[] names)
        => values.FirstOrDefault(x => names.Any(name => x.Key.Contains(name, StringComparison.OrdinalIgnoreCase)) && !string.IsNullOrWhiteSpace(x.Value)).Value;

    private static bool IsPositiveNumber(string value) => decimal.TryParse(value.Replace(",", ""), out var number) && number > 0;

    private static ImportPreviewDto Map(ImportBatch batch) => new()
    {
        Id = batch.Id, FileName = batch.FileName, Status = batch.Status, TotalRows = batch.TotalRows,
        ValidRows = batch.ValidRows, ErrorRows = batch.ErrorRows, CreatedAt = batch.CreatedAt,
        Rows = batch.Rows.OrderBy(x => x.SheetName).ThenBy(x => x.RowNumber).Select(x => new ImportStagingRowDto
        {
            Id = x.Id, SheetName = x.SheetName, RowNumber = x.RowNumber, PayloadJson = x.PayloadJson,
            IsValid = x.IsValid, ErrorMessage = x.ErrorMessage
        }).ToList()
    };
}
