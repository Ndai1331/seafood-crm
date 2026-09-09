namespace Contract.Identity.WebAuthn
{
    /// <summary>
    /// Values of the token_type claim on the short-lived tokens issued between "credentials
    /// accepted" and "second factor proven".
    ///
    /// Each one authorises exactly one ceremony. They are never interchangeable: an enrollment
    /// token must not complete a login, and a login token must not register a key.
    /// </summary>
    public static class WebAuthnTokenTypes
    {
        /// <summary>User holds a key and must produce an assertion.</summary>
        public const string SecurityKeyStep = "webauthn_temp";

        /// <summary>User holds no key and must register one before getting in.</summary>
        public const string Enrollment = "webauthn_enroll";

        /// <summary>
        /// The key is registered but the account is not finished: the PIN, the authenticator app,
        /// or both are still owed. Minted when enrollment ends and spent by those steps, so the
        /// session only appears once the account can actually be recovered — a key with no PIN is
        /// refused at the passwordless door, and a key with no authenticator has no way back at
        /// all if it breaks.
        /// </summary>
        public const string Setup = "webauthn_setup";

        /// <summary>
        /// A key was touched and Yubico confirmed the OTP, but no password was ever presented —
        /// only the PIN is left. Deliberately not SecurityKeyStep: that one is minted after a
        /// correct password, so reusing it would let a bare touch inherit the standing a password
        /// earned. Two entrances, two tokens.
        /// </summary>
        public const string PasswordlessPinStep = "yubikey_passwordless_pin";
    }
}
