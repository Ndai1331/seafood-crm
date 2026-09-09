using Contract.Identity.WebAuthn;

namespace Application.Identity.WebAuthn
{
    /// <summary>What still stands between a proven primary credential and a session.</summary>
    public enum WebAuthnLoginStep
    {
        /// <summary>Nothing left — issue the session.</summary>
        IssueSession,

        /// <summary>Ask for the authenticator code.</summary>
        RequireTotp,

        /// <summary>User holds at least one key; make them use it.</summary>
        RequireSecurityKey,

        /// <summary>User holds no key and the feature is mandatory; make them register one.</summary>
        RequireEnrollment
    }

    /// <summary>
    /// The whole branch decision as one pure function, on purpose.
    ///
    /// This is the gate the entire feature rests on, and it needs tests. The rest of the login
    /// path is welded to UserManager, DreamContext and IConfiguration, and this test project has
    /// no mocking library — so anything living inside those classes is untestable in practice.
    /// Keeping the decision separate is what makes "feature off behaves exactly as before"
    /// something we can assert rather than hope.
    ///
    /// Every branch must terminate: a step is only returned when the matching proof has NOT been
    /// supplied, otherwise completing that step would land straight back on it.
    /// </summary>
    public static class WebAuthnLoginDecision
    {
        /// <summary>
        /// What a sign-in stopped at the key step can still be finished with. Pure for the same
        /// reason Decide is: this list decides which doors the login screen draws, so it needs
        /// tests more than it needs convenience.
        ///
        /// TOTP makes the list only when the sign-in began with a password (proof == None on a
        /// fresh password login): an authenticator code is a second factor that sits BEHIND a
        /// password, and on any other entrance there is no password under it — offering it there
        /// would collapse two factors into one. Filtered here, server-side, not hidden in the UI.
        /// </summary>
        public static List<string> AvailableSecondFactors(
            bool userHasYubikey,
            bool userHasPasskey,
            bool userHasTotp,
            SecondFactorProof proof)
        {
            var factors = new List<string>();

            if (userHasYubikey)
            {
                factors.Add(SecondFactorKinds.Yubikey);
            }

            if (userHasPasskey)
            {
                factors.Add(SecondFactorKinds.Passkey);
            }

            if (userHasTotp && proof == SecondFactorProof.None)
            {
                factors.Add(SecondFactorKinds.Totp);
            }

            return factors;
        }

        public static WebAuthnLoginStep Decide(
            bool featureEnabled,
            bool enforced,
            bool userHasSecurityKey,
            bool userHasTotp,
            SecondFactorProof proof)
        {
            // A verified key is the strongest thing on offer here; nothing outranks it.
            if (proof == SecondFactorProof.SecurityKey)
            {
                return WebAuthnLoginStep.IssueSession;
            }

            // An accepted authenticator code is a finished second factor even for somebody who
            // holds a key. This branch sat BELOW the key demand until 2026-08-24, which made the
            // authenticator useless as a backup: a key-holder who lost the key was asked for it
            // again after every correct code. It never waives enrollment, checked further down.
            if (proof == SecondFactorProof.Totp)
            {
                return featureEnabled && enforced && !userHasSecurityKey
                    ? WebAuthnLoginStep.RequireEnrollment
                    : WebAuthnLoginStep.IssueSession;
            }

            if (featureEnabled && userHasSecurityKey)
            {
                return WebAuthnLoginStep.RequireSecurityKey;
            }

            // Only demanded when nothing at all has been proven yet. A renewal must not re-ask for
            // an authenticator code: refresh never checked TOTP before this feature existed, and
            // making it do so would change how existing sessions behave with the feature switched
            // off entirely.
            var totpStillOwed = userHasTotp && proof == SecondFactorProof.None;

            // Enrollment is owed only when the feature is on, mandatory, and this account has no
            // key. Note it is checked AFTER TOTP below: a user who already runs an authenticator
            // must clear it before enrolling, or switching this on would strip a second factor
            // from precisely the people who had already set one up.
            var enrollmentOwed = featureEnabled && enforced && !userHasSecurityKey;

            if (totpStillOwed)
            {
                return WebAuthnLoginStep.RequireTotp;
            }

            return enrollmentOwed
                ? WebAuthnLoginStep.RequireEnrollment
                : WebAuthnLoginStep.IssueSession;
        }
    }
}
