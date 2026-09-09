using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Application.Identity.Common;
using Application.Identity.WebAuthn;
using Contract.Common;
using Contract.Identity.Totp;
using Contract.Identity.WebAuthn;
using Contract.Identity.UserManager;
using Core.Exceptions;
using Domain.Identity.UserRecoveryCodes;
using Domain.Identity.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using SqlServ4r.EntityFramework;
using Volo.Abp.DependencyInjection;

namespace Application.Identity.Totp
{
    public class TotpService : ITotpService, ITransientDependency
    {
        private const int RecoveryCodeCount = 8;
        private const int RecoveryCodeLength = 10;
        private const string TemporaryTokenType = "totp_temp";

        private readonly UserManager<User> _userManager;
        private readonly IUserManagerService _userManagerService;
        private readonly IEncryptionService _encryptionService;
        private readonly DreamContext _context;
        private readonly IConfiguration _configuration;
        private readonly IMemoryCache _memoryCache;
        private readonly WebAuthnTokenValidator _ceremonyTokenValidator;
        private readonly ILogger<TotpService> _logger;

        public TotpService(
            UserManager<User> userManager,
            IUserManagerService userManagerService,
            IEncryptionService encryptionService,
            DreamContext context,
            IConfiguration configuration,
            IMemoryCache memoryCache,
            WebAuthnTokenValidator ceremonyTokenValidator,
            ILogger<TotpService> logger)
        {
            _userManager = userManager;
            _userManagerService = userManagerService;
            _encryptionService = encryptionService;
            _context = context;
            _configuration = configuration;
            _memoryCache = memoryCache;
            _ceremonyTokenValidator = ceremonyTokenValidator;
            _logger = logger;
        }

        /// <summary>
        /// The authenticator half of login-screen enrollment.
        ///
        /// Same two steps as the profile page, reached with a setup token instead of a session
        /// because the session is exactly what is being withheld: a key with no authenticator
        /// leaves nothing to fall back on when the key breaks, and the person cannot sign in to
        /// set one up afterwards. Confirming is what finally issues it.
        /// </summary>
        public async Task<TotpSetupResponseDto> SetupAtLoginAsync(TotpSetupWithTokenDto input)
        {
            var identity = _ceremonyTokenValidator.Validate(input.SetupToken, WebAuthnTokenTypes.Setup);
            return await SetupAsync(identity.UserId);
        }

        public async Task<TotpEnrollResultDto> ConfirmAtLoginAsync(TotpConfirmWithTokenDto input)
        {
            var identity = _ceremonyTokenValidator.Validate(input.SetupToken, WebAuthnTokenTypes.Setup);

            // ConfirmAsync refuses an account that already has the authenticator on, which is the
            // replay defence for this token too: the step it opens can only be paid once.
            var codes = await ConfirmAsync(identity.UserId, input.TotpCode);

            return new TotpEnrollResultDto
            {
                RecoveryCodes = codes.RecoveryCodes,
                // The key was touched to get here — that is the proof this session rests on.
                Token = await _userManagerService.IssueTokenForUserAsync(
                    identity.UserId, proof: SecondFactorProof.SecurityKey)
            };
        }

        public async Task<TotpSetupResponseDto> SetupAsync(int userId)
        {
            EnsureFeatureEnabled();
            var user = await GetUserAsync(userId);
            if (user.IsTotpEnabled)
            {
                throw new GlobalException("2FA is already enabled. Disable it before setting it up again.", HttpStatusCode.BadRequest);
            }

            var secret = TotpCodeService.GenerateBase32Secret();
            user.TotpSecretKey = _encryptionService.Encrypt(secret);
            user.IsTotpEnabled = false;
            user.TotpFailedCount = 0;
            user.TotpLockoutEnd = null;
            await _userManager.UpdateAsync(user);

            return new TotpSetupResponseDto
            {
                Base32Secret = secret,
                ProvisioningUri = BuildProvisioningUri(user, secret)
            };
        }

