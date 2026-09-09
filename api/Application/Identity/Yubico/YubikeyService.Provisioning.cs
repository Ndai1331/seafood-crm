using System.Net;
using Contract.Identity.UserManager;
using Contract.Identity.WebAuthn;
using Contract.Identity.Yubico;
using Core.Exceptions;
using Domain.Identity.UserYubikeys;
using Domain.Identity.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Application.Identity.Yubico
{
    /// <summary>Registering a key, setting its PIN, and taking it back.</summary>
    public partial class YubikeyService
    {
        /// <summary>
        /// One touch is the whole registration. Yubico's service confirms the OTP came from a real
        /// key, and the 12 characters at its head become that key's identity here — nothing secret
        /// changes hands, so there is nothing for an administrator to hand out first.
        /// </summary>
        public async Task<YubikeyDto> EnrollAsync(int userId, YubikeyEnrollDto input)
        {
            var user = await GetActiveUserAsync(userId);
            return await RegisterKeyAsync(user, input.Otp);
        }

        /// <summary>
        /// The same registration, reached from the login screen instead of the profile page.
        ///
        /// Enforcement used to send a keyless account to a dead end: the screen told them to sign
        /// in somewhere else and register there, which is impossible when every screen demands the
        /// key they are trying to register. The password (and the authenticator code, when they
        /// run one) is already proven by the time an enrollment token is minted — this endpoint
        /// adds no new trust, it only puts the registration where the person actually is.
        /// </summary>
        public async Task<YubikeyEnrollResultDto> EnrollAtLoginAsync(YubikeyEnrollWithTokenDto input)
        {
            var identity = _tokenValidator.Validate(input.EnrollToken, WebAuthnTokenTypes.Enrollment);
            var user = await GetActiveUserAsync(identity.UserId);

            // Replay defence without a stored "used" marker, copied from the passkey path: the
            // token is only good while the account still has no key, so finishing enrollment
            // invalidates it by construction. Both tables count — a passkey registered a second
            // ago is a key, and this token can mint a session.
            await EnsureNoKeyYetAsync(user.Id);

            var key = await RegisterKeyAsync(user, input.Otp);
            var progress = await BuildSetupProgressAsync(user);

            return new YubikeyEnrollResultDto
            {
                Key = key,
                Token = progress.Token,
                SetupToken = progress.SetupToken,
                NeedsPin = progress.NeedsPin,
                NeedsAuthenticator = progress.NeedsAuthenticator
            };
        }

        /// <summary>
        /// The PIN step. Reached with a setup token, which exists precisely because the session is
        /// still being withheld: a key with no PIN is turned away at the passwordless door, so
        /// registering one and stopping there hands the person a key that cannot sign them in.
        /// </summary>
        public async Task<YubikeySetupProgressDto> SetPinAtLoginAsync(YubikeySetupPinDto input)
        {
            var identity = _tokenValidator.Validate(input.SetupToken, WebAuthnTokenTypes.Setup);
            var user = await GetActiveUserAsync(identity.UserId);

            // Replay defence of the same shape as enrollment's: the token only opens a step the
            // account still owes, so finishing the step closes it.
            if (await HasPinAsync(user.Id))
            {
                throw new GlobalException(
                    "Khoá đã có mã PIN. Hãy đăng nhập lại.", HttpStatusCode.BadRequest);
            }

            await SetPinAsync(user.Id, new YubikeySetPinDto { Pin = input.Pin, ConfirmPin = input.ConfirmPin });

            return await BuildSetupProgressAsync(user);
        }

        /// <summary>
        /// What the account still owes, and either the token to pay it with or the session that
        /// says it owes nothing. One place decides this so the enrollment reply and every step
        /// after it agree on when a person is finished.
        /// </summary>
        private async Task<YubikeySetupProgressDto> BuildSetupProgressAsync(User user)
        {
            var needsPin = !await HasPinAsync(user.Id);

            // The authenticator is the backup that survives a broken key, so it is only demanded
            // where the feature exists at all. With it switched off the PIN is the whole wizard.
            var totpEnabled = _configuration.GetValue<bool>("Totp:Enabled");
            var needsAuthenticator = totpEnabled && !user.IsTotpEnabled;

            if (!needsPin && !needsAuthenticator)
            {
                return new YubikeySetupProgressDto
                {
                    Token = await _userManagerService.IssueTokenForUserAsync(
                        user.Id, proof: SecondFactorProof.SecurityKey)
                };
            }

            return new YubikeySetupProgressDto
            {
                NeedsPin = needsPin,
                NeedsAuthenticator = needsAuthenticator,
                SetupToken = await _userManagerService.CreateCeremonyTokenAsync(
                    user.Id, WebAuthnTokenTypes.Setup, GetSetupTokenExpiryMinutes())
            };
        }

        private async Task<bool> HasPinAsync(int userId) =>
            await _context.UserYubikeys.AnyAsync(x => x.UserId == userId && x.PinHash != null);

        /// <summary>
        /// Longer than the enrollment token on purpose: this one has to survive someone finding
        /// their phone, installing an authenticator app and scanning a QR code.
        /// </summary>
        private int GetSetupTokenExpiryMinutes()
        {
            var minutes = _configuration.GetValue<int?>("WebAuthn:SetupTokenExpiryMinutes") ?? 20;
            return minutes <= 0 ? 20 : minutes;
        }

        private async Task EnsureNoKeyYetAsync(int userId)
        {
            var hasKey = await _context.UserYubikeys.AnyAsync(x => x.UserId == userId)
                      || await _context.UserSecurityKeys.AnyAsync(x => x.UserId == userId);

            if (hasKey)
            {
                throw new GlobalException(
                    "Tài khoản đã có khoá bảo mật. Hãy đăng nhập lại.", HttpStatusCode.BadRequest);
            }
        }

        private async Task<YubikeyDto> RegisterKeyAsync(User user, string? submittedOtp)
        {
            var userId = user.Id;
            var otp = (submittedOtp ?? string.Empty).Trim();
            var publicId = YubicoOtpVerifier.ExtractPublicId(otp);

            if (publicId is null || otp.Length != ModhexCodec.OtpLength || !ModhexCodec.IsModhex(otp))
            {
                throw new GlobalException(
                    "Chưa nhận đủ tín hiệu từ khoá. Chạm và giữ khoá lâu hơn một chút.",
                    HttpStatusCode.BadRequest);
            }

            // Table-wide, not per account: a key's public id is fixed for life, so it identifies the
            // hardware. Two accounts claiming it would make an arriving OTP ambiguous about who is
            // signing in — the exact hole that lets one key open somebody else's account.
            var existing = await _context.UserYubikeys.AsNoTracking()
                .FirstOrDefaultAsync(x => x.PublicId == publicId);

            if (existing is not null)
            {
                throw new GlobalException(
                    existing.UserId == userId
                        ? "Khoá này đã được đăng ký cho tài khoản của bạn rồi."
                        : "Khoá này đã được đăng ký cho một tài khoản khác.",
                    HttpStatusCode.Conflict);
            }

            var cloud = await _cloudClient.VerifyAsync(otp);
            if (!cloud.IsValid)
            {
                _logger.LogWarning(
                    "YubiKey enrollment refused for user {UserId}: {Status}.", userId, cloud.Status);
                throw new GlobalException(cloud.Message, HttpStatusCode.BadRequest);
            }

            var key = new UserYubikey
            {
                UserId = userId,
                PublicId = publicId,
                DeviceName = "YubiKey",
                LastUsedAt = DateTime.UtcNow
            };

            _context.UserYubikeys.Add(key);
            await _context.SaveChangesAsync();

            await _notifier.KeyRegisteredAsync(user, key.DeviceName);

            return new YubikeyDto
            {
                Id = key.Id,
                PublicId = key.PublicId,
                DeviceName = key.DeviceName,
                HasPin = false,
                Confirmed = true,
                CreatedAt = key.CreatedAt,
                LastUsedAt = key.LastUsedAt
            };
        }

        public async Task SetPinAsync(int userId, YubikeySetPinDto input)
        {
            if (input.Pin != input.ConfirmPin)
            {
                throw new GlobalException("Hai lần nhập PIN không khớp nhau.", HttpStatusCode.BadRequest);
            }

            if (string.IsNullOrEmpty(input.Pin) || input.Pin.Length < MinPinLength || input.Pin.Length > MaxPinLength)
            {
                throw new GlobalException(
                    $"Mã PIN phải từ {MinPinLength} đến {MaxPinLength} ký tự.", HttpStatusCode.BadRequest);
            }

            var user = await GetActiveUserAsync(userId);
            var keys = await _context.UserYubikeys.Where(x => x.UserId == userId).ToListAsync();
            if (keys.Count == 0)
            {
                throw new GlobalException(
                    "Hãy đăng ký khoá trước khi đặt mã PIN.", HttpStatusCode.BadRequest);
            }

            // One PIN across every key the person holds. A spare key exists so a lost one does not
            // lock someone out; a separate PIN per key would just move the lockout onto the PIN.
            var hash = _pinHasher.HashPassword(user, input.Pin);
            foreach (var key in keys)
            {
                key.PinHash = hash;
                key.PinFailedCount = 0;
                key.PinLockoutEnd = null;
            }

            await _context.SaveChangesAsync();
        }

        public async Task<AdminYubikeyStatusDto> AdminGetStatusAsync(int forUserId)
        {
            var keys = await _context.UserYubikeys
                .AsNoTracking()
                .Where(x => x.UserId == forUserId)
                .ToListAsync();

            return new AdminYubikeyStatusDto
            {
                KeyCount = keys.Count,
                AnyConfirmed = keys.Any(k => k.LastUsedAt.HasValue),
                LastUsedAt = keys.Where(k => k.LastUsedAt.HasValue).Max(k => k.LastUsedAt)
            };
        }

        public async Task AdminResetAsync(int adminUserId, int forUserId)
        {
            var owner = await GetActiveUserAsync(forUserId);
            var keys = await _context.UserYubikeys.Where(x => x.UserId == forUserId).ToListAsync();
            if (keys.Count == 0)
            {
                return;
            }

            _context.UserYubikeys.RemoveRange(keys);

            // Revoking the keys without killing the session leaves whoever is already inside still
            // inside — and a suspected takeover is one of the two reasons this button gets pressed.
            // Refresh tokens carry no expiry of their own, so clearing it is what ends the session.
            owner.RefreshToken = null;
            var updated = await _userManager.UpdateAsync(owner);
            if (!updated.Succeeded)
            {
                throw new GlobalException(
                    "Không thể huỷ phiên đăng nhập của tài khoản này. Chưa gỡ khoá.",
                    HttpStatusCode.InternalServerError);
            }

            await _context.SaveChangesAsync();

            _logger.LogWarning(
                "Admin {AdminId} reset every YubiKey for user {UserId}.", adminUserId, forUserId);

            await _notifier.KeyRemovedAsync(owner, "YubiKey", removedByAdmin: true);
        }

        /// <summary>
        /// Whether this public id is registered to somebody else. Used only to choose between two
        /// wordings — "nobody registered this key" and "this is not your key" send the person to
        /// two different places.
        /// </summary>
        private async Task<bool> PublicIdExistsElsewhereAsync(string otp, int userId)
        {
            var publicId = YubicoOtpVerifier.ExtractPublicId(otp);
            if (publicId is null) return false;

            return await _context.UserYubikeys
                .AsNoTracking()
                .AnyAsync(x => x.PublicId == publicId && x.UserId != userId);
        }
    }
}
