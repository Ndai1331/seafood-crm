using Application.Identity.WebAuthn;
using System.Net;
using Contract.Identity.UserManager;
using Contract.Identity.WebAuthn;
using Contract.Identity.Yubico;
using Core.Exceptions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Identity.Yubico
{
    /// <summary>The two steps between a correct password and a session.</summary>
    public partial class YubikeyService
    {
        /// <summary>
        /// Marker saying the PIN was accepted for one particular half-authenticated token. Without
        /// it, calling the OTP endpoint directly would walk straight past the PIN — the PIN screen
        /// is only a screen, and screens are not security.
        /// </summary>
        private static string PinClearedKey(string jti) => $"yubikey-pin-cleared:{jti}";

        private static readonly TimeSpan PinClearanceLifetime = TimeSpan.FromMinutes(2);

        public async Task<YubikeyPinResultDto> VerifyPinAsync(YubikeyPinDto input)
        {
            var identity = _tokenValidator.Validate(input.TemporaryToken, WebAuthnTokenTypes.SecurityKeyStep);

            var keys = await _context.UserYubikeys.Where(x => x.UserId == identity.UserId).ToListAsync();
            var key = keys.FirstOrDefault(k => k.PinHash != null);

            // No PIN set yet: the step does not apply, so let the touch step open rather than
            // blocking someone out of an account that simply has not been through PIN setup.
            if (key is null)
            {
                _ceremonyStore.SaveChallenge(PinClearedKey(identity.Jti), "1", PinClearanceLifetime);
                return new YubikeyPinResultDto { Ok = true };
            }

            if (key.PinLockoutEnd is { } until && until > DateTime.UtcNow)
            {
                return new YubikeyPinResultDto
                {
                    Ok = false,
                    LockedForMinutes = Math.Max(1, (int)Math.Ceiling((until - DateTime.UtcNow).TotalMinutes)),
                    Error = "Nhập sai quá nhiều lần. Thử lại sau ít phút."
                };
            }

            var user = await GetActiveUserAsync(identity.UserId);
            var verified = _pinHasher.VerifyHashedPassword(user, key.PinHash!, input.Pin ?? string.Empty);

            if (verified == PasswordVerificationResult.Failed)
            {
                return await RecordPinFailureAsync(keys, identity.UserId);
            }

            foreach (var k in keys)
            {
                k.PinFailedCount = 0;
                k.PinLockoutEnd = null;
            }
            await _context.SaveChangesAsync();

            _ceremonyStore.SaveChallenge(PinClearedKey(identity.Jti), "1", PinClearanceLifetime);
            return new YubikeyPinResultDto { Ok = true };
        }

        public async Task<TokenDto> VerifyOtpAsync(YubikeyVerifyDto input)
        {
            var identity = _tokenValidator.Validate(input.TemporaryToken, WebAuthnTokenTypes.SecurityKeyStep);

            if (_ceremonyStore.IsLockedOut(identity.UserId))
            {
                throw new GlobalException(
                    "Đã thử sai quá nhiều lần. Hãy đợi ít phút rồi thử lại.", HttpStatusCode.TooManyRequests);
            }

            // One-shot: taking the marker here is what stops one PIN entry authorising a stream of
            // OTP attempts.
            if (_ceremonyStore.TakeChallenge(PinClearedKey(identity.Jti)) is null)
            {
                throw new GlobalException(
                    "Phiên xác minh đã hết hạn. Nhập lại mã PIN.", HttpStatusCode.BadRequest);
            }

            var keys = await _context.UserYubikeys.Where(x => x.UserId == identity.UserId).ToListAsync();
            var elsewhere = await PublicIdExistsElsewhereAsync(input.Otp, identity.UserId);

            // Ownership first, and only then the network call. Yubico answers "a real key made this
            // and it is unused" — never "this key belongs to that account". Asking them first and
            // trusting the OK would let any working key open any account.
            var owned = YubicoOtpVerifier.ResolveOwner(input.Otp, keys, elsewhere);
            if (!owned.IsValid)
            {
                _ceremonyStore.RecordFailure(identity.UserId);
                _logger.LogWarning(
                    "YubiKey OTP rejected for user {UserId}: {Status}.", identity.UserId, owned.Status);
                throw new GlobalException(owned.Message, HttpStatusCode.Unauthorized);
            }

            var cloud = await _cloudClient.VerifyAsync(input.Otp.Trim());
            if (!cloud.IsValid)
            {
                _ceremonyStore.RecordFailure(identity.UserId);
                _logger.LogWarning(
                    "Yubico refused the OTP for user {UserId}: {Status}.", identity.UserId, cloud.Status);
                throw new GlobalException(cloud.Message, HttpStatusCode.Unauthorized);
            }

            var key = keys.First(k => k.Id == owned.KeyId);
            key.LastUsedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            _ceremonyStore.ClearFailures(identity.UserId);

            return await _userManagerService.IssueTokenForUserAsync(
                identity.UserId, proof: SecondFactorProof.SecurityKey);
        }

        private async Task<YubikeyPinResultDto> RecordPinFailureAsync(
            List<Domain.Identity.UserYubikeys.UserYubikey> keys, int userId)
        {
            var failures = keys.Max(k => k.PinFailedCount) + 1;
            var locked = failures >= MaxPinAttempts;

            foreach (var k in keys)
            {
                k.PinFailedCount = locked ? 0 : failures;
                k.PinLockoutEnd = locked ? DateTime.UtcNow.AddMinutes(PinLockoutMinutes) : null;
            }
            await _context.SaveChangesAsync();

            if (locked)
            {
                _logger.LogWarning("YubiKey PIN locked out for user {UserId}.", userId);
                return new YubikeyPinResultDto
                {
                    Ok = false,
                    LockedForMinutes = PinLockoutMinutes,
                    Error = $"Nhập sai quá nhiều lần. Thử lại sau {PinLockoutMinutes} phút."
                };
            }

            var left = MaxPinAttempts - failures;
            return new YubikeyPinResultDto
            {
                Ok = false,
                AttemptsLeft = left,
                Error = $"Mã PIN không đúng. Còn {left} lần thử."
            };
        }
    }
}
