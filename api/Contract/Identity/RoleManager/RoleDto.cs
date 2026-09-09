using System;

namespace Contract.Identity.RoleManager
{
    public class RoleDto
    {
        public int Id { get; set; }
        public string Name { get; set;}
        public string? Code { get; set; }
        public string? DefaultPageUrl { get; set; }
    }
}