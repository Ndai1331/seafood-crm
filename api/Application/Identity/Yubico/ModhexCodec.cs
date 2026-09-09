namespace Application.Identity.Yubico
{
    /// <summary>
    /// Modhex is Yubico's hex alphabet, picked so the characters land on the same physical keys
    /// across keyboard layouts. A YubiKey types its OTP as a keyboard, so a normal hex alphabet
    /// would come out different on an AZERTY or Dvorak machine.
    ///
    /// Mapping is positional: index in the alphabet IS the nibble value.
    ///   c=0 b=1 d=2 e=3 f=4 g=5 h=6 i=7 j=8 k=9 l=a n=b r=c t=d u=e v=f
    /// </summary>
    public static class ModhexCodec
    {
        public const string Alphabet = "cbdefghijklnrtuv";

        /// <summary>Full OTP: 12 characters of public id followed by 32 of ciphertext.</summary>
        public const int OtpLength = 44;
        public const int PublicIdLength = 12;

        public static bool IsModhex(string value)
        {
            if (string.IsNullOrEmpty(value)) return false;
            foreach (var c in value)
            {
                if (Alphabet.IndexOf(char.ToLowerInvariant(c)) < 0) return false;
            }
            return true;
        }

        /// <summary>
        /// Decodes modhex to bytes. Returns false rather than throwing: the input comes straight
        /// from a keyboard and being malformed is an ordinary outcome, not an exceptional one.
        /// </summary>
        public static bool TryDecode(string value, out byte[] bytes)
        {
            bytes = [];
            if (string.IsNullOrEmpty(value) || value.Length % 2 != 0) return false;

            var result = new byte[value.Length / 2];
            for (var i = 0; i < result.Length; i++)
            {
                var hi = Alphabet.IndexOf(char.ToLowerInvariant(value[i * 2]));
                var lo = Alphabet.IndexOf(char.ToLowerInvariant(value[(i * 2) + 1]));
                if (hi < 0 || lo < 0) return false;
                result[i] = (byte)((hi << 4) | lo);
            }

            bytes = result;
            return true;
        }

        public static string Encode(ReadOnlySpan<byte> bytes)
        {
            var chars = new char[bytes.Length * 2];
            for (var i = 0; i < bytes.Length; i++)
            {
                chars[i * 2] = Alphabet[bytes[i] >> 4];
                chars[(i * 2) + 1] = Alphabet[bytes[i] & 0x0F];
            }
            return new string(chars);
        }
    }
}
