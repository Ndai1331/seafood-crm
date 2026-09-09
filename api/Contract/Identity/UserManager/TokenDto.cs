namespace Contract.Identity.UserManager
{
    public class TokenDto
    {
        public string? UserId { get; set; }
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
        public bool RequiresTwoFactor { get; set; } = false;

        /// <summary>
        /// The password (or SSO identity) checked out, but the account carries a security key and
        /// has not touched it yet. No access token is issued; the caller must complete the
        /// WebAuthn assertion with <see cref="TemporaryToken"/>.
        /// </summary>
        public bool RequiresSecurityKey { get; set; } = false;

        /// <summary>
        /// Same, except the account has no key at all and the feature is mandatory, so the caller
        /// must register one first. Enrolling issues the session — see the enroll endpoints.
        /// </summary>
        public bool RequiresSecurityKeyEnrollment { get; set; } = false;

        public string? TemporaryToken { get; set; }

        /// <summary>
        /// When a sign-in stops at the security-key step: every factor the person can finish it
        /// with, as SecondFactorKinds values. Built and filtered server-side — the client only
        /// renders it. Null on every other answer.
        /// </summary>
        public List<string>? AvailableSecondFactors { get; set; }
        public int? RemainingRecoveryCodes { get; set; }
    }
}