        public async Task<TotpRecoveryCodesResponseDto> ConfirmAsync(int userId, string totpCode)
        {
            EnsureFeatureEnabled();
            var user = await GetUserAsync(userId);
            if (user.IsTotpEnabled)
            {
                throw new GlobalException("2FA is already enabled.", HttpStatusCode.BadRequest);
            }

            var secret = GetPlaintextSecret(user);
            if (!TotpCodeService.VerifyCode(secret, totpCode, DateTime.UtcNow))
            {
                throw new GlobalException("Invalid authenticator code.", HttpStatusCode.BadRequest);
            }

            var oldCodes = await _context.UserRecoveryCodes.Where(x => x.UserId == user.Id).ToListAsync();
            _context.UserRecoveryCodes.RemoveRange(oldCodes);

            var plaintextCodes = GenerateRecoveryCodes();
            var rows = plaintextCodes.Select(code => CreateRecoveryCodeRow(user.Id, code)).ToList();
            await _context.UserRecoveryCodes.AddRangeAsync(rows);
            user.IsTotpEnabled = true;
            user.TotpFailedCount = 0;
            user.TotpLockoutEnd = null;
            await _userManager.UpdateAsync(user);
            await _context.SaveChangesAsync();

            return new TotpRecoveryCodesResponseDto { RecoveryCodes = plaintextCodes };
        }

        public async Task<TokenDto> VerifyLoginAsync(TotpVerifyLoginRequestDto request, string clientIp)
        {
            EnsureFeatureEnabled();
            var (userId, jti) = ValidateTemporaryToken(request.TemporaryToken);
            if (_memoryCache.TryGetValue($"totp_jti_used:{jti}", out _))
            {
                throw new GlobalException("Temporary token has already been used.", HttpStatusCode.Unauthorized);
            }

            await EnforceIpRateLimitAsync(clientIp);
            var user = await GetUserAsync(userId);
            EnforceUserLockout(user);

            var authenticatedByRecoveryCode = false;
            var remainingCodes = (int?)null;

            if (!string.IsNullOrWhiteSpace(request.RecoveryCode))
            {
                authenticatedByRecoveryCode = await TryConsumeRecoveryCodeAsync(user.Id, request.RecoveryCode);
                if (!authenticatedByRecoveryCode)
                {
                    await RecordFailureAsync(user, clientIp);
                    throw new GlobalException("Invalid recovery code.", HttpStatusCode.Unauthorized);
                }

                remainingCodes = await _context.UserRecoveryCodes.CountAsync(x => x.UserId == user.Id && !x.IsUsed);
                _logger.LogWarning("TOTP recovery code used for user {UserId}; {RemainingCodes} codes remain.", user.Id, remainingCodes);
            }
            else
            {
                var secret = GetPlaintextSecret(user);
                if (!TotpCodeService.VerifyCode(secret, request.TotpCode ?? string.Empty, DateTime.UtcNow))
                {
                    await RecordFailureAsync(user, clientIp);
                    throw new GlobalException("Invalid authenticator code.", HttpStatusCode.Unauthorized);
                }
            }

            await ResetFailureAsync(user, clientIp);
            _memoryCache.Set($"totp_jti_used:{jti}", true, TimeSpan.FromMinutes(GetTempTokenExpiryMinutes()));

            // Carries the same link offer as the password path, attached only after the second
            // factor has passed so the offer can never be used to sidestep it.
            //
            // Proof.Totp rather than "done": if security keys are mandatory and this user has none,
            // the choke point answers with an enrollment step instead of a session. Passing a
            // blanket "satisfied" here would let the authenticator stand in for the key.
            var token = await _userManagerService.IssueTokenForUserAsync(
                user.Id, proof: SecondFactorProof.Totp);
            token.RemainingRecoveryCodes = authenticatedByRecoveryCode ? remainingCodes : null;
            return token;
        }

