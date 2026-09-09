using System;

namespace Contract.Identity.UserManager
{
    public class UserIdentityDto
    {
        public int Id { get; set; }
        public int? PositionId { get; set; }
        public string? PositionName { get; set; }
        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? UserCode { get; set;}
        public string? AvatarURL { get; set; }
        public int? TeamId { get; set; }
    }
}