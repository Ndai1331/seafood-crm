using Contract.Companies;
using Contract.Departments;
using Contract.Identity.RoleManager;
using Contract.Positions;
using Contract.Teams;

namespace Contract.Identity.UserManager
{
    public class UserWithNavigationPropertiesDto
    {
        public int Index { get; set;}
        public int Count { get; set; } = 0;
        public UserDto User { get; set; } = new UserDto();
        public List<string> RoleNames { get; set; } = new List<string>();
        public List<RoleDto> Roles { get; set; } = new List<RoleDto>();
        public CompanyDto? Company { get; set; }
        public PositionDto? Position { get; set; }
        public TeamDto? Team { get; set; }
        public List<DepartmentDto> Departments { get; set; } = new List<DepartmentDto>();
        

    }
}