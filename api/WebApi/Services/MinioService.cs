using System.Reactive.Linq;
using Contract;
using Contract.Files;
using Minio.ApiEndpoints;
using Minio.DataModel.Args;
using Minio.Exceptions;

namespace WebApi.Services;

using Minio;
using Minio.DataModel;
using Microsoft.Extensions.Options;

public class MinioSettings
{
    public string Endpoint { get; set; } = string.Empty;
    public int Port { get; set; } = 0;
    public string AccessKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public string BucketName { get; set; } = string.Empty;
}

public interface IMinioService
{
    Task<ApiResponseBase<bool>> UploadFileToFolderAsync(string folderName,string fileName, Stream fileStream, string contentType);
    Task<ApiResponseBase<string>> UploadFileToFolderGetUrlAsync(string folderName,string fileName, Stream fileStream, string contentType);
    Task<ApiResponseBase<List<FileInfoDto>>> ListFilesInFolderAsync(string folderName);
    Task<ApiResponseBase<string>> GetPresignedUrlAsync(string objectName, int expiryInSeconds = 3600);
    Task<ApiResponseBase<string>> GetPresignedPutUrlAsync(string objectName, int expiryInSeconds = 3600);
    Task<ApiResponseBase<string>> GetFileUrlAsync(string objectName, int expiryInSeconds = 3600);
    Task<ApiResponseBase<Stream>> DownloadFileAsync(string fileName);
    Task<ApiResponseBase<List<string>>> ListFoldersAsync(string folderPrefix = "");
}

public class MinioService
{
    private readonly IMinioClient _minioClient;
    private readonly MinioSettings _settings;

    public MinioService(IOptions<MinioSettings> settings)
    {
        _settings = settings.Value;

        _minioClient = new MinioClient()
            .WithEndpoint(_settings.Endpoint)
            .WithCredentials(_settings.AccessKey, _settings.SecretKey)
            .WithSSL()
            .Build();
    }

    public async Task<ApiResponseBase<string>> UploadFileToFolderGetUrlAsync(string folderName,string fileName, Stream fileStream, string contentType)
    {
        ApiResponseBase<string> response = new ApiResponseBase<string>();
        try
        {
            var objectName = $"{folderName.TrimEnd('/')}/{fileName}";

            bool found = await _minioClient.BucketExistsAsync(new BucketExistsArgs().WithBucket(_settings.BucketName));
            if (!found)
            {
                await _minioClient.MakeBucketAsync(new MakeBucketArgs().WithBucket(_settings.BucketName));
            }

            await _minioClient.PutObjectAsync(new PutObjectArgs()
                .WithBucket(_settings.BucketName)
                .WithObject(objectName)
                .WithStreamData(fileStream)
                .WithObjectSize(fileStream.Length)
                .WithContentType(contentType));

            var url = await _minioClient.PresignedGetObjectAsync(new PresignedGetObjectArgs()
            .WithBucket(_settings.BucketName)
            .WithObject(objectName)
            .WithExpiry(60 * 10)); // URL có hiệu lực trong 10 phút

            response.Data = url;
        }
        catch (Exception ex)
        {
            response.Data = string.Empty;
            response.Message = ex.Message;
        }

        return response;
    }


    public async Task<ApiResponseBase<bool>> UploadFileToFolderAsync(string folderName,string fileName, Stream fileStream, string contentType)
    {
        ApiResponseBase<bool> response = new ApiResponseBase<bool>();
        try
        {
            var objectName = $"{folderName.TrimEnd('/')}/{fileName}";

            bool found = await _minioClient.BucketExistsAsync(new BucketExistsArgs().WithBucket(_settings.BucketName));
            if (!found)
            {
                await _minioClient.MakeBucketAsync(new MakeBucketArgs().WithBucket(_settings.BucketName));
            }

            await _minioClient.PutObjectAsync(new PutObjectArgs()
                .WithBucket(_settings.BucketName)
                .WithObject(objectName)
                .WithStreamData(fileStream)
                .WithObjectSize(fileStream.Length)
                .WithContentType(contentType));

            response.Data = true;
        }
        catch (Exception ex)
        {
            response.Data = false;
            response.Message = ex.Message;
        }

        return response;
    }
    public async Task<ApiResponseBase<List<FileInfoDto>>> ListFilesInFolderAsync(string folderName)
    {
        ApiResponseBase<List<FileInfoDto>> response = new ApiResponseBase<List<FileInfoDto>>();
        var objects = new List<FileInfoDto>();
        try
        {
            if(folderName == "All")
            {
                folderName = "";
            }else
            {
                folderName = folderName.TrimEnd('/') + "/";
            }
            IObservable<Item> observable = _minioClient.ListObjectsAsync(new ListObjectsArgs()
            .WithBucket(_settings.BucketName)
            .WithPrefix(folderName)
            .WithRecursive(true));

            await observable.ForEachAsync(async (item)  =>
            {
                var url = await _minioClient.PresignedGetObjectAsync(new PresignedGetObjectArgs()
                .WithBucket(_settings.BucketName)
                .WithObject(item.Key)
                .WithExpiry(60 * 60)); // URL có hiệu lực trong 1 giờ

                objects.Add(new FileInfoDto
                {
                    Name = item.Key,
                    Size = item.Size,
                    Url = url
                });
            });
            response.Data = objects;
        }
        catch (Exception ex)
        {
            response.Message = ex.Message;
        }
        return response;
    }

