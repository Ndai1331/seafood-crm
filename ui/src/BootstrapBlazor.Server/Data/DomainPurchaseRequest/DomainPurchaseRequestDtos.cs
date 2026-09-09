using System.ComponentModel.DataAnnotations;

namespace BootstrapBlazor.Server.Data.DomainPurchaseRequest;

public sealed class DomainPurchaseRequestFilterDto
{
    public string? Team { get; set; }
    public string? Pic { get; set; }
    public string? Status { get; set; }
    public string? Search { get; set; }
    public int Skip { get; set; }
    public int Take { get; set; } = 20;
}

public class DomainPurchaseRequestWriteDto
{
    [Required, StringLength(255)] public string Pic { get; set; } = "";
    [Required, StringLength(64)] public string Team { get; set; } = "";
    [Required, StringLength(255), DomainName] public string Domain { get; set; } = "";
    [StringLength(255)] public string? KeyMain { get; set; }
    [Range(0, double.MaxValue)] public decimal? Gia { get; set; }
    [StringLength(64)] public string? PhanLoai { get; set; }
    [StringLength(128)] public string? NhomKey { get; set; }
    [StringLength(64)] public string? ThiTruong { get; set; }
    [StringLength(64)] public string? DoKho { get; set; }
    [Range(0, int.MaxValue)] public int? Volume { get; set; }
    [StringLength(128)] public string? MucDichSuDung { get; set; }
    [StringLength(4000)] public string? LyDo { get; set; }
    [StringLength(4000)] public string? IcNote { get; set; }
}

public sealed class DomainPurchaseRequestItemDto : DomainPurchaseRequestWriteDto
{
    public long Id { get; set; }
    public string DomainNorm { get; set; } = "";
    public string Status { get; set; } = "";
    public string CreatedBy { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public sealed class DomainPurchaseRequestRefsDto
{
    public List<string> Pics { get; set; } = [];
    public List<string> Teams { get; set; } = [];
    public List<string> PhanLoai { get; set; } = [];
    public List<string> NhomKey { get; set; } = [];
    public List<string> ThiTruong { get; set; } = [];
    public List<string> DoKho { get; set; } = [];
    public List<string> MucDichSuDung { get; set; } = [];
}

public sealed class DomainPurchaseRequestBulkDto
{
    public List<DomainPurchaseRequestWriteDto> Rows { get; set; } = [];
}

public sealed class DomainPurchaseRequestBulkLineDto
{
    public int Line { get; set; }
    public bool Ok { get; set; }
    public List<string> Errors { get; set; } = [];
}

public sealed class DomainPurchaseRequestBulkResultDto
{
    public bool Committed { get; set; }
    public List<DomainPurchaseRequestBulkLineDto> Lines { get; set; } = [];
}

public sealed class DomainPurchaseRequestApprovalDto
{
    public long Id { get; set; }
    public long RequestId { get; set; }
    public string Step { get; set; } = "";
    public string Decision { get; set; } = "";
    public string Actor { get; set; } = "";
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; }
}

public sealed class DomainPurchaseRequestDecisionDto
{
    public string? Note { get; set; }
}

[AttributeUsage(AttributeTargets.Property)]
internal sealed class DomainNameAttribute() : ValidationAttribute("Domain không hợp lệ")
{
    public override bool IsValid(object? value)
    {
        if (value is not string raw || string.IsNullOrWhiteSpace(raw)) return true;
        var input = raw.Trim();
        if (!Uri.TryCreate(input.Contains("://", StringComparison.Ordinal) ? input : "https://" + input,
                UriKind.Absolute, out var uri)) return false;
        var host = uri.IdnHost.TrimEnd('.');
        if (host.StartsWith("www.", StringComparison.OrdinalIgnoreCase)) host = host[4..];
        return host.Length is >= 3 and <= 253
            && host.Contains('.')
            && !host.Contains('_')
            && Uri.CheckHostName(host) == UriHostNameType.Dns;
    }
}
