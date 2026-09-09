using System;
using System.Collections.Generic;
using Core.Enum;

namespace Contract.Identity.UserManager
{
    public class CreateUpdateUseDto
    {
        public string UserName { get; set; }
        public string Password { get; set;}
        public string UserCode { get; set;}
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public Gender Gender { get; set; } = Gender.Unknown; 
        public DateTime? DOB { get; set; } 
        public string PhoneNumber { get; set; }
        public string? Email { get; set; }
        public bool IsActive { get; set; }
        public List<string> DepartmentCodes { get; set; } =  new List<string>();
        public string PositionCode { get; set; }
        public string? AvatarURL { get; set; }
        public List<string> RoleCodes { get; set; } = new List<string>();
        
        public int Row { get; set; }
        public int? CreatedBy { get; set; }
        public int? ModifiedBy { get; set; }
    }
}