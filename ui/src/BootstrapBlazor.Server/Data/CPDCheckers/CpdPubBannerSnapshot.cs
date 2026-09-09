namespace BootstrapBlazor.Server.Data;

/// <summary>
/// Banner snapshot model for displaying banner history
/// </summary>
public class CpdPubBannerSnapshot
{
    public ulong Id { get; set; }
    public ulong CpdCheckerToolRowId { get; set; }
    
    // Version 1 (mới nhất)
    public string? BannerUrl1 { get; set; }
    public DateOnly? CheckDate1 { get; set; }
    public string? Brand1 { get; set; }
    public string? UtmSource1 { get; set; }
    public string? Affid1 { get; set; }
    
    // Version 2
    public string? BannerUrl2 { get; set; }
    public DateOnly? CheckDate2 { get; set; }
    public string? Brand2 { get; set; }
    public string? UtmSource2 { get; set; }
    public string? Affid2 { get; set; }
    
    // Version 3
    public string? BannerUrl3 { get; set; }
    public DateOnly? CheckDate3 { get; set; }
    public string? Brand3 { get; set; }
    public string? UtmSource3 { get; set; }
    public string? Affid3 { get; set; }
    
    // Version 4 (cũ nhất)
    public string? BannerUrl4 { get; set; }
    public DateOnly? CheckDate4 { get; set; }
    public string? Brand4 { get; set; }
    public string? UtmSource4 { get; set; }
    public string? Affid4 { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// Banner version info for display
/// </summary>
public class BannerVersionInfo
{
    public int Version { get; set; }
    public string? BannerUrl { get; set; }
    public DateOnly? CheckDate { get; set; }
    public string? Brand { get; set; }
    public string? UtmSource { get; set; }
    public string? Affid { get; set; }
    public bool HasData => !string.IsNullOrWhiteSpace(BannerUrl);
}

