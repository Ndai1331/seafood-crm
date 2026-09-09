using System.Net;
using Contract.Identity.WebAuthn;
using Core.Exceptions;
using Core.Const;
using Domain.Identity.Users;
using Domain.WebAuthnSettings;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SqlServ4r.EntityFramework;
using Volo.Abp.DependencyInjection;

namespace Application.Identity.WebAuthn
{
    /// <summary>
    /// Reads the single webauthn_settings row.
    ///
    /// Two behaviours here are deliberate and load-bearing:
    ///
    ///   - An empty table returns defaults and does NOT insert. This runs on the login path; a
    ///     read must never turn into a write.
    ///   - Any failure (missing table during a deploy window, connection blip) degrades to
    ///     "feature off" instead of propagating. Without this, shipping the code before the DDL
    ///     turns every sign-in into a 502 that the UI renders as "wrong password".
    /// </summary>
    public class WebAuthnSettingService : IWebAuthnSettingService, ITransientDependency
    {
        private const string CacheKey = "webauthn_settings_applied";
        private static readonly TimeSpan CacheDuration = TimeSpan.FromSeconds(30);

        private readonly DreamContext _context;
        private readonly IConfiguration _configuration;
        private readonly IMemoryCache _memoryCache;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<WebAuthnSettingService> _logger;

