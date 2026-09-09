using System.Collections.Generic;
using Domain.Departments;
using Domain.Identity.Roles;
using Domain.Positions;
using Domain.Teams;

namespace Domain.Identity.Users
{
    public class UserWithNavigationProperties
    {

        public User User { get; set; }
        public int Count { get; set; } = 0;
        public List<string> RoleNames { get; set; }
        public List<Role> Roles { get; set; }
        public Position? Position { get; set; }
        public Team? Team { get; set; }
        public List<Department> Departments { get; set; }
    }
}