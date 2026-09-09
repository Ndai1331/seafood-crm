namespace BootstrapBlazor.Server.Data;

public class MarketShareReportFilterDto
    {
        /// <summary>
        /// From month (format: MM-yyyy, e.g., "11-2025")
        /// </summary>
        public string FromMonth { get; set; } = string.Empty;

        /// <summary>
        /// To month (format: MM-yyyy, e.g., "12-2025")
        /// </summary>
        public string ToMonth { get; set; } = string.Empty;

        /// <summary>
        /// List of PicNames to filter (if empty, returns all)
        /// </summary>
        public List<string> PicNames { get; set; } = new List<string>();

        /// <summary>
        /// Key brand/industry filter (optional)
        /// </summary>
        public string? KeyBrand { get; set; }

        /// <summary>
        /// Single brand filter (optional)
        /// </summary>
        public string? Brand { get; set; }

        /// <summary>
        /// List of brands to filter (if empty, returns all)
        /// </summary>
        public List<string> Brands { get; set; } = new List<string>();
    }