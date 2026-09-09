namespace Contract.Identity.WebAuthn
{
    /// <summary>Runtime state of the security-key second factor.</summary>
    public class WebAuthnSettingDto
    {
        /// <summary>Off means the login screen behaves exactly as it did before the feature existed.</summary>
        public bool IsEnabled { get; set; }

        /// <summary>On means a user without a key must enroll before they can log in.</summary>
        public bool IsEnforced { get; set; }

        /// <summary>
        /// Off means username and password are gone and only a security key gets in. True
        /// everywhere by default — see WebAuthnSetting for why that direction is the safe one.
        /// </summary>
        public bool AllowPasswordLogin { get; set; } = true;

        /// <summary>Public half of the Yubico API pair. Safe to show an administrator.</summary>
        public string? YubicoClientId { get; set; }

        /// <summary>
        /// Whether a secret key is stored. Deliberately a flag and not the value: this DTO is
        /// serialised straight to a browser, and the secret must never make that trip.
        /// </summary>
        public bool HasYubicoSecret { get; set; }
    }

    /// <summary>
    /// What an admin sees next to the switches. The counts are the whole point: nobody should have
    /// to run a query to find out whether flipping a switch locks the organisation out.
    /// </summary>
    public class WebAuthnSettingStatusDto : WebAuthnSettingDto
    {
        public int ActiveUsers { get; set; }
        public int UsersWithKey { get; set; }

        /// <summary>
        /// Super admins holding at least one key — the number the enforcement guard rests on. Two
        /// of them is the way back: either can switch the feature off for whoever lost a key.
        /// </summary>
        public int SuperAdminsWithKey { get; set; }
        public int ActiveSuperAdmins { get; set; }

        /// <summary>Whether the admin making the request holds a key themselves.</summary>
        public bool CallerHasKey { get; set; }

        /// <summary>
        /// Active accounts that would have no way in once passwords are off: no key registered and
        /// no per-account password permission. Most of them are accounts that exist only so a
        /// domain can be assigned to a PIC, which is why this is a number to read rather than a
        /// wall — the admin decides whether those accounts matter.
        /// </summary>
        public int UsersLockedOut { get; set; }

        /// <summary>Null when enforcement may be turned on; otherwise why it is refused.</summary>
        public string? EnforcementBlockedReason { get; set; }

        /// <summary>
        /// Null when password login may be switched off; otherwise why it is refused. Separate
        /// from the enforcement reason because the two switches fail for different causes and an
        /// admin needs to know which one they are being stopped on.
        /// </summary>
        public string? PasswordDisableBlockedReason { get; set; }
    }

    /// <summary>
    /// One saved state of the login settings. The secret is a flag, never a value — same rule as
    /// the settings DTO, and for the same reason: this travels to a browser.
    /// </summary>
    public class WebAuthnSettingHistoryDto
    {
        public int Id { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsEnforced { get; set; }
        public bool AllowPasswordLogin { get; set; }
        public string? YubicoClientId { get; set; }
        public bool HasYubicoSecret { get; set; }

        /// <summary>What that save touched, in words, so the list reads without diffing rows.</summary>
        public string ChangedFields { get; set; } = string.Empty;
        public string? ChangedBy { get; set; }
        public DateTime ChangedAt { get; set; }
    }

    /// <summary>
    /// One active account on the login-access list: whether it holds a key, and whether it keeps
    /// the password door while the organisation-wide switch is off.
    /// </summary>
    public class WebAuthnUserAccessDto
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;

        /// <summary>Keys across both tables — a YubiKey and a passkey both count.</summary>
        public int KeyCount { get; set; }
        public bool PasswordLoginAllowed { get; set; }

        /// <summary>Super admins are called out: they are the way back if a switch goes wrong.</summary>
        public bool IsSuperAdmin { get; set; }
    }

    /// <summary>What an organisation-wide key reset actually removed.</summary>
    public class SecurityKeyResetSummaryDto
    {
        public int UsersAffected { get; set; }
        public int YubikeysRemoved { get; set; }
        public int PasskeysRemoved { get; set; }
    }

    /// <summary>
    /// Whether the login screen may show a password box for one named account. Answered before
    /// any password is typed, so it deliberately says nothing about whether the account exists:
    /// an unknown name and a known name without permission get the same answer.
    /// </summary>
    public class PasswordDoorDto
    {
        public bool Allowed { get; set; }
    }

    /// <summary>Grants or takes back one account's password door.</summary>
    public class PasswordLoginAllowedUpdateDto
    {
        public bool Allowed { get; set; }
    }

    public class WebAuthnSettingUpdateDto
    {
        public bool IsEnabled { get; set; }
        public bool IsEnforced { get; set; }

        /// <summary>Defaults to true so an older caller that omits it cannot switch passwords off.</summary>
        public bool AllowPasswordLogin { get; set; } = true;

        public string? YubicoClientId { get; set; }

        /// <summary>
        /// Write-only. Null or empty means "leave the stored secret alone", so an administrator can
        /// change the client id, or any other switch, without re-typing a key they cannot read back.
        /// </summary>
        public string? YubicoSecretKey { get; set; }
    }

    /// <summary>
    /// The Yubico API pair as the server uses it. Never serialised to a client — the settings
    /// endpoint answers with WebAuthnSettingDto, which carries a flag instead of the secret.
    /// </summary>
    public record YubicoCredentials(string? ClientId, string? SecretKey)
    {
        public bool IsComplete =>
            !string.IsNullOrWhiteSpace(ClientId) && !string.IsNullOrWhiteSpace(SecretKey);
    }
}
