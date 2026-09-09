using System.Net;
using Contract.Identity.WebAuthn;
using Core.Const;
using Core.Exceptions;
using Domain.Identity.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Identity.WebAuthn
{
    /// <summary>Key management and the admin recovery path.</summary>
    public partial class WebAuthnService
    {
        public async Task<List<SecurityKeyDto>> GetKeysAsync(int userId)
        {
            var keys = await _context.UserSecurityKeys
                .AsNoTracking()
                .Where(x => x.UserId == userId)
                .OrderBy(x => x.CreatedAt)
                .ToListAsync();

            return keys.Select(ToDto).ToList();
        }

        public async Task DeleteKeyAsync(int userId, int keyId)
        {
            var key = await _context.UserSecurityKeys
                .FirstOrDefaultAsync(x => x.Id == keyId && x.UserId == userId)
                ?? throw new GlobalException("Không tìm thấy khoá bảo mật.", HttpStatusCode.NotFound);

            var settings = await _settingService.GetAppliedAsync();
            var remaining = await _context.UserSecurityKeys.CountAsync(x => x.UserId == userId) - 1;

            // Enforced plus zero keys equals locked out, with an admin as the only way back. Not
            // something a user should be able to do to themselves with one click.
            if (settings.IsEnabled && settings.IsEnforced && remaining <= 0)
            {
                throw new GlobalException(
                    "Không thể xoá khoá cuối cùng. Hãy đăng ký khoá dự phòng trước.", HttpStatusCode.BadRequest);
            }

            var user = await GetActiveUserAsync(userId);
            _context.UserSecurityKeys.Remove(key);
            await _context.SaveChangesAsync();

            await _notifier.KeyRemovedAsync(user, key.DeviceName, removedByAdmin: false);
        }

        public async Task<AdminSecurityKeyStatusDto> AdminGetStatusAsync(int targetUserId)
        {
            var keys = await _context.UserSecurityKeys
                .AsNoTracking()
                .Where(x => x.UserId == targetUserId)
                .ToListAsync();

            return new AdminSecurityKeyStatusDto
            {
                KeyCount = keys.Count,
                LastUsedAt = keys.Where(x => x.LastUsedAt.HasValue).Max(x => x.LastUsedAt)
            };
        }

        public async Task AdminResetAsync(int adminUserId, int targetUserId)
        {
            var target = await GetActiveUserAsync(targetUserId);
            var keys = await _context.UserSecurityKeys.Where(x => x.UserId == targetUserId).ToListAsync();

            if (keys.Count == 0)
            {
                return;
            }

            await EnsureNotTheLastAdminWithAKeyAsync(target);

            _context.UserSecurityKeys.RemoveRange(keys);

            // Revoking the keys without killing the session leaves whoever is already inside still
            // inside — and "the account was taken over" is one of the two reasons this button gets
            // pressed. Refresh tokens have no expiry of their own, so this is what ends the session.
            target.RefreshToken = null;
            var sessionKilled = await _userManager.UpdateAsync(target);
            if (!sessionKilled.Succeeded)
            {
                // "Account was taken over" is one of the two reasons this button gets pressed, so
                // reporting success while the intruder's session survives would be the worst
                // possible outcome. Fail loudly instead.
                throw new GlobalException(
                    "Đã gỡ khoá nhưng không đăng xuất được phiên của user. Hãy thử lại.",
                    HttpStatusCode.Conflict);
            }

            await _context.SaveChangesAsync();
            _ceremonyStore.ClearFailures(targetUserId);

            _logger.LogWarning(
                "Admin {AdminId} removed {Count} security key(s) from user {UserId}.",
                adminUserId, keys.Count, targetUserId);

            await _notifier.KeyRemovedAsync(target, $"{keys.Count} khoá", removedByAdmin: true);
        }

        /// <summary>
        /// Refuses to strip the last administrator who can still get in while keys are mandatory.
        /// Without this, one mis-click on the user list locks the whole organisation out and the
        /// only remedy is a manual UPDATE on the production database.
        /// </summary>
        private async Task EnsureNotTheLastAdminWithAKeyAsync(User target)
        {
            var settings = await _settingService.GetAppliedAsync();
            if (!settings.IsEnabled || !settings.IsEnforced)
            {
                return;
            }

            var targetRoles = await _userManager.GetRolesAsync(target);
            var targetIsAdmin = targetRoles.Any(IsAdminRole);
            if (!targetIsAdmin)
            {
                return;
            }

            // Both tables, like every other key count in this feature. Reading user_security_keys
            // alone made every YubiKey invisible here, so once YubiKeys replaced WebAuthn this saw
            // zero admins holding a key and refused perfectly safe removals.
            var passkeyOwners = await _context.UserSecurityKeys
                .AsNoTracking()
                .Select(x => x.UserId)
                .ToListAsync();

            var yubikeyOwners = await _context.UserYubikeys
                .AsNoTracking()
                .Select(x => x.UserId)
                .ToListAsync();

            var adminIdsWithKeys = passkeyOwners.Concat(yubikeyOwners).Distinct().ToList();

            var otherAdminsWithKeys = 0;
            foreach (var userId in adminIdsWithKeys.Where(id => id != target.Id))
            {
                var candidate = await _userManager.FindByIdAsync(userId.ToString());
                if (candidate == null || !candidate.IsActive || candidate.IsDelete)
                {
                    continue;
                }

                var roles = await _userManager.GetRolesAsync(candidate);
                if (roles.Any(IsAdminRole))
                {
                    otherAdminsWithKeys++;
                }
            }

            if (otherAdminsWithKeys == 0)
            {
                throw new GlobalException(
                    "Đây là quản trị viên cuối cùng còn khoá bảo mật. Gỡ khoá sẽ khoá cả hệ thống ra ngoài.",
                    HttpStatusCode.BadRequest);
            }
        }

        private static bool IsAdminRole(string role) =>
            role == RoleNames.Admin || role == RoleNames.SuperAdmin;

        private async Task<User> GetActiveUserAsync(int userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null || !user.IsActive || user.IsDelete)
            {
                throw new GlobalException("Không tìm thấy tài khoản.", HttpStatusCode.BadRequest);
            }

            return user;
        }

        private async Task EnsureEnrollmentStillOwedAsync(int userId)
        {
            var hasKey = await _context.UserSecurityKeys.AnyAsync(x => x.UserId == userId)
                      || await _context.UserYubikeys.AnyAsync(x => x.UserId == userId);

            if (hasKey)
            {
                throw new GlobalException(
                    "Tài khoản đã có khoá bảo mật. Hãy đăng nhập lại.", HttpStatusCode.BadRequest);
            }
        }
    }
}
