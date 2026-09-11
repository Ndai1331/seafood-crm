using System.Net;
using System.Security.Claims;
using Application.Seafood;
using Contract.Seafood;
using Core.Const;
using Core.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.Authorization;
using WebApi.Services;

namespace WebApi.Controllers.Seafood;

[ApiController]
[Authorize]
[Route("api/seafood/import")]
public class SeafoodImportController : ControllerBase
{
    private const long MaxFileSize = 25 * 1024 * 1024;
    private readonly SeafoodImportService _imports;
    private readonly MinioService _minio;

    public SeafoodImportController(SeafoodImportService imports, MinioService minio)
    {
        _imports = imports;
        _minio = minio;
    }

    [HttpPost("preview")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(MaxFileSize)]
    [HasPermission(Permissions.Inbound)]
    public async Task<ImportPreviewDto> Preview([FromForm] IFormFile file)
    {
        if (file == null || file.Length == 0 || !Path.GetExtension(file.FileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
            throw new GlobalException("Chỉ hỗ trợ file .xlsx không rỗng.", HttpStatusCode.BadRequest);
        await using var input = file.OpenReadStream();
        using var buffer = new MemoryStream();
        await input.CopyToAsync(buffer);
        var folder = $"seafood/import/{DateTime.UtcNow:yyyy/MM}";
        var fileName = $"{Guid.NewGuid():N}.xlsx";
        var objectKey = $"{folder}/{fileName}";
        buffer.Position = 0;
        var upload = await _minio.UploadFileToFolderAsync(folder, fileName, buffer, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        if (!upload.IsSuccess || upload.Data != true)
            throw new GlobalException($"Không thể lưu file import: {upload.Message}", HttpStatusCode.BadGateway);
        buffer.Position = 0;
        return await _imports.PreviewAsync(buffer, file.FileName, objectKey, CurrentUserId());
    }

    [HttpGet("{id:int}")]
    [HasPermission(Permissions.Inbound)]
    public Task<ImportPreviewDto> Get(int id) => _imports.GetAsync(id);

    [HttpPost("{id:int}/confirm")]
    [HasPermission(Permissions.Inbound)]
    public Task<ImportPreviewDto> Confirm(int id, [FromBody] ImportConfirmDto request)
        => _imports.ConfirmAsync(id, request, CurrentUserId());

    private int? CurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.PrimarySid) ?? User.FindFirst("sub") ?? User.FindFirst("userId");
        return int.TryParse(claim?.Value, out var id) ? id : null;
    }
}