    public async Task<ApiResponseBase<string>> GetPresignedUrlAsync(string objectName, int expiryInSeconds = 3600)
    {
        ApiResponseBase<string> response = new ApiResponseBase<string>();
        try
        {
            var url = await _minioClient.PresignedGetObjectAsync(new PresignedGetObjectArgs()
            .WithBucket(_settings.BucketName)
            .WithObject(objectName)
            .WithExpiry(expiryInSeconds));
            response.Data = url;
        }
        catch (Exception ex)
        {
            response.Message = ex.Message;
        }
        return response;

    }


    public async Task<ApiResponseBase<string>> GetFileUrlAsync(string objectName, int expiryInSeconds = 3600)
    {
        ApiResponseBase<string> response = new ApiResponseBase<string>();
        try
        {
            var url = await _minioClient.PresignedGetObjectAsync(new PresignedGetObjectArgs()
            .WithBucket(_settings.BucketName)
            .WithObject(objectName)
            .WithExpiry(expiryInSeconds)); // URL có hiệu lực trong 10 phút
            response.Data = url;
        }
        catch (Exception ex)
        {
            response.Message = ex.Message;
        }
        return response;
    }

    public async Task<ApiResponseBase<List<string>>> ListFoldersAsync(string folderPrefix = "")
    {
        ApiResponseBase<List<string>> response = new ApiResponseBase<List<string>>();
        var folders = new List<string>();
        try
        {
            if(string.IsNullOrEmpty(folderPrefix) || folderPrefix == "All")
            {
                return await ListTopLevelFoldersAsync();
            }


            var observable = _minioClient.ListObjectsAsync(
                new ListObjectsArgs()
                    .WithBucket(_settings.BucketName)
                    .WithPrefix(folderPrefix)
                    .WithRecursive(true)
            );

            await observable.ForEachAsync(item =>
            {
                var remaining = item.Key.Substring(folderPrefix.Length);

                if (remaining.Contains("/"))
                {
                    var subfolder = remaining.Substring(0, remaining.IndexOf('/') + 1);
                    folders.Add(folderPrefix + subfolder);
                }
            });
            response.Data = folders;
        }
        catch (Exception ex)
        {
            response.Message = ex.Message;
        }
        return response;
    }
    private async Task<ApiResponseBase<List<string>>> ListTopLevelFoldersAsync()
    {
        ApiResponseBase<List<string>> response = new ApiResponseBase<List<string>>();
        var prefixes = new HashSet<string>();
        try
        {
            var observable = _minioClient.ListObjectsAsync(
            new ListObjectsArgs()
                .WithBucket(_settings.BucketName)
                .WithRecursive(true) // phải lấy toàn bộ object để xử lý
            );

            await observable.ForEachAsync(item =>
            {
                if (item.Key.Contains("/"))
                {
                    // Lấy phần trước dấu '/' đầu tiên → folder cấp 1
                    var prefix = item.Key.Substring(0, item.Key.IndexOf('/') + 1);
                    prefixes.Add(prefix);
                }
            });

            response.Data = prefixes.ToList();
        }
        catch (Exception ex)
        {
            response.Message = ex.Message;
        }
        
        return response;
    }
    public async Task<ApiResponseBase<string>> GetPresignedPutUrlAsync(string objectName, int expiryInSeconds = 3600)
    {
        ApiResponseBase<string> response = new ApiResponseBase<string>();
        try
        {
            var url = await _minioClient.PresignedPutObjectAsync(new PresignedPutObjectArgs()
            .WithBucket(_settings.BucketName)
            .WithObject(objectName)
            .WithExpiry(expiryInSeconds));

            response.Data = url;
        }
        catch (Exception ex)
        {
            response.Message = ex.Message;
        }
        return response;
    }

    public async Task<ApiResponseBase<Stream>> DownloadFileAsync(string fileName)
    {
        ApiResponseBase<Stream> response = new ApiResponseBase<Stream>();
        try
        {
            var stream = new MemoryStream();

        await _minioClient.GetObjectAsync(new GetObjectArgs()
            .WithBucket(_settings.BucketName)
            .WithObject(fileName)
            .WithCallbackStream(s => s.CopyTo(stream)));

            stream.Seek(0, SeekOrigin.Begin);
            response.Data = stream;
        }
        catch (Exception ex)
        {
            response.Message = ex.Message;
        }
        return response;
    }


    public async Task<ApiResponseBase<bool>> DeleteFileAsync(string objectName)
    {
        ApiResponseBase<bool> response = new ApiResponseBase<bool>();
        try
        {
            await _minioClient.RemoveObjectAsync(new RemoveObjectArgs()
                .WithBucket(_settings.BucketName)
                .WithObject(objectName));
            response.Data = true;
        }
        catch (Exception ex)
        {
            response.Data = false;
            response.Message = ex.Message;
        }
        return response;
    }

}
