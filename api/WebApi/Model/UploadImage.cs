namespace WebApi.Model;

public class UploadImageRequest
{
    public string Base64 { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string FileExt { get; set; } = string.Empty;
}