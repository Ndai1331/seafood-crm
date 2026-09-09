using Contract.Identity.UserManager;

namespace Contract.Identity.Yubico
{
    /// <summary>Setting the PIN on a key that was provisioned for this user.</summary>
    public class YubikeySetPinDto
    {
        public string Pin { get; set; } = string.Empty;
        public string ConfirmPin { get; set; } = string.Empty;
    }

    /// <summary>Checking the PIN on its own, so the UI can open the touch step.</summary>
    public class YubikeyPinDto
    {
        public string TemporaryToken { get; set; } = string.Empty;
        public string Pin { get; set; } = string.Empty;
    }

    /// <summary>Finishing a login: the OTP, plus the half-authenticated token it belongs to.</summary>
    public class YubikeyVerifyDto
    {
        public string TemporaryToken { get; set; } = string.Empty;
        public string Otp { get; set; } = string.Empty;
    }

    /// <summary>Starting a passwordless login: a touch and nothing else.</summary>
    public class YubikeyPasswordlessTouchDto
    {
        public string Otp { get; set; } = string.Empty;
    }

    /// <summary>
    /// What the touch step hands back. Carries no hint of whose key it was — the caller has not
    /// proven anything yet beyond holding a registered key, and naming the account here would turn
    /// a found key into a way of learning who it belongs to.
    /// </summary>
    public class YubikeyPasswordlessTouchResultDto
    {
        public string TemporaryToken { get; set; } = string.Empty;
    }

    /// <summary>Finishing a passwordless login with the PIN.</summary>
    public class YubikeyPasswordlessPinDto
    {
        public string TemporaryToken { get; set; } = string.Empty;
        public string Pin { get; set; } = string.Empty;
    }

    /// <summary>Registering a key: one touch, while signed in.</summary>
    public class YubikeyEnrollDto
    {
        public string Otp { get; set; } = string.Empty;
    }

    /// <summary>
    /// Registering the first key at the login screen, where there is no session yet. The
    /// enrollment token names the account — never a bearer header, for the same reason the other
    /// half-authenticated endpoints ignore it: the browser still attaches whatever token is left
    /// in localStorage, and on a shared machine that is somebody else's.
    /// </summary>
    public class YubikeyEnrollWithTokenDto
    {
        public string EnrollToken { get; set; } = string.Empty;
        public string Otp { get; set; } = string.Empty;
    }

    /// <summary>
    /// What login-screen enrollment answers with. The session only comes back once nothing is
    /// owed: a key with no PIN is refused at the passwordless door, and a key with no
    /// authenticator app leaves no way back when it breaks. While either is missing the reply
    /// carries a setup token instead, and the login screen keeps walking.
    /// </summary>
    public class YubikeyEnrollResultDto
    {
        public YubikeyDto Key { get; set; } = new();

        /// <summary>Present only when the account is finished. Null while a step is still owed.</summary>
        public TokenDto? Token { get; set; }

        /// <summary>Spends the remaining setup steps. Null once the session is issued.</summary>
        public string? SetupToken { get; set; }

        public bool NeedsPin { get; set; }
        public bool NeedsAuthenticator { get; set; }
    }

    /// <summary>Setting the PIN mid-enrollment, holding the setup token rather than a session.</summary>
    public class YubikeySetupPinDto
    {
        public string SetupToken { get; set; } = string.Empty;
        public string Pin { get; set; } = string.Empty;
        public string ConfirmPin { get; set; } = string.Empty;
    }

    /// <summary>
    /// Where the wizard stands after a step. Same shape as the enrollment reply minus the key, so
    /// the login screen has one thing to read: a session means done, a setup token means carry on.
    /// </summary>
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

    /// <summary>What the login screen needs to know before drawing anything.</summary>
    public class YubikeyFeatureStatusDto
    {
        public bool Enabled { get; set; }
        public bool Enforced { get; set; }
        public bool PinRequired { get; set; }

        /// <summary>
        /// Whether the login screen should still draw the password form. Anonymous callers get
        /// this because the screen has to decide what to show before anyone has signed in.
        /// </summary>
        public bool AllowPasswordLogin { get; set; } = true;
    }

    /// <summary>What an administrator sees about somebody else's keys — never the secrets.</summary>
    public class AdminYubikeyStatusDto
    {
        public int KeyCount { get; set; }
        public bool AnyConfirmed { get; set; }
        public DateTime? LastUsedAt { get; set; }
    }

}
