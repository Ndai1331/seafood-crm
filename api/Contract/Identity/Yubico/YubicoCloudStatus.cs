namespace Contract.Identity.Yubico
{
    /// <summary>
    /// What Yubico's validation service said, plus the ways its answer can fail to be trustworthy.
    ///
    /// The last three are ours, not Yubico's: they cover a reply that arrived but could not be
    /// believed. Folding those into "backend error" would hide a forged response behind a message
    /// about the service being busy.
    /// </summary>
    public enum YubicoCloudStatus
    {
        Ok,

        /// <summary>Not a well-formed OTP, or the key is unknown to Yubico.</summary>
        BadOtp,

        /// <summary>Yubico has already seen this OTP. Either a genuine double-send, or a replay.</summary>
        ReplayedOtp,

        /// <summary>Same otp+nonce pair sent twice — our fault, a nonce was reused.</summary>
        ReplayedRequest,

        /// <summary>Our request signature was wrong: the secret key is misconfigured.</summary>
        BadRequestSignature,

        NoSuchClient,
        OperationNotAllowed,
        MissingParameter,
        NotEnoughAnswers,
        BackendError,

        /// <summary>No client id or secret configured on this server.</summary>
        NotConfigured,

        /// <summary>Could not reach the service at all — offline, DNS, timeout.</summary>
        Unreachable,

        /// <summary>The reply's signature did not verify. Treat as hostile, not as a hiccup.</summary>
        BadResponseSignature,

        /// <summary>The reply echoed an otp or nonce we did not send.</summary>
        MismatchedEcho
    }

    public sealed record YubicoCloudResult(YubicoCloudStatus Status)
    {
        public bool IsValid => Status == YubicoCloudStatus.Ok;

        /// <summary>
        /// Split three ways on purpose. A person who touched the wrong key needs different words
        /// from one hitting an outage, and both need different words from an administrator whose
        /// configuration is broken.
        /// </summary>
        public string Message => Status switch
        {
            YubicoCloudStatus.Ok => string.Empty,

            YubicoCloudStatus.BadOtp =>
                "Chuỗi từ khoá không hợp lệ. Chạm lại khoá.",
            YubicoCloudStatus.ReplayedOtp or YubicoCloudStatus.ReplayedRequest =>
                "Chuỗi này đã được dùng rồi. Chạm khoá thêm một lần nữa.",

            YubicoCloudStatus.Unreachable or YubicoCloudStatus.NotEnoughAnswers
                or YubicoCloudStatus.BackendError =>
                "Không kết nối được dịch vụ xác thực khoá. Thử lại sau ít phút.",

            YubicoCloudStatus.NotConfigured or YubicoCloudStatus.NoSuchClient
                or YubicoCloudStatus.OperationNotAllowed or YubicoCloudStatus.BadRequestSignature
                or YubicoCloudStatus.MissingParameter =>
                "Cấu hình xác thực khoá chưa đúng. Báo quản trị viên.",

            YubicoCloudStatus.BadResponseSignature or YubicoCloudStatus.MismatchedEcho =>
                "Không xác minh được phản hồi từ dịch vụ xác thực. Báo quản trị viên ngay.",

            _ => "Không xác thực được khoá."
        };
    }
}
