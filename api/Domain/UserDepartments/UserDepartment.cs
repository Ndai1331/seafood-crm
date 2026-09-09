using System;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Departments;
using Domain.Identity.Users;

namespace Domain.UserDepartments
{
    [Table("userdepartments")]
    public class UserDepartment
    {
        public int Id { get; set; } 
        public int UserId { get; set; }
        public int DepartmentId { get; set;}
        
        public User User { get; set; }
        public Department Department { get; set; }
    }
}