namespace Contract.Uploads
{
    public class FileDto
    {
        public int Id { get; set; }
        public string Path { get; set; }
        public string FileName { get; set; }
        public long Size { get; set; }

        public string? Url { get; set; }
        public string? PDFURL { get; set; }

        public string? Extension { get; set; }
    }
}
