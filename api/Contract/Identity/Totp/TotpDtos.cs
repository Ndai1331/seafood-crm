namespace Contract.Identity.Totp
{
    public class TotpVerifyLoginRequestDto
    {
        public string TemporaryToken { get; set; } = string.Empty;
        public string? TotpCode { get; set; }
        public string? RecoveryCode { get; set; }
    }

    public class TotpCodeRequestDto
    {
        public string TotpCode { get; set; } = string.Empty;
    }

    /// <summary>Starting the authenticator step of login-screen enrollment.</summary>
    public class TotpSetupWithTokenDto
    {
        public string SetupToken { get; set; } = string.Empty;
    }

    /// <summary>Finishing it: the six digits, proving the app really holds the secret.</summary>
    public class TotpConfirmWithTokenDto
    {
        public string SetupToken { get; set; } = string.Empty;
        public string TotpCode { get; set; } = string.Empty;
    }

    /// <summary>
    /// The last step of enrollment: recovery codes to write down, and the session that was being
    /// withheld until the account had a way back.
    /// </summary>
    public class TotpEnrollResultDto
    {
        public List<string> RecoveryCodes { get; set; } = new();
        public Contract.Identity.UserManager.TokenDto? Token { get; set; }
    }

    public class TotpSetupResponseDto
    {
        public string ProvisioningUri { get; set; } = string.Empty;
        public string Base32Secret { get; set; } = string.Empty;
    }

    public class TotpRecoveryCodesResponseDto
    {
        public List<string> RecoveryCodes { get; set; } = new();
    }

    public class TotpStatusDto
    {
        public bool FeatureEnabled { get; set; }
        public bool TotpEnabled { get; set; }
        public int RemainingRecoveryCodes { get; set; }
    }
}
