using System;
using System.Collections.Generic;
using Core.Enum;

namespace Contract.Identity.UserManager
{
    public class UserExcelDto
    {
        public string? UserName { get; set; }
        public string? Password { get; set;}
        public string? UserCode { get; set;}
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Gender { get; set; } 
        public string? DOB { get; set; } 
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public int Row { get; set; }
        public List<string> RoleCodes { get; set; } = new List<string>();
        public List<string> DepartmentCodes { get; set; }  = new List<string>();
        public string PositionCode { get; set; } 
        public string IsActive { get; set; }
    }

    // public class ConvertedUserExcel
    // {
    //     public string UserName { get; set; }
    //     public string Password { get; set;}
    //     public string UserCode { get; set;}
    //     public string FirstName { get; set; }
    //     public string LastName { get; set; }
    //     public Gender? Gender { get; set; } = Core.Enum.Gender.Unknown; 
    //     public DateTime? DOB { get; set; } 
    //     public string PhoneNumber { get; set; }
    //     public string? Email { get; set; }
    //     public int Row { get; set; }
    //     public List<string> Roles { get; set; }
    // }
}