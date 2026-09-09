using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.UserDepartments;

namespace Domain.Departments
{
    [Table("departments")]
    public class Department
    {
        public int Id { get; set; }
        public string? Code { get; set; }
        public string Name { get; set; }
        public string? Note { get; set; }
        public int ODX { get; set; }
        public int? ParentCode { get; set; }
        
        
        //naviagation

        public List<UserDepartment> UserDepartments { get; set; }
    }
}