        public WebAuthnSettingService(
            DreamContext context,
            IConfiguration configuration,
            IMemoryCache memoryCache,
            UserManager<User> userManager,
            ILogger<WebAuthnSettingService> logger)
        {
            _context = context;
            _configuration = configuration;
            _memoryCache = memoryCache;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<WebAuthnSettingDto> GetAppliedAsync()
        {
            if (_memoryCache.TryGetValue(CacheKey, out WebAuthnSettingDto? cached) && cached != null)
            {
                return cached;
            }

            var applied = await ReadAsync();
            _memoryCache.Set(CacheKey, applied, CacheDuration);
            return applied;
        }

        /// <summary>
        /// The Yubico pair for server-side use. Reads the row directly rather than the cached
        /// applied-settings DTO, because that DTO deliberately carries no secret.
        ///
        /// Database first, configuration second. Production runs from environment variables today
        /// and keeps doing so until somebody fills these in; test lost its variables when the host
        /// directory holding them was deleted, and the database is what survives a container being
        /// rebuilt.
        /// </summary>
        public async Task<YubicoCredentials> GetYubicoCredentialsAsync()
        {
            string? clientId = null;
            string? secret = null;

            try
            {
                var setting = await _context.WebAuthnSettings
                    .AsNoTracking()
                    .OrderBy(x => x.Id)
                    .FirstOrDefaultAsync();

                clientId = setting?.YubicoClientId;
                secret = setting?.YubicoSecretKey;
            }
            catch (Exception ex)
            {
                // Same reason ReadAsync swallows: a missing column during a deploy window must not
                // throw on the login path. Falling through to configuration is the safe answer.
                _logger.LogWarning(ex, "Could not read the Yubico credentials from webauthn_settings.");
            }

            // Each half falls back on its own: a half-filled row should not silently disable a
            // working environment variable.
            if (string.IsNullOrWhiteSpace(clientId)) clientId = _configuration["Yubico:ClientId"];
            if (string.IsNullOrWhiteSpace(secret)) secret = _configuration["Yubico:SecretKey"];

            return new YubicoCredentials(clientId, secret);
        }

        public async Task<WebAuthnSettingStatusDto> GetStatusAsync(int callerUserId)
        {
            var applied = await ReadAsync();
            var status = new WebAuthnSettingStatusDto
            {
                IsEnabled = applied.IsEnabled,
                IsEnforced = applied.IsEnforced,
                AllowPasswordLogin = applied.AllowPasswordLogin,
                YubicoClientId = applied.YubicoClientId,
                HasYubicoSecret = applied.HasYubicoSecret
            };

            status.ActiveUsers = await _context.Users.CountAsync(u => u.IsActive && !u.IsDelete);

            var keyCountsByUser = await CountKeysPerUserAsync();

            status.UsersWithKey = keyCountsByUser.Count;
            status.CallerHasKey = keyCountsByUser.ContainsKey(callerUserId);

            var superAdminIds = await GetActiveSuperAdminIdsAsync();
            status.ActiveSuperAdmins = superAdminIds.Count;
            status.SuperAdminsWithKey = keyCountsByUser.Count(k => superAdminIds.Contains(k.Key) && k.Value >= 1);

            // Neither a key nor a granted password door: the accounts that would find no way in.
            // The key side is matched in memory — the counts are already loaded, and a dictionary
            // does not translate into SQL.
            var withoutPassword = await _context.Users
                .Where(u => u.IsActive && !u.IsDelete && !u.PasswordLoginAllowed)
                .Select(u => u.Id)
                .ToListAsync();
            status.UsersLockedOut = withoutPassword.Count(id => !keyCountsByUser.ContainsKey(id));
            status.EnforcementBlockedReason = DescribeEnforcementBlock(status);
            status.PasswordDisableBlockedReason = DescribePasswordDisableBlock(status);

            return status;
        }

        public async Task<WebAuthnSettingStatusDto> UpdateAsync(
            int callerUserId, string? updatedBy, WebAuthnSettingUpdateDto input)
        {
            var current = await GetStatusAsync(callerUserId);

            // Only the transition into enforcement is gated. Turning it OFF must always work —
            // that is the way back when something goes wrong.
            if (input.IsEnabled && input.IsEnforced && !current.IsEnforced)
            {
                var blocked = DescribeEnforcementBlock(current);
                if (blocked != null)
                {
                    throw new GlobalException(blocked, HttpStatusCode.BadRequest);
                }
            }

            // Same shape, one notch more dangerous. Enforcement still lets someone enroll a key at
            // the login screen; switching passwords off leaves anyone without a key with no screen
            // to reach at all.
            if (!input.AllowPasswordLogin && current.AllowPasswordLogin)
            {
                var blocked = DescribePasswordDisableBlock(current);
                if (blocked != null)
                {
                    throw new GlobalException(blocked, HttpStatusCode.BadRequest);
                }
            }

            var row = await _context.WebAuthnSettings.OrderBy(x => x.Id).FirstOrDefaultAsync();
            if (row is null)
            {
                row = new WebAuthnSetting();
                await _context.WebAuthnSettings.AddAsync(row);
            }

            // Captured before the writes below so the history row can say what actually changed.
            var before = new WebAuthnSettingHistory
            {
                IsEnabled = row.IsEnabled,
                IsEnforced = row.IsEnforced,
                AllowPasswordLogin = row.AllowPasswordLogin,
                YubicoClientId = row.YubicoClientId,
                YubicoSecretKey = row.YubicoSecretKey
            };

            row.IsEnabled = input.IsEnabled;
            // Enforcement without the feature on is a state nobody can reason about later.
            row.IsEnforced = input.IsEnabled && input.IsEnforced;
            // Passwords come back on their own if the feature is switched off, otherwise turning
            // the feature off would leave a system with no way in at all.
            row.AllowPasswordLogin = input.AllowPasswordLogin || !input.IsEnabled;
            row.YubicoClientId = string.IsNullOrWhiteSpace(input.YubicoClientId)
                ? null
                : input.YubicoClientId.Trim();

            // Empty means "leave it alone". An administrator cannot read the stored secret back,
            // so treating a blank field as "erase" would wipe it every time they touched any
            // other switch on this page.
            if (!string.IsNullOrWhiteSpace(input.YubicoSecretKey))
            {
                row.YubicoSecretKey = input.YubicoSecretKey.Trim();
            }
            row.UpdatedBy = updatedBy;
            row.UpdatedAt = DateTime.UtcNow;

            RecordHistory(before, row, updatedBy);

            await _context.SaveChangesAsync();

            // Drop the cache immediately rather than waiting out the TTL: an admin who flips a
            // switch and sees no effect for half a minute will flip it again.
            _memoryCache.Remove(CacheKey);

            _logger.LogWarning(
                "WebAuthn switches set to enabled={Enabled} enforced={Enforced} by {UpdatedBy}",
                row.IsEnabled, row.IsEnforced, updatedBy ?? "unknown");

            return await GetStatusAsync(callerUserId);
        }

        /// <summary>
        /// Queues one history row per save. Never throws: this is a safety net, and a safety net
        /// that can stop an administrator from turning passwords back on is worse than none —
        /// the table is applied by hand, so "not there yet" is a real state during a deploy.
        /// </summary>
        private void RecordHistory(WebAuthnSettingHistory before, WebAuthnSetting after, string? changedBy)
        {
            try
            {
                var changed = new List<string>();
                if (before.IsEnabled != after.IsEnabled) changed.Add("Bật khoá bảo mật");
                if (before.IsEnforced != after.IsEnforced) changed.Add("Bắt buộc khoá");
                if (before.AllowPasswordLogin != after.AllowPasswordLogin) changed.Add("Đăng nhập mật khẩu");
                if (before.YubicoClientId != after.YubicoClientId) changed.Add("Yubico Client ID");
                if (before.YubicoSecretKey != after.YubicoSecretKey) changed.Add("Yubico Secret Key");

                // A save that changed nothing still gets a row: "somebody pressed save and the
                // values were already these" is information when a login breaks.
                _context.WebAuthnSettingHistories.Add(new WebAuthnSettingHistory
                {
                    IsEnabled = after.IsEnabled,
                    IsEnforced = after.IsEnforced,
                    AllowPasswordLogin = after.AllowPasswordLogin,
                    YubicoClientId = after.YubicoClientId,
                    YubicoSecretKey = after.YubicoSecretKey,
                    ChangedFields = changed.Count == 0 ? "Không đổi gì" : string.Join(", ", changed),
                    ChangedBy = changedBy,
                    ChangedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not record the login-settings history row.");
            }
        }

        /// <summary>
        /// The saved states, newest first. The secret never travels: each row says whether one was
        /// stored and shows the client id, which is enough to recognise the moment things broke.
        /// </summary>
        public async Task<List<WebAuthnSettingHistoryDto>> GetHistoryAsync(int take)
        {
            try
            {
                var rows = await _context.WebAuthnSettingHistories
                    .AsNoTracking()
                    .OrderByDescending(x => x.ChangedAt)
                    .ThenByDescending(x => x.Id)
                    .Take(take <= 0 ? 20 : take)
                    .ToListAsync();

                return rows.Select(r => new WebAuthnSettingHistoryDto
                {
                    Id = r.Id,
                    IsEnabled = r.IsEnabled,
                    IsEnforced = r.IsEnforced,
                    AllowPasswordLogin = r.AllowPasswordLogin,
                    YubicoClientId = r.YubicoClientId,
                    HasYubicoSecret = !string.IsNullOrWhiteSpace(r.YubicoSecretKey),
                    ChangedFields = r.ChangedFields,
                    ChangedBy = r.ChangedBy,
                    ChangedAt = r.ChangedAt
                }).ToList();
            }
            catch (Exception ex)
            {
                // Same reason the write swallows: the page must still render before the table
                // has been applied.
                _logger.LogWarning(ex, "Could not read the login-settings history.");
                return new List<WebAuthnSettingHistoryDto>();
            }
        }

        /// <summary>
        /// Copies the Yubico pair from a saved state back into the live settings. The whole point
        /// of keeping the history: the alternative to restoring a known-good pair was wiping every
        /// registered key in the company.
        ///
        /// Only the pair moves. The switches stay as they are, because an admin restoring
        /// credentials is fixing authentication, not asking to also re-open a door they closed
        /// since.
        /// </summary>
        public async Task<WebAuthnSettingStatusDto> RestoreYubicoAsync(int callerUserId, string? restoredBy, int historyId)
        {
            var snapshot = await _context.WebAuthnSettingHistories
                               .AsNoTracking()
                               .FirstOrDefaultAsync(x => x.Id == historyId)
                           ?? throw new GlobalException(
                               "Không tìm thấy mốc lịch sử này.", HttpStatusCode.NotFound);

            if (string.IsNullOrWhiteSpace(snapshot.YubicoClientId)
                && string.IsNullOrWhiteSpace(snapshot.YubicoSecretKey))
            {
                throw new GlobalException(
                    "Mốc này không có cặp khoá Yubico nào để khôi phục.", HttpStatusCode.BadRequest);
            }

            var row = await _context.WebAuthnSettings.OrderBy(x => x.Id).FirstOrDefaultAsync();
            if (row is null)
            {
                row = new WebAuthnSetting();
                await _context.WebAuthnSettings.AddAsync(row);
            }

            var before = new WebAuthnSettingHistory
            {
                IsEnabled = row.IsEnabled,
                IsEnforced = row.IsEnforced,
                AllowPasswordLogin = row.AllowPasswordLogin,
                YubicoClientId = row.YubicoClientId,
                YubicoSecretKey = row.YubicoSecretKey
            };

            row.YubicoClientId = snapshot.YubicoClientId;
            row.YubicoSecretKey = snapshot.YubicoSecretKey;
            row.UpdatedBy = restoredBy;
            row.UpdatedAt = DateTime.UtcNow;

            RecordHistory(before, row, restoredBy);

            await _context.SaveChangesAsync();
            _memoryCache.Remove(CacheKey);

            _logger.LogWarning(
                "Yubico credentials restored from history {HistoryId} ({ChangedAt:u}) by {RestoredBy}.",
                snapshot.Id, snapshot.ChangedAt, restoredBy ?? "unknown");

            return await GetStatusAsync(callerUserId);
        }

        /// <summary>
        /// Wipes registered keys across the organisation, super admins excepted. Irreversible, and
        /// deliberately blunt: the alternative is resetting 121 accounts one at a time.
        ///
        /// Super admins keep theirs because they are the way back. Wiping every key at once would
        /// leave the enforcement guard at zero super admins holding a key, and the person who just
        /// pressed the button without one either — the state this action exists to repair. Their
        /// own keys are re-done one at a time from the user list.
        /// </summary>
        public async Task<SecurityKeyResetSummaryDto> ResetAllKeysAsync(int callerUserId, string? performedBy)
        {
            var applied = await ReadAsync();

            // The one case where this bricks the organisation instead of resetting it: with
            // passwords off, a key is the only way in, and there would be no key left to use.
            if (!applied.AllowPasswordLogin)
            {
                throw new GlobalException(
                    "Đang tắt đăng nhập bằng mật khẩu. Bật lại trước khi xoá khoá, "
                  + "nếu không sẽ không ai đăng nhập được nữa.",
                    HttpStatusCode.BadRequest);
            }

            // By role, not by active flag: a deactivated super admin still holds the role, and the
            // point here is to never touch that group's keys.
            var protectedIds = (await _userManager.GetUsersInRoleAsync(RoleNames.SuperAdmin))
                .Select(u => u.Id)
                .ToHashSet();

            var yubikeys = await _context.UserYubikeys
                .Where(k => !protectedIds.Contains(k.UserId))
                .ToListAsync();

            var passkeys = await _context.UserSecurityKeys
                .Where(k => !protectedIds.Contains(k.UserId))
                .ToListAsync();

            var ownerIds = yubikeys.Select(k => k.UserId)
                .Concat(passkeys.Select(k => k.UserId))
                .Distinct()
                .ToList();

            _context.UserYubikeys.RemoveRange(yubikeys);
            _context.UserSecurityKeys.RemoveRange(passkeys);

            // Same reason the single-user reset does it: a session opened with a key that no
            // longer exists should not outlive the key. Access tokens carry their own 24h life,
            // so this ends the renewal rather than the current request.
            var owners = await _userManager.Users
                .Where(u => ownerIds.Contains(u.Id) && u.RefreshToken != null)
                .ToListAsync();

            foreach (var owner in owners)
            {
                owner.RefreshToken = null;
            }

            await _context.SaveChangesAsync();

            _logger.LogWarning(
                "Security keys removed by {PerformedBy} (user {CallerUserId}): "
              + "{Yubikeys} YubiKeys and {Passkeys} passkeys across {Users} accounts. "
              + "{Protected} super admin accounts were left alone.",
                performedBy ?? "unknown", callerUserId, yubikeys.Count, passkeys.Count,
                ownerIds.Count, protectedIds.Count);

            return new SecurityKeyResetSummaryDto
            {
                UsersAffected = ownerIds.Count,
                YubikeysRemoved = yubikeys.Count,
                PasskeysRemoved = passkeys.Count
            };
        }

        /// <summary>
        /// The organisation-lockout guard. Returns null when enforcement is safe to turn on.
        /// </summary>
        private static string? DescribeEnforcementBlock(WebAuthnSettingStatusDto status)
        {
            if (!status.CallerHasKey)
            {
                return "Bạn chưa đăng ký khoá bảo mật nào. Hãy đăng ký ở trang Hồ sơ trước khi bật bắt buộc, "
                     + "nếu không chính bạn sẽ phải đăng ký ngay ở màn đăng nhập lần sau.";
            }

            // One is enough here, unlike the password-disable guard below. Enforcement leaves
            // passwords working, so a super admin who loses their key still signs in with one and
            // either registers a replacement at the login screen or switches this back off. The
            // second holder was insurance against a dead end that enrollment-at-login removed.
            if (status.SuperAdminsWithKey < 1)
            {
                return "Cần ít nhất 1 super admin đã có khoá. Đây là đường thoát khi có sự cố: "
                     + "người đó tắt tính năng hộ.";
            }

            // Users without a key are deliberately NOT a blocker. Enforcement now walks them
            // through registering one at the login screen itself, so "chưa có khoá" is a step they
            // take on the way in, not a wall.
            return null;
        }

        /// <summary>
        /// The guard behind switching password login off. Returns null when it is safe.
        ///
        /// Stricter than the enforcement guard on purpose, and the extra rule is that EVERY active
        /// user must already hold a key. Enforcement can send a keyless person through enrollment
        /// at the login screen because they still have a password to get that far; with passwords
        /// gone there is no first step left, so a keyless account is simply shut out.
        /// </summary>
        private static string? DescribePasswordDisableBlock(WebAuthnSettingStatusDto status)
        {
            if (!status.IsEnabled)
            {
                return "Hãy bật tính năng khoá bảo mật trước khi tắt đăng nhập bằng mật khẩu.";
            }

            if (!status.CallerHasKey)
            {
                return "Bạn chưa đăng ký khoá bảo mật nào. Tắt mật khẩu bây giờ là chính bạn mất đường vào.";
            }

            // Keyless accounts are deliberately NOT a blocker any more. Most accounts here exist so
            // a domain can be assigned to a PIC and will never hold a key; requiring all 121 of them
            // made this switch unreachable. Who loses the door is shown on the page, account by
            // account, and anyone who still needs a password gets one granted to them.
            if (status.SuperAdminsWithKey < 2)
            {
                return $"Cần ít nhất 2 super admin đã có khoá (hiện có {status.SuperAdminsWithKey}) "
                     + "trước khi bỏ hẳn mật khẩu.";
            }

            return null;
        }

        /// <summary>
        /// Whether the login screen may draw a password box for this name.
        ///
        /// Asked before anything is typed, so the answer is deliberately thin: an unknown name and
        /// a known name without the exception both come back false. It never says whether the
        /// account exists, and it is not a credential check — the sign-in endpoint still re-checks
        /// the same permission after the password, which is what actually keeps anyone out.
        /// </summary>
        public async Task<bool> IsPasswordDoorOpenAsync(string? userName)
        {
            var applied = await GetAppliedAsync();
            if (applied.AllowPasswordLogin)
            {
                return true;
            }

            if (string.IsNullOrWhiteSpace(userName))
            {
                return false;
            }

            var name = userName.Trim();
            try
            {
                return await _context.Users.AsNoTracking()
                    .AnyAsync(u => u.UserName == name
                                && u.IsActive && !u.IsDelete
                                && u.PasswordLoginAllowed);
            }
            catch (Exception ex)
            {
                // Same rule as everything else on the login path: a database hiccup must not draw
                // a box that cannot work. Closed is the safe answer here.
                _logger.LogWarning(ex, "Could not read the password permission for {UserName}.", name);
                return false;
            }
        }

        /// <summary>
        /// Every active account with its key count and its password permission — the list behind
        /// the "12/121" on the settings page, which said how many but never who.
        /// </summary>
        public async Task<List<WebAuthnUserAccessDto>> GetUserAccessAsync()
        {
            var keyCounts = await CountKeysPerUserAsync();
            var superAdminIds = await GetActiveSuperAdminIdsAsync();

            var users = await _context.Users
                .AsNoTracking()
                .Where(u => u.IsActive && !u.IsDelete)
                .Select(u => new
                {
                    u.Id,
                    u.UserName,
                    u.FirstName,
                    u.LastName,
                    u.PasswordLoginAllowed
                })
                .ToListAsync();

            return users
                .Select(u => new WebAuthnUserAccessDto
                {
                    UserId = u.Id,
                    UserName = u.UserName ?? string.Empty,
                    FullName = $"{u.FirstName} {u.LastName}".Trim(),
                    KeyCount = keyCounts.GetValueOrDefault(u.Id),
                    PasswordLoginAllowed = u.PasswordLoginAllowed,
                    IsSuperAdmin = superAdminIds.Contains(u.Id)
                })
                // Key holders first, then whoever was granted a password: the two groups that can
                // still get in once passwords are off, which is what this list is read for.
                .OrderByDescending(u => u.KeyCount > 0)
                .ThenByDescending(u => u.PasswordLoginAllowed)
                .ThenBy(u => u.UserName, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        /// <summary>
        /// Grants or takes back one account's password door. Deliberately not reachable by the
        /// account itself — this is an admin handing somebody a way in to register their key.
        /// </summary>
        public async Task<WebAuthnUserAccessDto> SetPasswordLoginAllowedAsync(
            int userId, bool allowed, string? changedBy)
        {
            var user = await _context.Users
                           .FirstOrDefaultAsync(u => u.Id == userId && u.IsActive && !u.IsDelete)
                       ?? throw new GlobalException(
                           "Không tìm thấy user đang hoạt động này.", HttpStatusCode.NotFound);

            user.PasswordLoginAllowed = allowed;
            await _context.SaveChangesAsync();

            _logger.LogWarning(
                "Password login {State} for user {UserId} ({UserName}) by {ChangedBy}.",
                allowed ? "granted" : "revoked", user.Id, user.UserName, changedBy ?? "unknown");

            var keyCounts = await CountKeysPerUserAsync();
            var superAdminIds = await GetActiveSuperAdminIdsAsync();

            return new WebAuthnUserAccessDto
            {
                UserId = user.Id,
                UserName = user.UserName ?? string.Empty,
                FullName = $"{user.FirstName} {user.LastName}".Trim(),
                KeyCount = keyCounts.GetValueOrDefault(user.Id),
                PasswordLoginAllowed = user.PasswordLoginAllowed,
                IsSuperAdmin = superAdminIds.Contains(user.Id)
            };
        }

        /// <summary>
        /// Keys per user across BOTH tables.
        ///
        /// This counted user_security_keys alone while the login path
        /// (UserManagerService.LookupSecurityKeysAsync) already read both. Once YubiKeys replaced
        /// WebAuthn the WebAuthn table emptied, so this reported nobody had a key — the settings
        /// page showed 0, and the enforcement guard refused every admin who was holding a working
        /// key. It failed safe, but it made the way into enforcement impassable.
        /// </summary>
        private async Task<Dictionary<int, int>> CountKeysPerUserAsync()
        {
            var webAuthn = await _context.UserSecurityKeys
                .AsNoTracking()
                .GroupBy(k => k.UserId)
                .Select(g => new { UserId = g.Key, Count = g.Count() })
                .ToListAsync();

            var yubikeys = await _context.UserYubikeys
                .AsNoTracking()
                .GroupBy(k => k.UserId)
                .Select(g => new { UserId = g.Key, Count = g.Count() })
                .ToListAsync();

            return MergeKeyCounts(
                webAuthn.Select(r => (r.UserId, r.Count)),
                yubikeys.Select(r => (r.UserId, r.Count)));
        }

        /// <summary>
        /// The merge, kept pure so it can be tested without a database — the same reason
        /// WebAuthnLoginDecision lives on its own. Everything around it is welded to DreamContext.
        /// </summary>
        internal static Dictionary<int, int> MergeKeyCounts(
            IEnumerable<(int UserId, int Count)> webAuthn,
            IEnumerable<(int UserId, int Count)> yubikeys)
        {
            var counts = new Dictionary<int, int>();
            foreach (var (userId, count) in webAuthn.Concat(yubikeys))
            {
                counts[userId] = counts.GetValueOrDefault(userId) + count;
            }

            return counts;
        }

        /// <summary>
        /// Super admins only, not every administrator. They are the accounts that can switch this
        /// feature off again, which is what makes them the escape hatch the guard counts.
        /// </summary>
        private async Task<HashSet<int>> GetActiveSuperAdminIdsAsync()
        {
            var ids = new HashSet<int>();
            foreach (var user in await _userManager.GetUsersInRoleAsync(RoleNames.SuperAdmin))
            {
                if (user.IsActive && !user.IsDelete)
                {
                    ids.Add(user.Id);
                }
            }

            return ids;
        }

        private async Task<WebAuthnSettingDto> ReadAsync()
        {
            try
            {
                var setting = await _context.WebAuthnSettings
                    .AsNoTracking()
                    .OrderBy(x => x.Id)
                    .FirstOrDefaultAsync();

                if (setting is null)
                {
                    return new WebAuthnSettingDto();
                }

                return new WebAuthnSettingDto
                {
                    IsEnabled = setting.IsEnabled,
                    IsEnforced = setting.IsEnforced,
                    AllowPasswordLogin = setting.AllowPasswordLogin,
                    YubicoClientId = setting.YubicoClientId,
                    // A flag, never the value — this DTO reaches a browser.
                    HasYubicoSecret = !string.IsNullOrWhiteSpace(setting.YubicoSecretKey)
                };
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not read webauthn_settings; treating the feature as disabled.");
                return new WebAuthnSettingDto();
            }
        }
    }
}
