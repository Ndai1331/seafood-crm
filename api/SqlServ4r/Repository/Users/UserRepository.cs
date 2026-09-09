using System.Linq.Dynamic.Core;
using Contract.Identity.UserManager;
using Core.Enum;
using Domain.Identity.Users;
using Domain.Positions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using SqlServ4r.EntityFramework;
using SqlServ4r.RepGenerationPatten;
using Volo.Abp.DependencyInjection;

namespace SqlServ4r.Repository.Users
{
    public class UserRepository
        : GenericRepository<User, Guid>,
            ITransientDependency,
            IUserRepository
    {
        public UserRepository([System.Diagnostics.CodeAnalysis.NotNull] DreamContext context)
            : base(context) { }

        public bool 
         CheckDuplicateInformation( string userCode)
        {
            return _context.Users.Any(x => x.UserCode == userCode);
        }

        public 
            bool 
         CheckDuplicateInformation(string userCode, int id)
        {
            return _context.Users.Where(x => x.Id != id).Any(x => x.UserCode == userCode);
        }

        public async Task<List<UserWithNavigationProperties>> GetListWithNavigationProperties(
        )
        {
            var users = await _context.Users
                .Where(x => !x.IsDelete)
                .Include(u => u.Position)
                .Include(u => u.Team)
                .ToListAsync();

            var userIds = users.Select(x => x.Id).ToList();
            var userRoles = await (
                from roleUser in _context.UserRoles
                join role in _context.Roles on roleUser.RoleId equals role.Id
                where userIds.Contains(roleUser.UserId)
                select new
                {
                    roleUser.UserId,
                    Role = role
                }
            ).ToListAsync();

            var userDepartments = await (
                from department in _context.Departments
                join departmentUser in _context.UserDepartments
                    on department.Id equals departmentUser.DepartmentId
                where userIds.Contains(departmentUser.UserId)
                select new
                {
                    departmentUser.UserId,
                    Department = department
                }
            ).ToListAsync();

            var rolesByUserId = userRoles
                .GroupBy(x => x.UserId)
                .ToDictionary(x => x.Key, x => x.Select(item => item.Role).ToList());

            var departmentsByUserId = userDepartments
                .GroupBy(x => x.UserId)
                .ToDictionary(x => x.Key, x => x.Select(item => item.Department).ToList());

            return users.Select(user =>
            {
                rolesByUserId.TryGetValue(user.Id, out var roles);
                departmentsByUserId.TryGetValue(user.Id, out var departments);
                roles ??= new();
                departments ??= new();

                return new UserWithNavigationProperties
                {
                    User = user,
                    Position = user.Position,
                    RoleNames = roles.Select(r => r.Name).ToList(),
                    Roles = roles,
                    Departments = departments,
                    Team = user.Team
                };
            }).ToList();
        }

        public async Task<List<User>> GetListByRoles(UserFilterPagingModel? filter = null)
        {
            bool isAdminUser = (
                from roleUser in _context.UserRoles
                join role in _context.Roles on roleUser.RoleId equals role.Id
                where
                    (
                        filter != null
                            ? !string.IsNullOrEmpty(filter.RoleName)
                                ? roleUser.UserId == filter.CreatedBy
                                    && role.Name == RoleClaimEnum.ADMIN.ToString()
                                : true
                            : true
                    )
                select role.Name
            ).Any();
            var query =
                from user in _context.Users.Where(x => !x.IsDelete)
                where
                    (
                        from roleUser in _context.UserRoles
                        join role in _context.Roles on roleUser.RoleId equals role.Id
                        where
                            (
                                filter != null
                                    ? !string.IsNullOrEmpty(filter.RoleName)
                                        ? roleUser.UserId == user.Id && role.Name == filter.RoleName
                                        : true
                                    : true
                            )
                        select role.Name
                    ).Any()
                    && (
                        filter != null && filter.CreatedBy != null
                            ? (user.CreatedBy == filter.CreatedBy || isAdminUser == true)
                            : true
                    )
                    && (
                        filter != null && !string.IsNullOrEmpty(filter.FullName)
                            ? ((user.FirstName + " " + user.LastName).Contains(filter.FullName))
                            : true
                    )
                    && (
                        filter != null && !string.IsNullOrEmpty(filter.UserCode)
                            ? ((user.UserCode).Contains(filter.UserCode))
                            : true
                    )
                    && (
                        filter != null && filter.Gender != null
                            ? ((user.Gender).Equals(filter.Gender))
                            : true
                    )
                select user;

            List<User> result = new List<User>();
            if (filter != null && filter.Take > 0)
            {
                int count = query.Count();
                result = await query.Skip(filter.Skip).Take(filter.Take).ToListAsync();
            }
            else
            {
                result = await query.ToListAsync();
            }

            return result;
        }

        public async Task<List<UserBasicInfoDto>> GetUserBasicInfoWithNavigationProperties(
            string? text
        )
        {
            var rolePatient = await _context
                .Roles.AsNoTracking()
                .FirstOrDefaultAsync(s => s.Code == RoleClaimEnum.PATIENT.ToString());

            var query =
                from user in _context
                    .Users.Where(x => !x.IsDelete && x.IsActive)
                    .WhereIf(
                        !text.IsNullOrWhiteSpace(),
                        x => x.FirstName.Contains(text) || x.LastName.Contains(text)
                    )
                where
                    _context.UserRoles.Count(x => x.UserId == user.Id && x.RoleId == rolePatient.Id)
                    == 0
                select new UserBasicInfoDto()
                {
                    Id = user.Id,
                    FullName = user.FirstName + " " + user.LastName,
                    Gender = user.Gender,
                    DOB = user.DOB,
                    UserCode = user.UserCode,
                    PhoneNumber = user.PhoneNumber,
                    AvatarURL = user.AvatarURL,
                    Position = user.Position.Name,
                    Team = user.Team != null ? user.Team.Name : "",
                    Departments = (
                        from department in _context.Departments
                        join departmentUser in _context.UserDepartments
                            on department.Id equals departmentUser.DepartmentId
                        where departmentUser.UserId == user.Id
                        select department
                    )
                        .Select(x => x.Name)
                        .ToList()
                };

            return await query.ToListAsync();
        }

        public async Task<UserWithNavigationProperties> GetWithNavigationProperties(int id)
        {
            var query =
                from user in _context.Users.Where(x => x.Id == id)
                select new UserWithNavigationProperties
                {
                    User = user,
                    RoleNames = (
                        from roleUser in _context.UserRoles
                        join role in _context.Roles on roleUser.RoleId equals role.Id
                        where roleUser.UserId == user.Id
                        select role.Name
                    ).ToList(),
                    Position = (
                        from position in _context.Positions
                        where position.Id == user.PositionId
                        select position
                    ).FirstOrDefault(),
                    Team = (
                        from team in _context.Teams
                        where team.Id == user.TeamId
                        select team
                    ).FirstOrDefault(),
                    Departments = (
                        from department in _context.Departments
                        join departmentUser in _context.UserDepartments
                            on department.Id equals departmentUser.DepartmentId
                        where departmentUser.UserId == user.Id
                        select department
                    ).ToList()
                };

            return await query.FirstOrDefaultAsync() ?? new UserWithNavigationProperties();
        }
    }
}
