
using Contract;
using Contract.Files;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Minio.DataModel;
using WebApi.Model;
using WebApi.Services;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/file/")]
    [Authorize]
    public class FileController : ControllerBase
    {
        private readonly MinioService _minioService;
        public FileController(MinioService minioService)
        {
            _minioService = minioService;
        }

        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        public async Task<ApiResponseBase<bool>> Upload([FromForm] UploadFileRequest request)
        {
            using var stream = request.File.OpenReadStream();
            return await _minioService.UploadFileToFolderAsync(request.Folder, request.File.FileName, stream, request.File.ContentType);
        }

        [HttpPost("upload/banner-telegram")]
        [AllowAnonymous]
        public async Task<ApiResponseBase<string>> UploadBannerTelegram([FromBody] UploadImageRequest request)
        {
            var file = Convert.FromBase64String(request.Base64);
            using var stream = new MemoryStream(file);
            var response = await _minioService.UploadFileToFolderGetUrlAsync("banner-telegram", $"{request.FileName}", stream, request.FileExt);
            return response;
        }

        [HttpGet("list")]
        public async Task<ApiResponseBase<List<FileInfoDto>>> List([FromQuery] string folder)
        {
            if(folder == "All")
            {
                folder = string.Empty;
            }
            return await _minioService.ListFilesInFolderAsync(folder);
        }

        [HttpGet("upload-url")]
        public async Task<ApiResponseBase<string>> GetPresignedUploadUrl([FromQuery] string folder, [FromQuery] string fileName)
        {
            var objectName = $"{folder.TrimEnd('/')}/{fileName}";
            return await _minioService.GetPresignedPutUrlAsync(objectName);
        }

        [HttpGet("list-folders")]
        public async Task<ApiResponseBase<List<string>>> ListFolders([FromQuery] string folder)
        {
            return await _minioService.ListFoldersAsync(folder);
        }

        [HttpDelete("delete")]
        public async Task<ApiResponseBase<bool>> Delete([FromQuery] string fileName)
        {
            return await _minioService.DeleteFileAsync(fileName);
        }
    }
}