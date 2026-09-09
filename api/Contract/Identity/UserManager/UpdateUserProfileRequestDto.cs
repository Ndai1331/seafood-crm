using System;

namespace Contract.Identity.UserManager
{
    public class UpdateUserProfileRequestDto
    {
        public int Id { get; set; }
        public DateTime DOB { get; set; }
        public string AvatarURL { get; set; }
        public string? SignImgageUrl { get; set; }

        public string? SignKey { get; set; }
        public string? SignSecrect { get; set; }
    }
}
