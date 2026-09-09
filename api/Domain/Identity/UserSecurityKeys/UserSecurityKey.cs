using Domain.Identity.Users;

namespace Domain.Identity.UserSecurityKeys
{
    /// <summary>
    /// One WebAuthn credential registered by a user — a YubiKey, or a platform authenticator such
    /// as Touch ID or Windows Hello. See Scripts/20260821_create_user_security_keys.sql.
    ///
    /// Revoking removes the row rather than flagging it, so that re-registering the same physical
    /// key after an admin reset does not collide with the unique key.
    /// </summary>
    public class UserSecurityKey
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public byte[] CredentialId { get; set; } = [];
        public byte[] PublicKey { get; set; } = [];

        /// <summary>Signature counter last seen. Going backwards suggests a cloned authenticator.</summary>
        public uint SignatureCounter { get; set; }

        public string DeviceName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastUsedAt { get; set; }
    }
}
