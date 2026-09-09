using Microsoft.AspNetCore.Http;

namespace Contract.Uploads
{
    public interface IUploadService
    {
        public Task<FileDto> UploadImage(IFormFile file);

        public Task<FileDto> UploadExcelFileOfUsers(IFormFile file);

        public Task<FileDto> UploadDocumentFile(IFormFile file);

        public Task<List<int>> UploadImages(IFormFileCollection files);

        public Task<List<int>> UploadTaskFiles(IFormFileCollection files);

        public Task<FileDto> UploadAnyDocumentFile(IFormFile file);
        public Task<List<int>> UploadAnyDocumentFiles(IFormFileCollection files);
        public Task<List<FileDto>> UploadAnyFiles(IFormFileCollection files);
    }
}