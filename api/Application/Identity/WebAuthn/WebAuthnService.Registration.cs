using System.Net;
using System.Text.Json;
using Contract.Identity.WebAuthn;
using Core.Exceptions;
using Domain.Identity.UserSecurityKeys;
using Domain.Identity.Users;
using Fido2NetLib;
using Fido2NetLib.Objects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Identity.WebAuthn
{
    /// <summary>Registration half of the ceremonies — shared by the signed-in and enrollment paths.</summary>
    public partial class WebAuthnService
    {
        private const int MaxKeysPerUser = 10;

        private async Task<WebAuthnCeremonyOptionsDto> BuildRegistrationOptionsAsync(User user, string ceremonyId)
        {
            var existing = await _context.UserSecurityKeys
                .AsNoTracking()
                .Where(x => x.UserId == user.Id)
                .Select(x => x.CredentialId)
                .ToListAsync();

            if (existing.Count >= MaxKeysPerUser)
            {
                throw new GlobalException(
                    $"Mỗi tài khoản chỉ đăng ký tối đa {MaxKeysPerUser} khoá.", HttpStatusCode.BadRequest);
            }

            var options = _fido2.RequestNewCredential(new RequestNewCredentialParams
            {
                User = new Fido2User
                {
                    Id = System.Text.Encoding.UTF8.GetBytes(user.Id.ToString()),
                    Name = user.UserName ?? user.Id.ToString(),
                    DisplayName = $"{user.FirstName} {user.LastName}".Trim()
                },
                // Stops the browser from silently registering a key this account already holds.
                ExcludeCredentials = existing.Select(id => new PublicKeyCredentialDescriptor(id)).ToList(),
                AuthenticatorSelection = new AuthenticatorSelection
                {
                    // AuthenticatorAttachment deliberately unset: leaving it open is what lets a
                    // phone passkey, Touch ID or Windows Hello count. Pinning it to
                    // "cross-platform" would strand everyone without a spare USB port, and the
                    // only remedy then is switching enforcement off for the whole company.
                    ResidentKey = ResidentKeyRequirement.Discouraged,
                    UserVerification = UserVerificationRequirement.Preferred
                },
                AttestationPreference = AttestationConveyancePreference.None
            });

            return StoreOptions(ceremonyId, options.ToJson());
        }

        private async Task<SecurityKeyDto> StoreNewCredentialAsync(
            User user, string ceremonyId, WebAuthnRegisterCompleteDto input)
        {
            var deviceName = await ResolveDeviceNameAsync(user.Id, input.DeviceName);

            var optionsJson = _ceremonyStore.TakeChallenge(ceremonyId)
                ?? throw new GlobalException("Phiên đăng ký đã hết hạn. Hãy thử lại.", HttpStatusCode.BadRequest);

            var attestation = Deserialize<AuthenticatorAttestationRawResponse>(input.AttestationJson);

            RegisteredPublicKeyCredential credential;
            try
            {
                credential = await _fido2.MakeNewCredentialAsync(new MakeNewCredentialParams
                {
                    AttestationResponse = attestation,
                    OriginalOptions = CredentialCreateOptions.FromJson(optionsJson),
                    IsCredentialIdUniqueToUserCallback = (args, _) => IsCredentialFreeAsync(args.CredentialId)
                });
            }
            catch (Fido2VerificationException ex)
            {
                _logger.LogWarning(ex, "WebAuthn registration failed for user {UserId}.", user.Id);
                throw new GlobalException("Không xác thực được khoá bảo mật. Hãy thử lại.", HttpStatusCode.BadRequest);
            }

            // No "already registered to me" check here: IsCredentialIdUniqueToUserCallback above
            // already rejects a credential present anywhere in the table, so this point is only
            // reached for a genuinely new one.
            var key = new UserSecurityKey
            {
                UserId = user.Id,
                CredentialId = credential.Id,
                PublicKey = credential.PublicKey,
                SignatureCounter = credential.SignCount,
                DeviceName = deviceName,
                CreatedAt = DateTime.UtcNow
            };

            await _context.UserSecurityKeys.AddAsync(key);
            await _context.SaveChangesAsync();

            // The account owner has to hear about this. If a leaked password was used to attach an
            // attacker's key, this message is the only thing that surfaces it — changing the
            // password afterwards does not detach the key.
            await _notifier.KeyRegisteredAsync(user, deviceName);

            return ToDto(key);
        }

        private async Task<bool> IsCredentialFreeAsync(byte[] credentialId) =>
            !await _context.UserSecurityKeys.AnyAsync(x => x.CredentialId == credentialId);

        /// <summary>
        /// Names the key so the user does not have to. Almost everyone registers exactly one, and
        /// asking them to invent a label for it is friction at the worst possible moment — during
        /// forced enrollment, before they can get in at all.
        ///
        /// A caller may still pass a name; a blank one gets "YubiKey", and the numbering only
        /// appears once a second key exists, which is the only point at which telling them apart
        /// starts to matter.
        /// </summary>
        private async Task<string> ResolveDeviceNameAsync(int userId, string? requested)
        {
            var name = (requested ?? string.Empty).Trim();
            if (name.Length > 0)
            {
                return name.Length > 100 ? name[..100] : name;
            }

            var taken = await _context.UserSecurityKeys
                .AsNoTracking()
                .Where(x => x.UserId == userId)
                .Select(x => x.DeviceName)
                .ToListAsync();

            const string baseName = "YubiKey";
            if (!taken.Contains(baseName, StringComparer.OrdinalIgnoreCase))
            {
                return baseName;
            }

            for (var i = 2; i <= MaxKeysPerUser + 1; i++)
            {
                var candidate = $"{baseName} {i}";
                if (!taken.Contains(candidate, StringComparer.OrdinalIgnoreCase))
                {
                    return candidate;
                }
            }

            return $"{baseName} {DateTime.Now:dd/MM HH:mm}";
        }

        /// <summary>
        /// Stores the library's own JSON so the complete half can hand back exactly what the
        /// begin half produced — the challenge must survive the round trip byte for byte.
        /// </summary>
        private WebAuthnCeremonyOptionsDto StoreOptions(string ceremonyId, string optionsJson)
        {
            _ceremonyStore.SaveChallenge(ceremonyId, optionsJson, TimeSpan.FromMinutes(5));
            return new WebAuthnCeremonyOptionsDto { OptionsJson = optionsJson };
        }

        internal static SecurityKeyDto ToDto(UserSecurityKey key) => new()
        {
            Id = key.Id,
            DeviceName = key.DeviceName,
            CreatedAt = key.CreatedAt,
            LastUsedAt = key.LastUsedAt
        };
    }
}
