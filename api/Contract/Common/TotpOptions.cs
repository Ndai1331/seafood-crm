namespace Contract.Common
{
    public class TotpOptions
    {
        public bool Enabled { get; set; } = false;
        public string EncryptionKey { get; set; } = string.Empty;
        public string Issuer { get; set; } = "Task9";
        public int TemporaryTokenExpiryMinutes { get; set; } = 5;
    }
}
