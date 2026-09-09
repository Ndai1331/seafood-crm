namespace Contract.Identity.UserManager
{
    public class TokenModel
    {
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
    }

    public class TokenFribaseModel : TokenModel
    {
        public string? LoginProvider { get; set; }
        public string? FireBaseToken { get; set; }
        public string? DeviceId { get; set; }
    }
}