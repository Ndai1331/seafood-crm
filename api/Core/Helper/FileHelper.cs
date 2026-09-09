using System.ComponentModel.DataAnnotations.Schema;
using System.Net;
using Core.Const;
using Core.Exceptions;
using Microsoft.AspNetCore.Http;

namespace Core.Helper
{
    public static class FileHelper
    {
        public static async Task<FileModel> UploadFile(
            IFormFile file,
            string basePath,
            List<string> ExtensionsConstraint,
            string baseUri = ""
        )
        {
            string directoryPath = "";
            var filePath = "";
            var fileDto = new FileModel();
            if (file.Length > 0)
            {
                var ext = Path.GetExtension(file.FileName);

                if (ExtensionsConstraint.Any() && !ExtensionsConstraint.Any(x => x.Contains(ext)))
                {
                    throw new GlobalException(
                        HttpMessage.InvalidExtension,
                        HttpStatusCode.BadRequest
                    );
                }

                var directory = $"{DateTime.Now:dd-MM-yyyy}";
                directoryPath = Path.GetFullPath(Path.Combine(basePath, directory));
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                var StorageFileName =
                    $"{DateTime.Now:yyyyMMddHHmmss}_" + file.FileName;
                filePath = Path.Combine(directoryPath, StorageFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                fileDto.Extension = ext;
                fileDto.Path = filePath;
                fileDto.Name = file.FileName;
                if (baseUri != "")
                {
                    fileDto.Url = baseUri + "/" + directory + "/" + StorageFileName;
                }
                return fileDto;
            }
            else
            {
                return fileDto;
            }
        }


        public static async Task<FileModel> UploadFileBytes(
                   byte[] bytes,
                   string fileName,
                   string basePath,
                   List<string> ExtensionsConstraint,
                   string baseUri = ""
               )
        {
            string directoryPath = "";
            var filePath = "";
            var fileDto = new FileModel();
            if (bytes.Length > 0)
            {
                var ext = Path.GetExtension(fileName);

                if (ExtensionsConstraint.Any() && !ExtensionsConstraint.Any(x => x.Contains(ext)))
                {
                    throw new GlobalException(
                        HttpMessage.InvalidExtension,
                        HttpStatusCode.BadRequest
                    );
                }

                var directory = $"{DateTime.Now:dd-MM-yyyy}";
                directoryPath = Path.GetFullPath(Path.Combine(basePath, directory));
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                var StorageFileName =
                    $"{Guid.NewGuid()}{DateTime.Now:yyyyMMddHHmmss}_" + fileName;
                filePath = Path.Combine(directoryPath, StorageFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await fileStream.WriteAsync(bytes, 0, bytes.Length);
                }

                fileDto.Extension = ext;
                fileDto.Path = filePath;
                fileDto.Name = fileName;
                if (baseUri != "")
                {
                    fileDto.Url = baseUri + "/" + directory + "/" + StorageFileName;
                }
                return fileDto;
            }
            else
            {
                return fileDto;
            }
        }



        public static void WriteLog(Exception e, string basePath)
        {
            if (!Directory.Exists(basePath))
            {
                Directory.CreateDirectory(basePath);
            }

            string filePath = Path.GetFullPath(
                Path.Combine(basePath, $"{DateTime.Now:dd-MM-yyyy}.txt")
            );
            StreamWriter sw;

            if (!File.Exists(filePath))
            {
                sw = File.CreateText(filePath);
            }
            else
            {
                sw = File.AppendText(filePath);
            }

            sw.WriteLine("----------------------------------------");
            sw.WriteLine($"Time:{DateTime.Now:dd-MM-yyyy-->HH/mm}");
            sw.WriteLine($"Error:{e.Message}");
            sw.WriteLine(e.StackTrace);
            sw.WriteLine("----------------------------------------");

            sw.Flush();
            sw.Close();
        }

        public static async Task<FileModel> UploadFileFromByteArray(
            byte[] file,
            string fileName,
            string basePath,
            List<string> ExtensionsConstraint,
            string baseUri = ""
        )
        {
            string directoryPath = "";
            var filePath = "";
            var fileDto = new FileModel();
            if (file.Length > 0)
            {
                var ext = Path.GetExtension(fileName);

                if (ExtensionsConstraint.Any() && !ExtensionsConstraint.Any(x => x.Contains(ext)))
                {
                    throw new GlobalException(
                        HttpMessage.InvalidExtension,
                        HttpStatusCode.BadRequest
                    );
                }

                var directory = $"{DateTime.Now:dd-MM-yyyy}";
                directoryPath = Path.GetFullPath(Path.Combine(basePath, directory));
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                var StorageFileName = $"{Guid.NewGuid()}{DateTime.Now:yyyyMMddHHmmss}_" + fileName;
                filePath = Path.Combine(directoryPath, StorageFileName);
                await File.WriteAllBytesAsync(filePath, file);

                fileDto.Extension = ext;
                fileDto.Path = filePath;
                fileDto.Name = fileName;
                if (baseUri != "")
                {
                    fileDto.Url = Path.Combine(baseUri, directory, StorageFileName);
                    ;
                }
                return fileDto;
            }
            else
            {
                return fileDto;
            }
        }

        public static void RemoveFileAsync(string filePath)
        {
            if (File.Exists(filePath))
            {
                GC.WaitForPendingFinalizers();
                File.Delete(filePath);
            }
        }
    }

    public class FileModel
    {
        public string Path { get; set; }
        public string Name { get; set; }
        public string Extension { get; set; }
        public string Url { get; set; }
    }
}
