using Contract.Base;
using Contract.Teams;

namespace Application.Teams
{
    public interface ITeamService
    {
        Task<ResponseHttpBaseList<TeamDto>> GetListAsync(TeamFilterDto filter);
        Task<ResponseHttpBase<TeamDto>> GetByIdAsync(int id);
        Task<ResponseHttpBase<TeamDto>> CreateAsync(CreateTeamDto dto);
        Task<ResponseHttpBase<TeamDto>> UpdateAsync(UpdateTeamDto dto);
        Task<ResponseHttpBaseBool> DeleteAsync(int id);
    }
}

