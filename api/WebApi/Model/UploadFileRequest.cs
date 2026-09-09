using Microsoft.AspNetCore.Http;

namespace WebApi.Model
{
    public class UploadFileRequest
    {
        // IFormFile must be bound from form-data for Swagger to generate correctly
        public IFormFile File { get; set; }

        // Additional fields should also be part of form-data
        public string Folder { get; set; }
    }
}


