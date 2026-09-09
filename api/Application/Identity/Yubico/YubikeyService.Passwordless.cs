using System.Net;
using Application.Identity.WebAuthn;
using Contract.Identity.UserManager;
using Contract.Identity.WebAuthn;
using Contract.Identity.Yubico;
using Core.Exceptions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Identity.Yubico
{
    /// <summary>
    /// Signing in with nothing but the key and its PIN.
    ///
    /// The order is reversed from the second-factor path, and it has to be. There, a password has
    /// already named the account, so the PIN can be asked for first and the touch confirms it.
    /// Here the touch is what names the account — the 12 characters at the head of every OTP are
    /// that key's identity for life, and user_yubikeys.public_id is unique table-wide — so nobody
    /// can be asked for a PIN until the key has been read.
    ///
    /// That leaves two factors, not one: the key is what you hold, the PIN is what you know. The
    /// guard rail keeping it that way is the PinHash check below. Without it a found key would be
    /// the entire lock.
    /// </summary>
    public partial class YubikeyService
    {
        /// <summary>
        /// Marker proving a specific key was touched and Yubico vouched for the OTP. Holds the key
        /// id rather than a bare "yes": the PIN step must land on the row that was actually
        /// presented, not on whichever of the account's keys happens to be first.
        /// </summary>
        private static string PasswordlessKeyKey(string jti) => $"yubikey-passwordless-key:{jti}";

        private const int PasswordlessPinStepMinutes = 2;

        public async Task<YubikeyPasswordlessTouchResultDto> PasswordlessTouchAsync(
            YubikeyPasswordlessTouchDto input)
        {
            var settings = await _settingService.GetAppliedAsync();
            if (!settings.IsEnabled)
            {
                throw new GlobalException(
                    "Đăng nhập bằng khoá bảo mật đang tắt.", HttpStatusCode.BadRequest);
            }

            // Shape first, so a mistyped or half-captured string never costs a call to Yubico.
            var otp = (input.Otp ?? string.Empty).Trim();
            var publicId = YubicoOtpVerifier.ExtractPublicId(otp);

            if (publicId is null || otp.Length != ModhexCodec.OtpLength || !ModhexCodec.IsModhex(otp))
            {
                throw new GlobalException(
                    "Chưa nhận đủ tín hiệu từ khoá. Chạm và giữ khoá lâu hơn một chút.",
                    HttpStatusCode.BadRequest);
            }

            // Ownership before the network call, same order as the second-factor path. Yubico
            // answers "a real key made this and it is unused" and never "whose key it is", so
            // asking them first and trusting the OK would let any working key open any account.
            var key = await _context.UserYubikeys.FirstOrDefaultAsync(x => x.PublicId == publicId);
            if (key is null)
            {
                _logger.LogWarning("Passwordless login refused: unknown public id {PublicId}.", publicId);
                throw new GlobalException(
                    "Khoá này chưa được đăng ký cho tài khoản nào.", HttpStatusCode.Unauthorized);
            }

            // No PIN means the key alone would be the whole lock — one lost key and whoever picks
            // it up is signed in. Refuse, and send them through the password door to set one.
            if (key.PinHash is null)
            {
                throw new GlobalException(
                    "Khoá này chưa có mã PIN. Hãy đăng nhập bằng mật khẩu rồi đặt PIN ở trang Hồ sơ trước.",
                    HttpStatusCode.BadRequest);
            }

            if (_ceremonyStore.IsLockedOut(key.UserId))
            {
                throw new GlobalException(
                    "Đã thử sai quá nhiều lần. Hãy đợi ít phút rồi thử lại.",
                    HttpStatusCode.TooManyRequests);
            }

            var cloud = await _cloudClient.VerifyAsync(otp);
            if (!cloud.IsValid)
            {
                _ceremonyStore.RecordFailure(key.UserId);
                _logger.LogWarning(
                    "Yubico refused a passwordless OTP for user {UserId}: {Status}.",
                    key.UserId, cloud.Status);
                throw new GlobalException(cloud.Message, HttpStatusCode.Unauthorized);
            }

            // Minted through UserManagerService so issuer, audience and signing key match what
            // WebAuthnTokenValidator expects. Returns null when the account is inactive or gone,
            // which is the same answer an unknown key deserves.
            var token = await _userManagerService.CreateCeremonyTokenAsync(
                key.UserId, WebAuthnTokenTypes.PasswordlessPinStep, PasswordlessPinStepMinutes);

            if (token is null)
            {
                throw new GlobalException(
                    "Tài khoản gắn với khoá này không còn hoạt động.", HttpStatusCode.Unauthorized);
            }

            var identity = _tokenValidator.Validate(token, WebAuthnTokenTypes.PasswordlessPinStep);
            _ceremonyStore.SaveChallenge(
                PasswordlessKeyKey(identity.Jti),
                key.Id.ToString(),
                TimeSpan.FromMinutes(PasswordlessPinStepMinutes));

            return new YubikeyPasswordlessTouchResultDto { TemporaryToken = token };
        }

        public async Task<TokenDto> PasswordlessPinAsync(YubikeyPasswordlessPinDto input)
        {
            var identity = _tokenValidator.Validate(
                input.TemporaryToken, WebAuthnTokenTypes.PasswordlessPinStep);

            if (_ceremonyStore.IsLockedOut(identity.UserId))
            {
                throw new GlobalException(
                    "Đã thử sai quá nhiều lần. Hãy đợi ít phút rồi thử lại.",
                    HttpStatusCode.TooManyRequests);
            }

            // One-shot, and taken before the PIN is checked. A wrong PIN therefore costs a fresh
            // touch rather than another guess against the same OTP — which is what keeps a stolen
            // key from being walked through a four-digit space offline.
            var proven = _ceremonyStore.TakeChallenge(PasswordlessKeyKey(identity.Jti));
            if (proven is null || !int.TryParse(proven, out var provenKeyId))
            {
                throw new GlobalException(
                    "Phiên xác minh đã hết hạn. Chạm khoá lại.", HttpStatusCode.BadRequest);
            }

            var keys = await _context.UserYubikeys.Where(x => x.UserId == identity.UserId).ToListAsync();
            var key = keys.FirstOrDefault(k => k.Id == provenKeyId);

            // Gone between the two steps: an administrator can revoke a key mid-ceremony, and that
            // has to take effect immediately rather than after this login.
            if (key?.PinHash is null)
            {
                throw new GlobalException(
                    "Khoá này không còn dùng để đăng nhập được.", HttpStatusCode.Unauthorized);
            }

            if (key.PinLockoutEnd is { } until && until > DateTime.UtcNow)
            {
                var minutes = Math.Max(1, (int)Math.Ceiling((until - DateTime.UtcNow).TotalMinutes));
                throw new GlobalException(
                    $"Nhập sai quá nhiều lần. Thử lại sau {minutes} phút.",
                    HttpStatusCode.TooManyRequests);
            }

            var user = await GetActiveUserAsync(identity.UserId);
            var verified = _pinHasher.VerifyHashedPassword(user, key.PinHash, input.Pin ?? string.Empty);

            if (verified == PasswordVerificationResult.Failed)
            {
                var failure = await RecordPinFailureAsync(keys, identity.UserId);
                _ceremonyStore.RecordFailure(identity.UserId);
                throw new GlobalException(
                    failure.Error ?? "Mã PIN không đúng. Chạm khoá lại để thử lần nữa.",
                    HttpStatusCode.Unauthorized);
            }

            foreach (var k in keys)
            {
                k.PinFailedCount = 0;
                k.PinLockoutEnd = null;
            }

            key.LastUsedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            _ceremonyStore.ClearFailures(identity.UserId);

            // SecurityKey, because that is exactly what was proven — the key was read and Yubico
            // vouched for it. IssueTokenForUserAsync sees a satisfied second factor and lets the
            // session out without asking for an authenticator code on top.
            return await _userManagerService.IssueTokenForUserAsync(
                identity.UserId, proof: SecondFactorProof.SecurityKey);
        }
    }
}
