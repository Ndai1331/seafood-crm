using Domain.Identity.Users;

namespace Domain.Identity.UserYubikeys
{
    /// <summary>
    /// One YubiKey registered to a user. Yubico's service vouches that an OTP is genuine and
    /// unused; this row is what says whose key it came from.
    /// See Scripts/20260822_create_user_yubikeys.sql and 20260823_yubikeys_drop_local_secrets.sql.
    ///
    /// PublicId is unique table-wide, unlike the WebAuthn table. A Yubico OTP credential has ONE
    /// fixed public id for the life of the key, so it is the key's identity: letting two accounts
    /// claim it would make an incoming OTP ambiguous about who is logging in.
    /// </summary>
    public class UserYubikey
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; } = null!;

        /// <summary>12 modhex characters typed at the start of every OTP this key produces.</summary>
        public string PublicId { get; set; } = string.Empty;

        public string DeviceName { get; set; } = "YubiKey";

        /// <summary>BCrypt. Null when the organisation has the PIN step switched off.</summary>
        public string? PinHash { get; set; }
        public int PinFailedCount { get; set; }
        public DateTime? PinLockoutEnd { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastUsedAt { get; set; }
    }
}
