using Application.Teams;
using Contract.Base;
using Contract.Teams;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/team")]
    public class TeamController : ControllerBase
    {
        private readonly ITeamService _service;

        public TeamController(ITeamService service)
        {
            _service = service;
        }

        /// <summary>
        /// Get list of teams with filtering and paging
        /// </summary>
        [HttpPost("list")]
        public async Task<ResponseHttpBaseList<TeamDto>> GetList([FromBody] TeamFilterDto filter)
        {
            return await _service.GetListAsync(filter);
        }

        /// <summary>
        /// Get team by id
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ResponseHttpBase<TeamDto>> GetById(int id)
        {
            return await _service.GetByIdAsync(id);
        }

        /// <summary>
        /// Create new team
        /// </summary>
        [HttpPost]
        public async Task<ResponseHttpBase<TeamDto>> Create([FromBody] CreateTeamDto dto)
        {
            return await _service.CreateAsync(dto);
        }

        /// <summary>
        /// Update team
        /// </summary>
        [HttpPut]
        public async Task<ResponseHttpBase<TeamDto>> Update([FromBody] UpdateTeamDto dto)
        {
            return await _service.UpdateAsync(dto);
        }

        /// <summary>
        /// Delete team
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ResponseHttpBaseBool> Delete(int id)
        {
            return await _service.DeleteAsync(id);
        }
    }
}