        public async Task DisableAsync(int userId, string totpCode)
        {
            EnsureFeatureEnabled();
            var user = await GetUserAsync(userId);
            if (!user.IsTotpEnabled)
            {
                throw new GlobalException("2FA is not enabled for this user.", HttpStatusCode.BadRequest);
            }

            var secret = GetPlaintextSecret(user);
            if (!TotpCodeService.VerifyCode(secret, totpCode, DateTime.UtcNow))
            {
                throw new GlobalException("Invalid authenticator code.", HttpStatusCode.BadRequest);
            }

            var codes = await _context.UserRecoveryCodes.Where(x => x.UserId == user.Id).ToListAsync();
            _context.UserRecoveryCodes.RemoveRange(codes);
            user.TotpSecretKey = null;
            user.IsTotpEnabled = false;
            user.TotpFailedCount = 0;
            user.TotpLockoutEnd = null;
            await _userManager.UpdateAsync(user);
            await _context.SaveChangesAsync();
        }

        public async Task<TotpStatusDto> GetStatusAsync(int userId)
        {
            EnsureFeatureEnabled();
            var user = await GetUserAsync(userId);
            return new TotpStatusDto
            {
                FeatureEnabled = true,
                TotpEnabled = user.IsTotpEnabled,
                RemainingRecoveryCodes = user.IsTotpEnabled
                    ? await _context.UserRecoveryCodes.CountAsync(x => x.UserId == user.Id && !x.IsUsed)
                    : 0
            };
        }

        public async Task<TotpRecoveryCodesResponseDto> RegenerateRecoveryCodesAsync(int userId, string totpCode)
        {
            EnsureFeatureEnabled();
            var user = await GetUserAsync(userId);
            if (!user.IsTotpEnabled)
            {
                throw new GlobalException("Enable 2FA before regenerating recovery codes.", HttpStatusCode.BadRequest);
            }

            var secret = GetPlaintextSecret(user);
            if (!TotpCodeService.VerifyCode(secret, totpCode, DateTime.UtcNow))
            {
                throw new GlobalException("Invalid authenticator code.", HttpStatusCode.BadRequest);
            }

            var oldCodes = await _context.UserRecoveryCodes.Where(x => x.UserId == user.Id).ToListAsync();
            _context.UserRecoveryCodes.RemoveRange(oldCodes);
            var plaintextCodes = GenerateRecoveryCodes();
            await _context.UserRecoveryCodes.AddRangeAsync(plaintextCodes.Select(code => CreateRecoveryCodeRow(user.Id, code)));
            await _context.SaveChangesAsync();

            return new TotpRecoveryCodesResponseDto { RecoveryCodes = plaintextCodes };
        }

        public async Task<TotpStatusDto> AdminGetStatusAsync(int userId)
        {
            EnsureFeatureEnabled();
            var user = await GetUserAsync(userId);
            return new TotpStatusDto
            {
                FeatureEnabled = true,
                TotpEnabled = user.IsTotpEnabled,
                RemainingRecoveryCodes = user.IsTotpEnabled
                    ? await _context.UserRecoveryCodes.CountAsync(x => x.UserId == user.Id && !x.IsUsed)
                    : 0
            };
        }

        public async Task AdminDisableAsync(int userId)
        {
            EnsureFeatureEnabled();
            var user = await GetUserAsync(userId);
            var codes = await _context.UserRecoveryCodes.Where(x => x.UserId == user.Id).ToListAsync();
            _context.UserRecoveryCodes.RemoveRange(codes);
            user.TotpSecretKey = null;
            user.IsTotpEnabled = false;
            user.TotpFailedCount = 0;
            user.TotpLockoutEnd = null;
            await _userManager.UpdateAsync(user);
            await _context.SaveChangesAsync();
        }

        public async Task<TotpRecoveryCodesResponseDto> AdminRegenerateRecoveryCodesAsync(int userId)
        {
            EnsureFeatureEnabled();
            var user = await GetUserAsync(userId);
            if (!user.IsTotpEnabled)
            {
                throw new GlobalException("User must enable 2FA before recovery codes can be regenerated.", HttpStatusCode.BadRequest);
            }

            var oldCodes = await _context.UserRecoveryCodes.Where(x => x.UserId == user.Id).ToListAsync();
            _context.UserRecoveryCodes.RemoveRange(oldCodes);
            var plaintextCodes = GenerateRecoveryCodes();
            await _context.UserRecoveryCodes.AddRangeAsync(plaintextCodes.Select(code => CreateRecoveryCodeRow(user.Id, code)));
            await _context.SaveChangesAsync();

            return new TotpRecoveryCodesResponseDto { RecoveryCodes = plaintextCodes };
        }

