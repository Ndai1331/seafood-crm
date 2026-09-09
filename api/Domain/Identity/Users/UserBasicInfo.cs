using System;
using Core.Enum;

namespace Domain.Identity.Users
{
    public class UserBasicInfo
    {
        public int Id { get; set; }
        public string UserCode { get; set;}
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public Gender Gender { get; set; } = Gender.Unknown;
        public DateTime DOB { get; set; } = DateTime.Now;
        public string PhoneNumber { get; set; }
        public string Department { get; set; }
        public string Position { get; set; }
        public string AvatarURL { get; set; }

    }
}