using Microsoft.Extensions.Configuration;

namespace Application.Identity.Common
{
    /// <summary>
    /// Contract for the short-lived tokens handed out between "password accepted" and "second
    /// factor proven".
    ///
    /// These are signed with the same key as a real access token, so the only things keeping them
    /// from working as one are the audience below and the token_type claim. Both the issuing side
    /// and every validating side must go through here — a mismatch silently turns a
    /// half-authenticated token back into a full credential.
    /// </summary>
    public static class TwoFactorTokens
    {
        /// <summary>Claim marking a token as NOT a session token. Rejected by the bearer scheme.</summary>
        public const string TokenTypeClaim = "token_type";

        public static string ResolveAudience(IConfiguration configuration)
        {
            var configured = configuration["Jwt:TwoFactorAudience"];
            if (!string.IsNullOrWhiteSpace(configured))
            {
                return configured;
            }

            // Derived rather than falling back to Jwt:Audience: an unset key must not hand the
            // token the session audience, which is exactly the hole this separation closes.
            return $"{configuration["Jwt:Audience"]}/2fa";
        }
    }
}
