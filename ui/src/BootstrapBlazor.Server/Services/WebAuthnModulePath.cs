namespace BootstrapBlazor.Server.Services;

/// <summary>
/// Import paths for the browser modules, each carrying a per-build token.
///
/// Without it the browser keeps serving whatever copy it cached, so a fix can be live on the
/// server while every user still runs the old file — and the only symptom is that nothing
/// changed, which is indistinguishable from the fix not working.
/// </summary>
public static class WebAuthnModulePath
{
    private static readonly string BuildToken =
        typeof(WebAuthnModulePath).Assembly.ManifestModule.ModuleVersionId.ToString("N")[..8];

    public static string Value { get; } = $"./js/webauthn.js?v={BuildToken}";

    /// <summary>The YubiKey keystroke capture module, versioned the same way.</summary>
    public static string YubikeyCapture { get; } = $"./js/yubikey-capture.js?v={BuildToken}";
}
