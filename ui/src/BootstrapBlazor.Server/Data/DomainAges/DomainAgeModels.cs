namespace BootstrapBlazor.Server.Data.DomainAges;

public enum DomainAgeLookupStatus
{
    Found,
    NotRegistered,
    UnsupportedTld,
    UnknownRegistrationDate,
    InvalidDomain,
    UpstreamError,
    Cancelled,
}

public enum WaybackArchiveStatus
{
    Found,
    NotArchived,
    UpstreamError,
    Cancelled,
}

public sealed record DomainAgeNormalizationResult(
    string OriginalInput,
    string? Domain,
    bool IsValid,
    string? Message = null);

public sealed record DomainAgeLookupResult
{
    public required string OriginalInput { get; init; }

    public required string Domain { get; init; }

    public required DomainAgeLookupStatus Status { get; init; }

    public DateTimeOffset? RegisteredAtUtc { get; init; }

    public int? AgeYears { get; init; }

    public int? AgeMonths { get; init; }

    public string? Source { get; init; }

    public string? Message { get; init; }

    public WaybackArchiveStatus? WaybackStatus { get; init; }

    public DateTimeOffset? FirstArchivedAtUtc { get; init; }

    public int? ArchiveAgeYears { get; init; }

    public int? ArchiveAgeMonths { get; init; }

    public string? WaybackUrl { get; init; }

    public string? WaybackMessage { get; init; }
}

public sealed record RdapDomainAgeResult
{
    public required DomainAgeLookupStatus Status { get; init; }

    public DateTimeOffset? RegisteredAtUtc { get; init; }

    public string? Source { get; init; }

    public string? Message { get; init; }
}

public sealed record WaybackArchiveResult
{
    public required WaybackArchiveStatus Status { get; init; }

    public DateTimeOffset? FirstArchivedAtUtc { get; init; }

    public string? SourceUrl { get; init; }

    public string? Message { get; init; }
}
