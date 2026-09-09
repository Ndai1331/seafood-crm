using System.Security.Cryptography;
using System.Text;

namespace Application.Identity.Totp
{
    internal static class TotpCodeService
    {
        private const string Base32Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";

        public static string GenerateBase32Secret(int byteLength = 20)
        {
            var bytes = RandomNumberGenerator.GetBytes(byteLength);
            return ToBase32(bytes);
        }

        public static bool VerifyCode(string base32Secret, string code, DateTime utcNow)
        {
            if (string.IsNullOrWhiteSpace(code) || code.Length != 6 || !code.All(char.IsDigit))
            {
                return false;
            }

            var secretBytes = FromBase32(base32Secret);
            var currentStep = ToTimeStep(utcNow);
            for (var offset = -1; offset <= 1; offset++)
            {
                if (GenerateCode(secretBytes, currentStep + offset) == code)
                {
                    return true;
                }
            }

            return false;
        }

        private static string GenerateCode(byte[] secretBytes, long timeStep)
        {
            var counter = BitConverter.GetBytes(timeStep);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(counter);
            }

            using var hmac = new HMACSHA1(secretBytes);
            var hash = hmac.ComputeHash(counter);
            var offset = hash[^1] & 0x0f;
            var binary =
                ((hash[offset] & 0x7f) << 24) |
                ((hash[offset + 1] & 0xff) << 16) |
                ((hash[offset + 2] & 0xff) << 8) |
                (hash[offset + 3] & 0xff);
            var otp = binary % 1_000_000;
            return otp.ToString("D6");
        }

        private static long ToTimeStep(DateTime utcNow)
        {
            var unixSeconds = new DateTimeOffset(utcNow).ToUnixTimeSeconds();
            return unixSeconds / 30;
        }

        private static string ToBase32(byte[] data)
        {
            var result = new StringBuilder();
            var bitBuffer = 0;
            var bitCount = 0;

            foreach (var b in data)
            {
                bitBuffer = (bitBuffer << 8) | b;
                bitCount += 8;
                while (bitCount >= 5)
                {
                    result.Append(Base32Alphabet[(bitBuffer >> (bitCount - 5)) & 31]);
                    bitCount -= 5;
                }
            }

            if (bitCount > 0)
            {
                result.Append(Base32Alphabet[(bitBuffer << (5 - bitCount)) & 31]);
            }

            return result.ToString();
        }

        private static byte[] FromBase32(string input)
        {
            var clean = input.TrimEnd('=').Replace(" ", string.Empty).ToUpperInvariant();
            var bytes = new List<byte>();
            var bitBuffer = 0;
            var bitCount = 0;

            foreach (var c in clean)
            {
                var value = Base32Alphabet.IndexOf(c);
                if (value < 0)
                {
                    throw new FormatException("Invalid Base32 secret.");
                }

                bitBuffer = (bitBuffer << 5) | value;
                bitCount += 5;
                if (bitCount >= 8)
                {
                    bytes.Add((byte)((bitBuffer >> (bitCount - 8)) & 255));
                    bitCount -= 8;
                }
            }

            return bytes.ToArray();
        }
    }
}
