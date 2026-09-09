using Contract.Identity.Yubico;
using Domain.Identity.UserYubikeys;

namespace Application.Identity.Yubico
{
    /// <summary>
    /// Decides whether a typed OTP belongs to the account trying to sign in.
    ///
    /// This is the half Yubico does not do. Their service answers "a genuine key produced this OTP
    /// and it has not been used" — it has no idea which of our accounts that key was handed to.
    /// Taking status=OK as permission to sign someone in would let any working key open any
    /// account, which is the classic way an OTP integration ends up wide open.
    ///
    /// Pure function, so every refusal reason is testable without a network or a database.
    /// </summary>
    public static class YubicoOtpVerifier
    {
        /// <summary>Checks shape and ownership. The service call happens separately, and only if this passes.</summary>
        /// <param name="publicIdExistsElsewhere">
        /// Whether some other account owns this key. Only used to choose wording — "nobody
        /// registered this key" and "this is not your key" send a person to different places.
        /// </param>
        public static YubicoOtpVerification ResolveOwner(
            string otp,
            IReadOnlyCollection<UserYubikey> candidates,
            bool publicIdExistsElsewhere)
        {
            if (string.IsNullOrWhiteSpace(otp)) return new(YubicoOtpStatus.Malformed);

            otp = otp.Trim().ToLowerInvariant();
            if (otp.Length != ModhexCodec.OtpLength || !ModhexCodec.IsModhex(otp))
            {
                return new(YubicoOtpStatus.Malformed);
            }

            var publicId = ExtractPublicId(otp);
            var key = candidates.FirstOrDefault(k =>
                string.Equals(k.PublicId, publicId, StringComparison.OrdinalIgnoreCase));

            if (key is null)
            {
                return new(publicIdExistsElsewhere
                    ? YubicoOtpStatus.WrongOwner
                    : YubicoOtpStatus.UnknownKey);
            }

            return new(YubicoOtpStatus.Ok, key.Id);
        }

        /// <summary>The 12 characters that identify the physical key, constant for its lifetime.</summary>
        public static string? ExtractPublicId(string? otp)
        {
            if (string.IsNullOrWhiteSpace(otp)) return null;

            var trimmed = otp.Trim().ToLowerInvariant();
            return trimmed.Length < ModhexCodec.PublicIdLength
                ? null
                : trimmed[..ModhexCodec.PublicIdLength];
        }
    }
}
