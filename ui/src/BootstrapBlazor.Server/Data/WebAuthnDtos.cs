namespace BootstrapBlazor.Server.Data;

public class WebAuthnFeatureStatusDto
{
    public bool Enabled { get; set; }
    public bool Enforced { get; set; }
}

/// <summary>Raw ceremony options from the API, passed straight through to the browser.</summary>
public class WebAuthnCeremonyOptionsDto
{
    public string OptionsJson { get; set; } = string.Empty;
}

public class WebAuthnRegisterCompleteDto
{
    public string AttestationJson { get; set; } = string.Empty;
    public string DeviceName { get; set; } = string.Empty;
}

public class WebAuthnEnrollCompleteDto : WebAuthnRegisterCompleteDto
{
    public string EnrollToken { get; set; } = string.Empty;
}

public class WebAuthnCeremonyStartDto
{
    public string TemporaryToken { get; set; } = string.Empty;
}

public class WebAuthnLoginCompleteDto
{
    public string TemporaryToken { get; set; } = string.Empty;
    public string AssertionJson { get; set; } = string.Empty;
}

public class SecurityKeyDto
{
    public int Id { get; set; }
    public string DeviceName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? LastUsedAt { get; set; }
}

public class SecurityKeyEnrollResultDto
{
    public SecurityKeyDto Key { get; set; } = new();
    public TokenDto? Token { get; set; }
}

public class WebAuthnSettingStatusDto
{
    public bool IsEnabled { get; set; }
    public bool IsEnforced { get; set; }

    /// <summary>Off means only a security key gets in. True by default so a response missing the
    /// field never renders as "passwords are gone".</summary>
    public bool AllowPasswordLogin { get; set; } = true;

    /// <summary>Public half of the Yubico API pair.</summary>
    public string? YubicoClientId { get; set; }

    /// <summary>Whether a secret is stored. The API never sends the secret itself.</summary>
    public bool HasYubicoSecret { get; set; }

    public int ActiveUsers { get; set; }
    public int UsersWithKey { get; set; }
    /// <summary>Super admins holding at least one key — what the enforcement guard counts.</summary>
    public int SuperAdminsWithKey { get; set; }
    public int ActiveSuperAdmins { get; set; }
    public bool CallerHasKey { get; set; }

    /// <summary>
    /// Active accounts with no key and no password permission — the people who would find no way
    /// in once passwords are off. A number to read, not a wall: most accounts here exist only so a
    /// domain can be assigned to a PIC.
    /// </summary>
    public int UsersLockedOut { get; set; }

    /// <summary>Null when enforcement may be turned on; otherwise why the API refuses.</summary>
    public string? EnforcementBlockedReason { get; set; }

    /// <summary>Null when password login may be switched off; otherwise why the API refuses.</summary>
    public string? PasswordDisableBlockedReason { get; set; }
}

/// <summary>One saved state of the login settings. The secret is a flag, never a value.</summary>
public class WebAuthnSettingHistoryDto
{
    public int Id { get; set; }
    public bool IsEnabled { get; set; }
    public bool IsEnforced { get; set; }
    public bool AllowPasswordLogin { get; set; }
    public string? YubicoClientId { get; set; }
    public bool HasYubicoSecret { get; set; }
    public string ChangedFields { get; set; } = string.Empty;
    public string? ChangedBy { get; set; }
    public DateTime ChangedAt { get; set; }
}

/// <summary>Whether the login screen may draw a password box for one named account.</summary>
public class PasswordDoorDto
{
    /// <summary>False by default: a failed call must never open a box that cannot work.</summary>
    public bool Allowed { get; set; }
}

/// <summary>
/// One active account on the login-access list: whether it holds a key, and whether it keeps the
/// password door while the organisation-wide switch is off.
/// </summary>
public class WebAuthnUserAccessDto
{
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public int KeyCount { get; set; }
    public bool PasswordLoginAllowed { get; set; }
    public bool IsSuperAdmin { get; set; }
}

/// <summary>What an organisation-wide key reset removed.</summary>
public class SecurityKeyResetSummaryDto
{
    public int UsersAffected { get; set; }
    public int YubikeysRemoved { get; set; }
    public int PasskeysRemoved { get; set; }
}

public class WebAuthnSettingUpdateDto
{
    public bool IsEnabled { get; set; }
    public bool IsEnforced { get; set; }
    public bool AllowPasswordLogin { get; set; } = true;
    public string? YubicoClientId { get; set; }

    /// <summary>Empty means "keep the stored secret" — nobody can read it back to retype it.</summary>
    public string? YubicoSecretKey { get; set; }
}

public class AdminSecurityKeyStatusDto
{
    public int KeyCount { get; set; }
    public DateTime? LastUsedAt { get; set; }
}

/// <summary>
/// Result of a ceremony driven entirely in the browser. Carries the session token when the
/// ceremony was one that ends with the user signed in (login, or first-key enrollment).
/// </summary>
public class WebAuthnCeremonyOutcome
{
    public bool Ok { get; set; }
    public string? Error { get; set; }
    public string? Message { get; set; }
    public TokenDto? Token { get; set; }
}

/// <summary>
/// What the browser reported back from a ceremony. Failures arrive as data with the DOM error
/// name intact — thrown exceptions lose it, and the name is what the user-facing message hangs on.
/// </summary>
public class WebAuthnBrowserResult
{
    public bool Ok { get; set; }
    public string? Json { get; set; }
    public string? Error { get; set; }
    public string? Message { get; set; }
}
