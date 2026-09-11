using AutoMapper;
using Contract.AppConfigs;
using Contract.AppHistories;
using Contract.Departments;
using Contract.Identity.RoleManager;
using Contract.Identity.UserManager;
using Contract.Positions;
using Contract.Teams;
using Domain.AppConfigs;
using Domain.AppHistories;
using Domain.Departments;
using Domain.Identity.Roles;
using Domain.Identity.Users;
using Domain.Positions;
using Domain.Teams;

namespace Application
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<AppConfig, AppConfigDto>().ReverseMap();

            CreateMap<AppHistoryDto, AppHistory>();
            CreateMap<AppHistory, AppHistoryDto>()
                .ForMember(dest => dest.FullName,
                    opt => opt.MapFrom(src => src.User == null
                        ? $"User #{src.UserId}"
                        : (src.User.FirstName + " " + src.User.LastName).Trim()));
            CreateMap<CreateUpdateAppHistoryDto, AppHistory>().ReverseMap();

            CreateMap<UserWithNavigationProperties, UserWithNavigationPropertiesDto>().ReverseMap();
            CreateMap<CreateUserDto, User>().ReverseMap();
            CreateMap<UpdateUserDto, User>().ReverseMap();
            CreateMap<UpdateUserProfileRequestDto, User>().ReverseMap();
            CreateMap<CreateUpdateUseDto, CreateUserDto>();
            CreateMap<CreateUpdateUseDto, UpdateUserDto>();
            CreateMap<User, UserDto>()
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.LastName} {src.FirstName}"))
                .ForMember(dest => dest.TeamCode, opt => opt.MapFrom(src => src.Team != null ? src.Team.Code : null))
                .ForMember(dest => dest.TeamName, opt => opt.MapFrom(src => src.Team != null ? src.Team.Name : null));
            CreateMap<User, UserIdentityDto>()
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.LastName} {src.FirstName}"))
                .ForMember(dest => dest.PositionName, opt => opt.MapFrom(src => src.Position != null ? src.Position.Name : ""));
            CreateMap<CreateUpdateUseDto, User>().ReverseMap();
            CreateMap<Role, RoleDto>().ReverseMap();
            CreateMap<RoleDto, CreateUpdateRoleDto>().ReverseMap();
            CreateMap<Role, CreateUpdateRoleDto>().ReverseMap();
            CreateMap<Position, PositionDto>().ReverseMap();
            CreateMap<CreateUpdatePositionDto, Position>().ReverseMap();
            CreateMap<Department, DepartmentDto>().ReverseMap();
            CreateMap<CreateUpdateDepartmentDto, Department>().ReverseMap();
            CreateMap<UserIdentity, UserIdentityDto>().ReverseMap();
            CreateMap<Team, TeamDto>().ReverseMap();
            CreateMap<CreateTeamDto, Team>().ReverseMap();
            CreateMap<UpdateTeamDto, Team>().ReverseMap();
        }
    }
}
