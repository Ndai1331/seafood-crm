using System;
using System.Collections.Generic;
using Core.Enum;

namespace Contract.Identity.UserManager
{
    public class UserBasicInfoDto
    {
        public int Id { get; set; }
        public string UserCode { get; set;}
        public string FullName { get; set; }
        public Gender Gender { get; set; } = Gender.Unknown;
        public DateTime DOB { get; set; } = DateTime.Now;
        public DateTime CreatedDate { get; set; }
        public string PhoneNumber { get; set; }
        public List<string> Departments { get; set; }
        public string Position { get; set; }
        public string Team { get; set; }
        public int AverageLevelSatisfaction { get; set; }
        public string AvatarURL { get; set; }
    }
}