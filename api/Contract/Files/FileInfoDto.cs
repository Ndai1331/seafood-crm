namespace Contract.Files
{
    public class FileInfoDto
    {
        public string Name { get; set; } = string.Empty;
        public ulong Size { get; set; }
        public string Url { get; set; } = string.Empty;
        public DateTime LastModified { get; set; }
    }
}
