namespace BootstrapBlazor.Server.Data;

public class TotpFeatureStatusDto
{
    public bool FeatureEnabled { get; set; }
}

public class TotpStatusDto
{
    public bool FeatureEnabled { get; set; }
    public bool TotpEnabled { get; set; }
    public int RemainingRecoveryCodes { get; set; }
}

/// <summary>The last step of login-screen enrollment: codes to write down, plus the session.</summary>
public class TotpEnrollResultDto
{
    public List<string> RecoveryCodes { get; set; } = new();
    public TokenDto? Token { get; set; }
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

public class TotpCodeRequestDto
{
    public string TotpCode { get; set; } = string.Empty;
}

public class TotpVerifyLoginRequestDto
{
    public string TemporaryToken { get; set; } = string.Empty;
    public string? TotpCode { get; set; }
    public string? RecoveryCode { get; set; }
}
