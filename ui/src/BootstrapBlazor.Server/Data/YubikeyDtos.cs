namespace BootstrapBlazor.Server.Data;

/// <summary>Mirrors Contract.Identity.Yubico on the API side.</summary>
public class YubikeyFeatureStatusDto
{
    public bool Enabled { get; set; }
    public bool Enforced { get; set; }
    public bool PinRequired { get; set; }

    /// <summary>Whether the login screen should still draw the password form. True by default so
    /// a response missing the field never renders as "there is no way in".</summary>
    public bool AllowPasswordLogin { get; set; } = true;
}

public class YubikeyPasswordlessTouchResultDto
{
    public string TemporaryToken { get; set; } = string.Empty;
}

public class YubikeyDto
{
    public int Id { get; set; }
    public string PublicId { get; set; } = string.Empty;
    public string DeviceName { get; set; } = string.Empty;
    public bool HasPin { get; set; }
    public bool Confirmed { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastUsedAt { get; set; }
}

/// <summary>
/// What registering the first key at the login screen answers with. A session means the account
/// is finished; a setup token means a step is still owed.
/// </summary>
public class YubikeyEnrollResultDto
{
    public YubikeyDto Key { get; set; } = new();
    public TokenDto? Token { get; set; }
    public string? SetupToken { get; set; }
    public bool NeedsPin { get; set; }
    public bool NeedsAuthenticator { get; set; }
}

/// <summary>Where the enrollment wizard stands after a step.</summary>
public class YubikeySetupProgressDto
{
    public TokenDto? Token { get; set; }
    public string? SetupToken { get; set; }
    public bool NeedsPin { get; set; }
    public bool NeedsAuthenticator { get; set; }
}

public class YubikeyPinResultDto
{
    public bool Ok { get; set; }
    public int? AttemptsLeft { get; set; }
    public int? LockedForMinutes { get; set; }
    public string? Error { get; set; }
}


/// <summary>What an administrator sees about somebody else's keys — never the secrets.</summary>
public class AdminYubikeyStatusDto
{
    public int KeyCount { get; set; }
    public bool AnyConfirmed { get; set; }
    public DateTime? LastUsedAt { get; set; }
}
