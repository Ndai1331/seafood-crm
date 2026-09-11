using System.Net;
using System.Security.Claims;
using Contract.Seafood;
using Core.Const;
using Core.Exceptions;
using Domain.Seafood;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.Authorization;
using WebApi.Services;
using Application.Seafood;

namespace WebApi.Controllers.Seafood;

[ApiController]
[Authorize]
[Route("api/seafood/documents")]
public class SeafoodDocumentController : ControllerBase
{
    private const long MaxFileSize = 25 * 1024 * 1024;
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf", ".jpg", ".jpeg", ".png", ".xls", ".xlsx", ".doc", ".docx"
    };
    private readonly SeafoodDocumentService _documents;
    private readonly MinioService _minio;

    public SeafoodDocumentController(SeafoodDocumentService documents, MinioService minio)
    {
        _documents = documents;
        _minio = minio;
    }

    [HttpGet("types")]
    [HasPermission(Permissions.MasterCatalog)]
    public Task<List<DocumentTypeDto>> Types([FromQuery] bool includeInactive = true) => _documents.ListTypesAsync(includeInactive);

    [HttpGet("types/page")]
    [HasPermission(Permissions.MasterCatalog)]
    public Task<SeafoodPagedResult<DocumentTypeDto>> TypesPage([FromQuery] bool includeInactive = true, [FromQuery] string? search = null, [FromQuery] int skip = 0, [FromQuery] int take = 20)
        => _documents.ListTypesPageAsync(includeInactive, search, skip, take);

    [HttpGet("types/options")]
    [HasPermission(Permissions.MasterCatalog)]
    public Task<SeafoodSelect2SearchResponseDto> TypeOptions([FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        => _documents.SearchTypesAsync(search, page, pageSize);

    [HttpPost("types")]
    [HasPermission(Permissions.MasterCatalog)]
    public Task<DocumentTypeDto> SaveType([FromBody] DocumentTypeDto dto) => _documents.SaveTypeAsync(dto);

    [HttpDelete("types/{id:int}")]
    [HasPermission(Permissions.MasterCatalog)]
    public Task<bool> DeleteType(int id) => _documents.DeleteTypeAsync(id);

    [HttpGet]
    [HasPermission(Permissions.Inventory)]
    public async Task<List<DocumentAttachmentDto>> List([FromQuery] string ownerType, [FromQuery] int ownerId)
    {
        var rows = await _documents.ListAsync(ownerType, ownerId);
        foreach (var row in rows)
        {
            var key = await _documents.GetObjectKeyAsync(row.Id);
            var url = await _minio.GetPresignedUrlAsync(key, 900);
            row.DownloadUrl = url.Data;
        }
        return rows;
    }

    [HttpGet("checklist")]
    [HasPermission(Permissions.Inventory)]
    public Task<DocumentChecklistDto> Checklist([FromQuery] string ownerType, [FromQuery] int ownerId,
        [FromQuery] string marketCode, [FromQuery] DateTime? effectiveDate)
        => _documents.GetChecklistAsync(ownerType, ownerId, marketCode, effectiveDate);

    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(MaxFileSize)]
    [HasPermission(Permissions.Inventory)]
    public async Task<DocumentAttachmentDto> Upload([FromQuery] string ownerType, [FromQuery] int ownerId,
        [FromQuery] int? documentTypeId, [FromForm] IFormFile file)
    {
        if (file == null || file.Length == 0)
            throw new GlobalException("File chứng từ rỗng.", HttpStatusCode.BadRequest);
        if (file.Length > MaxFileSize)
            throw new GlobalException("File chứng từ vượt quá 25 MB.", HttpStatusCode.BadRequest);
        var extension = Path.GetExtension(file.FileName);
        if (!AllowedExtensions.Contains(extension))
            throw new GlobalException("Định dạng file chứng từ không được hỗ trợ.", HttpStatusCode.BadRequest);
        await _documents.EnsureOwnerExistsAsync(ownerType, ownerId);

        var fileName = $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
        var folder = $"seafood/{ownerType.Trim().ToLowerInvariant()}/{ownerId}";
        await using var stream = file.OpenReadStream();
        var result = await _minio.UploadFileToFolderAsync(folder, fileName, stream, ContentType(extension));
        if (!result.IsSuccess || result.Data != true)
            throw new GlobalException($"Không thể lưu file chứng từ: {result.Message}", HttpStatusCode.BadGateway);

        var saved = await _documents.SaveMetadataAsync(new DocumentAttachment
        {
            OwnerType = ownerType,
            OwnerId = ownerId,
            DocumentTypeId = documentTypeId,
            ObjectKey = $"{folder}/{fileName}",
            FileName = Path.GetFileName(file.FileName),
            ContentType = ContentType(extension),
            FileSize = file.Length,
            UploadedByUserId = CurrentUserId(),
            Status = DocumentStatus.Pending
        });
        var url = await _minio.GetPresignedUrlAsync($"{folder}/{fileName}", 900);
        saved.DownloadUrl = url.Data;
        return saved;
    }

    [HttpPost("{id:long}/verify")]
    [HasPermission(Permissions.Inventory)]
    public Task<DocumentAttachmentDto> Verify(long id, [FromBody] DocumentVerificationRequest request)
        => _documents.VerifyAsync(id, request.Verified, CurrentUserId(), request.Note);

    private int? CurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.PrimarySid) ?? User.FindFirst("sub") ?? User.FindFirst("userId");
        return int.TryParse(claim?.Value, out var id) ? id : null;
    }

    private static string ContentType(string extension) => extension.ToLowerInvariant() switch
    {
        ".pdf" => "application/pdf",
        ".jpg" or ".jpeg" => "image/jpeg",
        ".png" => "image/png",
        ".xls" => "application/vnd.ms-excel",
        ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        ".doc" => "application/msword",
        ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        _ => "application/octet-stream"
    };
}

public class DocumentVerificationRequest
{
    public bool Verified { get; set; }
    public string? Note { get; set; }
}
