using System;

namespace Domain.Identity.Users
{
    public class UserIdentity
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string UserCode { get; set;}
        public string AvatarURL { get; set; }
    }
}