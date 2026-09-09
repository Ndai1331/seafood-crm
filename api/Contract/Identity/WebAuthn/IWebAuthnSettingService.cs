namespace Contract.Identity.WebAuthn
{
    public interface IWebAuthnSettingService
    {
        /// <summary>
        /// Current switches. Never throws: this sits on the login path, so a missing table or a
        /// database hiccup must degrade to "feature off", not lock everyone out.
        /// </summary>
        Task<WebAuthnSettingDto> GetAppliedAsync();

        /// <summary>
        /// The Yubico API pair, secret included — server-side callers only. Separate from
        /// GetAppliedAsync on purpose: that DTO is serialised to browsers, so the secret must have
        /// no route into it.
        /// </summary>
        Task<YubicoCredentials> GetYubicoCredentialsAsync();

        /// <summary>Switches plus the numbers an admin needs before flipping enforcement.</summary>
        Task<WebAuthnSettingStatusDto> GetStatusAsync(int callerUserId);

        /// <summary>
        /// Persists the switches. Refuses to turn enforcement on while doing so would lock the
        /// organisation out — that check lives here, not in the page, because a UI guard sits
        /// above the trust boundary and Swagger, curl and a stale tab all bypass it.
        /// </summary>
        Task<WebAuthnSettingStatusDto> UpdateAsync(int callerUserId, string? updatedBy, WebAuthnSettingUpdateDto input);

        /// <summary>
        /// Removes registered keys across the organisation so that everybody re-registers through
        /// the current enrollment flow. The way out of a half-provisioned estate: keys registered
        /// before enrollment collected a PIN still work at the password door but are turned away
        /// at the passwordless one, and nothing short of re-registering fixes an individual row.
        ///
        /// Super admins keep their keys — they are the way back if this goes wrong. Authenticators
        /// are left alone too: a working authenticator is never part of the broken state, and
        /// wiping it would cost people their recovery codes for nothing.
        /// </summary>
        Task<SecurityKeyResetSummaryDto> ResetAllKeysAsync(int callerUserId, string? performedBy);

        /// <summary>
        /// Whether this named account may be shown a password box at all. True for everyone while
        /// the organisation-wide switch is on; once it is off, only accounts that were granted the
        /// exception. The login screen asks this before drawing the box, so somebody who was never
        /// granted it never sees a field they cannot use.
        /// </summary>
        Task<bool> IsPasswordDoorOpenAsync(string? userName);

        /// <summary>
        /// Every active account with its key count and its password permission. The page that
        /// switches passwords off has to show WHO loses the door, not only how many.
        /// </summary>
        Task<List<WebAuthnUserAccessDto>> GetUserAccessAsync();

        /// <summary>
        /// Grants or takes back one account's password door. The way a person without a key gets
        /// in once to register one while the organisation-wide switch is off.
        /// </summary>
        Task<WebAuthnUserAccessDto> SetPasswordLoginAllowedAsync(int userId, bool allowed, string? changedBy);

        /// <summary>The saved states of these settings, newest first. Secrets never travel.</summary>
        Task<List<WebAuthnSettingHistoryDto>> GetHistoryAsync(int take);

        /// <summary>
        /// Puts the Yubico pair from a saved state back. This is what the history is for: a wrong
        /// client id or secret breaks every key touch, the secret cannot be read back off the
        /// page, and the only other remedy was wiping every registered key in the company.
        /// </summary>
        Task<WebAuthnSettingStatusDto> RestoreYubicoAsync(int callerUserId, string? restoredBy, int historyId);
    }
}
