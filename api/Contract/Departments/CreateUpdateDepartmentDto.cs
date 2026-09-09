using System;

namespace Contract.Departments
{
    public class CreateUpdateDepartmentDto
    {
        public string? Code { get; set; }
        public string Name { get; set; }
        public string? Note { get; set; }
        public int ODX { get; set; }
        public int? ParentCode { get; set; }

    }
}