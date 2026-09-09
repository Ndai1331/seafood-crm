namespace Contract.Identity.WebAuthn
{
    /// <summary>
    /// Which second factor the caller has just proven, if any.
    ///
    /// Lives on the public contract because it is a parameter of IUserManagerService's token
    /// issuing methods — the choke point where the second factor is enforced.
    /// </summary>
    public enum SecondFactorProof
    {
        /// <summary>Only the primary credential (password or SSO identity) has been checked.</summary>
        None,

        /// <summary>An authenticator code was accepted in this exchange.</summary>
        Totp,

        /// <summary>A WebAuthn assertion or registration was verified in this exchange.</summary>
        SecurityKey,

        /// <summary>
        /// Renewing a session that was already issued. Whatever factors were in force when it was
        /// created were satisfied then, so they are not demanded again — but a factor introduced
        /// SINCE still applies, which is what stops an open session outliving a new requirement.
        /// </summary>
        ExistingSession
    }

    /// <summary>
    /// Names for the second factors a stopped sign-in can still be finished with. Wire values —
    /// the login screen matches on them — so they are constants here rather than an enum that
    /// would serialise as numbers.
    /// </summary>
    public static class SecondFactorKinds
    {
        /// <summary>A Yubico OTP key: touch plus PIN.</summary>
        public const string Yubikey = "yubikey";

        /// <summary>A WebAuthn credential: browser passkey, Touch ID, Windows Hello.</summary>
        public const string Passkey = "passkey";

        /// <summary>An authenticator app code. Only ever offered behind a password.</summary>
        public const string Totp = "totp";
    }
}
