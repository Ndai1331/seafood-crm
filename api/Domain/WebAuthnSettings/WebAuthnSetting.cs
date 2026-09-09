using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.WebAuthnSettings
{
    /// <summary>
    /// The single row that decides what the login screen offers.
    ///
    /// Started as the security-key switches; allow_password_login joined it when workspace-email
    /// sign-in was retired, because two tables answering "what does the login screen show" is how
    /// they end up disagreeing.
    /// See Scripts/20260821_create_webauthn_settings.sql.
    ///
    /// An empty table means the same as {false, false}: the service returns defaults without
    /// writing, so shipping the schema changes nobody's login.
    /// </summary>
    [Table("webauthn_settings")]
    public class WebAuthnSetting
    {
        public int Id { get; set; }

        /// <summary>Master switch. Off means the login screen behaves exactly as it did before.</summary>
        [Column("is_enabled")]
        public bool IsEnabled { get; set; }

        /// <summary>On means a user without a key is pushed into enrollment before they can log in.</summary>
        [Column("is_enforced")]
        public bool IsEnforced { get; set; }

        /// <summary>
        /// Off means username and password are gone and only a security key gets in. Defaults to
        /// true everywhere: a missing column, an unreadable row, or a deploy that lands ahead of
        /// the migration all have to leave passwords working, because a password is the only way
        /// back for anyone holding no key.
        /// </summary>
        [Column("allow_password_login")]
        public bool AllowPasswordLogin { get; set; } = true;

        /// <summary>Yubico API client id. Public half of the pair; safe to show an administrator.</summary>
        [Column("yubico_client_id")]
        public string? YubicoClientId { get; set; }

        /// <summary>
        /// Base64 HMAC key for api.yubico.com. Never leaves the server: the settings endpoint
        /// answers with a "configured" flag instead. Lives here rather than only in an environment
        /// variable because variables die with the container — which is exactly how this feature
        /// became unrepairable-without-SSH on 2026-08-24.
        /// </summary>
        [Column("yubico_secret_key")]
        public string? YubicoSecretKey { get; set; }

        [Column("updated_by")]
        public string? UpdatedBy { get; set; }

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