        private void EnsureFeatureEnabled()
        {
            if (!_configuration.GetValue<bool>("Totp:Enabled"))
            {
                throw new GlobalException("TOTP feature is disabled.", HttpStatusCode.Forbidden);
            }
        }

        private async Task<User> GetUserAsync(int userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null || user.IsDelete || !user.IsActive)
            {
                throw new GlobalException("User not found.", HttpStatusCode.BadRequest);
            }

            return user;
        }

        private string GetPlaintextSecret(User user)
        {
            if (string.IsNullOrWhiteSpace(user.TotpSecretKey))
            {
                throw new GlobalException("Authenticator setup is required first.", HttpStatusCode.BadRequest);
            }

            return _encryptionService.Decrypt(user.TotpSecretKey);
        }

        private string BuildProvisioningUri(User user, string secret)
        {
            var issuer = _configuration["Totp:Issuer"] ?? "Task9";
            var account = !string.IsNullOrWhiteSpace(user.UserName) ? user.UserName : user.Id.ToString();
            return $"otpauth://totp/{Uri.EscapeDataString(issuer)}:{Uri.EscapeDataString(account)}?secret={secret}&issuer={Uri.EscapeDataString(issuer)}";
        }

        private (int UserId, string Jti) ValidateTemporaryToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                throw new GlobalException("Temporary token is required.", HttpStatusCode.Unauthorized);
            }

            var validationParameters = new TokenValidationParameters
            {
                ValidIssuer = _configuration["Jwt:Issuer"],
                // Must match what GenerateTemporaryToken issued: the two-factor audience, never
                // the session audience.
                ValidAudience = TwoFactorTokens.ResolveAudience(_configuration),
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? string.Empty)),
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            try
            {
                var principal = new JwtSecurityTokenHandler().ValidateToken(token, validationParameters, out _);

                // Two types, one direction. The key-step token is accepted here because both are
                // minted after a correct password, and someone stopped at the key step who lost
                // the key must be able to fall back to their authenticator without signing in
                // again. The reverse is NOT mirrored: the key endpoints keep refusing totp_temp,
                // and the passwordless PIN token opens neither — no password stands behind it.
                var tokenType = principal.FindFirst("token_type")?.Value;
                if (tokenType != TemporaryTokenType && tokenType != WebAuthnTokenTypes.SecurityKeyStep)
                {
                    throw new GlobalException("Invalid temporary token.", HttpStatusCode.Unauthorized);
                }

                var userIdValue = principal.FindFirst(ClaimTypes.PrimarySid)?.Value;
                var jti = principal.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
                if (!int.TryParse(userIdValue, out var userId) || string.IsNullOrWhiteSpace(jti))
                {
                    throw new GlobalException("Invalid temporary token.", HttpStatusCode.Unauthorized);
                }

                return (userId, jti);
            }
            catch (SecurityTokenException ex)
            {
                throw new GlobalException("Temporary token has expired or is invalid.", HttpStatusCode.Unauthorized);
            }
            catch (ArgumentException ex)
            {
                throw new GlobalException("Temporary token has expired or is invalid.", HttpStatusCode.Unauthorized);
            }
        }

        private async Task EnforceIpRateLimitAsync(string ip)
        {
            var now = DateTime.UtcNow;
            var record = await _context.TotpRateLimits.FirstOrDefaultAsync(x => x.IpAddress == ip && x.UserId == null);
            if (record?.LockedUntil > now)
            {
                throw new GlobalException("Too many authenticator attempts. Try again later.", HttpStatusCode.TooManyRequests);
            }
        }

        private void EnforceUserLockout(User user)
        {
            if (user.TotpLockoutEnd > DateTime.UtcNow)
            {
                throw new GlobalException("Authenticator login is temporarily locked.", HttpStatusCode.Locked);
            }
        }

        private async Task RecordFailureAsync(User user, string ip)
        {
            var now = DateTime.UtcNow;
            user.TotpFailedCount += 1;
            if (user.TotpFailedCount >= 10)
            {
                user.TotpLockoutEnd = now.AddMinutes(15);
                _logger.LogWarning("TOTP locked for user {UserId} after {FailureCount} failures.", user.Id, user.TotpFailedCount);
            }
            await _userManager.UpdateAsync(user);

            var ipRecord = await _context.TotpRateLimits.FirstOrDefaultAsync(x => x.IpAddress == ip && x.UserId == null);
            if (ipRecord == null)
            {
                ipRecord = new Domain.Identity.TotpRateLimits.TotpRateLimit
                {
                    IpAddress = ip,
                    FailureCount = 1,
                    WindowStart = now
                };
                await _context.TotpRateLimits.AddAsync(ipRecord);
            }
            else if (ipRecord.WindowStart.AddMinutes(10) < now)
            {
                ipRecord.FailureCount = 1;
                ipRecord.WindowStart = now;
                ipRecord.LockedUntil = null;
            }
            else
            {
                ipRecord.FailureCount += 1;
                if (ipRecord.FailureCount >= 5)
                {
                    ipRecord.LockedUntil = now.AddMinutes(10);
                }
            }

            await _context.SaveChangesAsync();
        }

        private async Task ResetFailureAsync(User user, string ip)
        {
            user.TotpFailedCount = 0;
            user.TotpLockoutEnd = null;
            await _userManager.UpdateAsync(user);

            var ipRecord = await _context.TotpRateLimits.FirstOrDefaultAsync(x => x.IpAddress == ip && x.UserId == null);
            if (ipRecord != null)
            {
                _context.TotpRateLimits.Remove(ipRecord);
                await _context.SaveChangesAsync();
            }
        }

        private async Task<bool> TryConsumeRecoveryCodeAsync(int userId, string recoveryCode)
        {
            var normalized = recoveryCode.Trim().ToUpperInvariant();
            var rows = await _context.UserRecoveryCodes.Where(x => x.UserId == userId && !x.IsUsed).ToListAsync();
            var match = rows.FirstOrDefault(row => VerifyRecoveryCode(normalized, row));
            if (match == null)
            {
                return false;
            }

            match.IsUsed = true;
            match.UsedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        private static UserRecoveryCode CreateRecoveryCodeRow(int userId, string code)
        {
            var salt = Convert.ToBase64String(RandomNumberGenerator.GetBytes(16));
            return new UserRecoveryCode
            {
                UserId = userId,
                CodeSalt = salt,
                CodeHash = HashRecoveryCode(code, salt),
                CreatedAt = DateTime.UtcNow
            };
        }

        private static bool VerifyRecoveryCode(string code, UserRecoveryCode row)
        {
            return HashRecoveryCode(code, row.CodeSalt) == row.CodeHash;
        }

        private static string HashRecoveryCode(string code, string salt)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes($"{salt}:{code}"));
            return Convert.ToHexString(bytes);
        }

        private static List<string> GenerateRecoveryCodes()
        {
            const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var codes = new HashSet<string>();
            while (codes.Count < RecoveryCodeCount)
            {
                var chars = new char[RecoveryCodeLength];
                var randomBytes = RandomNumberGenerator.GetBytes(RecoveryCodeLength);
                for (var i = 0; i < chars.Length; i++)
                {
                    chars[i] = alphabet[randomBytes[i] % alphabet.Length];
                }
                codes.Add(new string(chars));
            }

            return codes.ToList();
        }

        private int GetTempTokenExpiryMinutes()
        {
            var configured = _configuration.GetValue<int?>("Totp:TemporaryTokenExpiryMinutes") ?? 5;
            return configured > 0 ? configured : 5;
        }
    }
}
