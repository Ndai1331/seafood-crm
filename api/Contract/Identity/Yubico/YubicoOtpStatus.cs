namespace Contract.Identity.Yubico
{
    /// <summary>
    /// Why an OTP was accepted or turned away.
    ///
    /// Every case gets its own value because every case needs its own sentence on screen. Folding
    /// them into one "invalid" is what turns a five-second fix ("touch the key again") into a
    /// support ticket.
    /// </summary>
    public enum YubicoOtpStatus
    {
        Ok,

        /// <summary>Not 44 modhex characters — usually a half-caught keystroke run.</summary>
        Malformed,

        /// <summary>No registered key carries this public id.</summary>
        UnknownKey,

        /// <summary>The key is registered, but to a different account.</summary>
        WrongOwner
    }

    public sealed record YubicoOtpVerification(YubicoOtpStatus Status, int? KeyId = null)
    {
        public bool IsValid => Status == YubicoOtpStatus.Ok;

        /// <summary>Wording shown to the person at the keyboard, not to a log reader.</summary>
        public string Message => Status switch
        {
            YubicoOtpStatus.Ok => string.Empty,
            YubicoOtpStatus.Malformed =>
                "Chưa nhận đủ tín hiệu từ khoá. Chạm và giữ khoá lâu hơn một chút.",
            YubicoOtpStatus.UnknownKey =>
                "Khoá này chưa được đăng ký trong hệ thống.",
            YubicoOtpStatus.WrongOwner =>
                "Khoá này chưa được đăng ký cho tài khoản của bạn.",
            _ => "Không xác thực được khoá."
        };
    }
}
