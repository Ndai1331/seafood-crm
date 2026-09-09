using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.WebAuthnSettings
{
    /// <summary>
    /// One row per save of <see cref="WebAuthnSetting"/>, holding the state as it was afterwards.
    ///
    /// The Yubico pair is the reason this exists. Saving a wrong client id or secret makes every
    /// key touch fail with "cấu hình xác thực khoá chưa đúng", and the secret is never readable
    /// from the settings page — so without a record of what worked before, the only remedy was
    /// wiping every registered key and making the whole company enroll again.
    ///
    /// See Scripts/20260907_create_webauthn_setting_history.sql.
    /// </summary>
    [Table("webauthn_setting_history")]
    public class WebAuthnSettingHistory
    {
        public int Id { get; set; }

        [Column("is_enabled")]
        public bool IsEnabled { get; set; }

        [Column("is_enforced")]
        public bool IsEnforced { get; set; }

        [Column("allow_password_login")]
        public bool AllowPasswordLogin { get; set; } = true;

        [Column("yubico_client_id")]
        public string? YubicoClientId { get; set; }

        /// <summary>
        /// Stored exactly as the settings row stores it, and for the same reason: this is what a
        /// restore copies back. Server-side only — no endpoint ever returns it.
        /// </summary>
        [Column("yubico_secret_key")]
        public string? YubicoSecretKey { get; set; }

        /// <summary>
        /// Which fields this save touched, comma separated. Written at save time rather than
        /// derived later: reading a diff out of two rows is exactly the work an admin should not
        /// have to do while a login is broken.
        /// </summary>
        [Column("changed_fields")]
        public string ChangedFields { get; set; } = string.Empty;

        [Column("changed_by")]
        public string? ChangedBy { get; set; }

        [Column("changed_at")]
        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
    }
}
