using Contract.Identity.UserManager;

using System.ComponentModel.DataAnnotations;

namespace Contract.Identity.WebAuthn
{
    public class WebAuthnFeatureStatusDto
    {
        public bool Enabled { get; set; }
        public bool Enforced { get; set; }
    }

    /// <summary>What the browser needs to run a ceremony, as raw JSON straight from the library.</summary>
    public class WebAuthnCeremonyOptionsDto
    {
        public string OptionsJson { get; set; } = string.Empty;
    }

    /// <summary>Registration finish, from an already-signed-in user.</summary>
    public class WebAuthnRegisterCompleteDto
    {
        [MaxLength(64 * 1024)]
        public string AttestationJson { get; set; } = string.Empty;
        public string DeviceName { get; set; } = string.Empty;
    }

    /// <summary>
    /// Registration finish for someone who is NOT signed in yet, mid-enrollment.
    /// The token is read from the body and the Authorization header is ignored on this route: a
    /// stale bearer left in localStorage on a shared machine must never decide whose account the
    /// key lands on.
    /// </summary>
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

        [MaxLength(64 * 1024)]
        public string AssertionJson { get; set; } = string.Empty;
    }

    /// <summary>
    /// A registered key as shown to its owner. Deliberately carries no credential id and no public
    /// key — the UI only ever needs to tell one key from another.
    /// </summary>
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

        /// <summary>Set only for the enrollment path: registering was the second factor.</summary>
        public TokenDto? Token { get; set; }
    }

    public class AdminSecurityKeyStatusDto
    {
        public int KeyCount { get; set; }
        public DateTime? LastUsedAt { get; set; }
    }
}
