using System.Security.Cryptography;
using Contract.Common;
using Microsoft.Extensions.Configuration;
using Volo.Abp.DependencyInjection;

namespace Application.Common
{
    public class AesEncryptionService : IEncryptionService, ITransientDependency
    {
        private readonly byte[] _key;

        public AesEncryptionService(IConfiguration configuration)
        {
            var enabled = configuration.GetValue<bool>("Totp:Enabled");
            var configuredKey = configuration["Totp:EncryptionKey"] ?? string.Empty;

            if (!enabled)
            {
                _key = new byte[32];
                return;
            }

            if (string.IsNullOrWhiteSpace(configuredKey))
            {
                throw new InvalidOperationException("Totp:EncryptionKey is required when Totp:Enabled is true.");
            }

            try
            {
                _key = Convert.FromBase64String(configuredKey);
            }
            catch (FormatException ex)
            {
                throw new InvalidOperationException("Totp:EncryptionKey must be a Base64 encoded 32-byte key.", ex);
            }

            if (_key.Length != 32)
            {
                throw new InvalidOperationException("Totp:EncryptionKey must decode to exactly 32 bytes.");
            }

            var jwtKey = configuration["Jwt:Key"] ?? string.Empty;
            if (configuredKey == jwtKey || Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(jwtKey)) == configuredKey)
            {
                throw new InvalidOperationException("Totp:EncryptionKey must be different from Jwt:Key.");
            }
        }

        public string Encrypt(string plaintext)
        {
            using var aes = Aes.Create();
            aes.Key = _key;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            aes.GenerateIV();

            using var encryptor = aes.CreateEncryptor();
            var plaintextBytes = System.Text.Encoding.UTF8.GetBytes(plaintext);
            var cipherBytes = encryptor.TransformFinalBlock(plaintextBytes, 0, plaintextBytes.Length);
            var result = new byte[aes.IV.Length + cipherBytes.Length];
            Buffer.BlockCopy(aes.IV, 0, result, 0, aes.IV.Length);
            Buffer.BlockCopy(cipherBytes, 0, result, aes.IV.Length, cipherBytes.Length);
            return Convert.ToBase64String(result);
        }

        public string Decrypt(string ciphertext)
        {
            var raw = Convert.FromBase64String(ciphertext);
            if (raw.Length <= 16)
            {
                throw new InvalidOperationException("Invalid encrypted TOTP secret.");
            }

            var iv = raw[..16];
            var cipherBytes = raw[16..];

            using var aes = Aes.Create();
            aes.Key = _key;
            aes.IV = iv;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using var decryptor = aes.CreateDecryptor();
            var plaintextBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
            return System.Text.Encoding.UTF8.GetString(plaintextBytes);
        }
    }
}
