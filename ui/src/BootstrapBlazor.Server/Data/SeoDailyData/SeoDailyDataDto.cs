using System;
using System.Collections.Generic;

namespace BootstrapBlazor.Server.Data
{
    public class GetSeoDailyDataRequest
    {
        public int UserId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public bool OnlyManagedDomains { get; set; } = false;
        public int Skip { get; set; } = 0;
        public int Take { get; set; } = 50;
        public string? Keyword { get; set; }
        public string? Team { get; set; }
        public string? Domain { get; set; }
        public string? UserName { get; set; }
        
        // Pagination properties
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 50;
        public bool EnablePaging { get; set; } = false;
    }

    public class SeoDailyDataPaginatedResponse<T>
    {
        public bool Status { get; set; }
        public string? Message { get; set; }
        public List<T> Data { get; set; } = new List<T>();

        public int TotalRecords { get; set; }
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public bool HasNextPage { get; set; }
        public bool HasPreviousPage { get; set; }

        public DateTime QueryExecutedAt { get; set; }
        public double QueryExecutionTimeMs { get; set; }
    }

    public class UploadSeoDailyDataRequest
    {
        public DateTime DataDate { get; set; }
        public byte[] ExcelBytes { get; set; } = Array.Empty<byte>();
        public string FileName { get; set; } = string.Empty;
    }

    public class UploadSeoDailyDataResponse
    {
        public int SuccessRecords { get; set; }
    }

    public class SeoDailyDataCountByDateResponse
    {
        public DateTime DataDate { get; set; }
        public int Count { get; set; }
    }

    public class SeoDailyDataWithManagedDomainsDto
    {
        public int Id { get; set; }
        public DateTime DataDate { get; set; }
        public string Keyword { get; set; } = string.Empty;
        public int? SearchVolumePreviousMonth { get; set; }
        public int? SearchVolumeCurrentMonth { get; set; }
        public string Team { get; set; } = string.Empty;
        public int? BrandId { get; set; }
        public string? BrandName { get; set; }
        public int? BrandOdx { get; set; }
        public string? Top1 { get; set; }
        public string? Top2 { get; set; }
        public string? Top3 { get; set; }
        public string? Top4 { get; set; }
        public string? Top5 { get; set; }
        public string? Top6 { get; set; }
        public string? Top7 { get; set; }
        public string? Top8 { get; set; }
        public string? Top9 { get; set; }
        public string? Top10 { get; set; }
        public List<string> ManagedDomains { get; set; } = new List<string>();
        public Dictionary<string, bool> IsManagedDomain { get; set; } = new Dictionary<string, bool>();
        public Dictionary<string, List<string>> DomainPicMapping { get; set; } = new Dictionary<string, List<string>>();
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class SeoDailyDataWithPicDto
    {
        public int Id { get; set; }
        public DateTime DataDate { get; set; }
        public string Keyword { get; set; } = string.Empty;
        public int? SearchVolumePreviousMonth { get; set; }
        public int? SearchVolumeCurrentMonth { get; set; }
        public string Team { get; set; } = string.Empty;
        public int? BrandId { get; set; }
        public string? BrandName { get; set; }
        public int? BrandOdx { get; set; }
        public string? Top1 { get; set; }
        public string? Top2 { get; set; }
        public string? Top3 { get; set; }
        public string? Top4 { get; set; }
        public string? Top5 { get; set; }
        public string? Top6 { get; set; }
        public string? Top7 { get; set; }
        public string? Top8 { get; set; }
        public string? Top9 { get; set; }
        public string? Top10 { get; set; }
        public Dictionary<string, string?> DomainPicInfo { get; set; } = new Dictionary<string, string?>();
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
