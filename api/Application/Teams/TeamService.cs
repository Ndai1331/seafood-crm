using Contract.Base;
using Contract.Teams;
using Domain.Teams;
using Microsoft.EntityFrameworkCore;
using SqlServ4r.Repository.Teams;
using Volo.Abp.DependencyInjection;

namespace Application.Teams
{
    public class TeamService : ServiceBase, ITeamService, ITransientDependency
    {
        private readonly TeamRepository _repository;

        public TeamService(TeamRepository repository)
        {
            _repository = repository;
        }

        public async Task<ResponseHttpBaseList<TeamDto>> GetListAsync(TeamFilterDto filter)
        {
            var response = new ResponseHttpBaseList<TeamDto>();
            try
            {
                var query = _repository.GetQueryable().AsQueryable();

                // Apply filters
                if (!string.IsNullOrEmpty(filter.Code))
                {
                    query = query.Where(x => x.Code.Contains(filter.Code));
                }

                if (!string.IsNullOrEmpty(filter.Name))
                {
                    query = query.Where(x => x.Name != null && x.Name.Contains(filter.Name));
                }

                // Get total count
                response.Total = await query.CountAsync();

                // Apply paging
                if (filter.Take > 0)
                {
                    query = query.Skip(filter.Skip)
                                 .Take(filter.Take);
                }

                // Select to DTO
                response.Data = await query.Select(x => new TeamDto
                {
                    Id = x.Id,
                    Code = x.Code,
                    Name = x.Name
                }).ToListAsync();

                response.Status = true;
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.Message = ex.Message;
            }

            return response;
        }

        public async Task<ResponseHttpBase<TeamDto>> GetByIdAsync(int id)
        {
            var response = new ResponseHttpBase<TeamDto>();
            try
            {
                var entity = await _repository.FirstOrDefaultAsync(x => x.Id == id);
                if (entity == null)
                {
                    response.Status = false;
                    response.Message = "Team not found";
                    return response;
                }

                response.Data = new TeamDto
                {
                    Id = entity.Id,
                    Code = entity.Code,
                    Name = entity.Name
                };
                response.Status = true;
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.Message = ex.Message;
            }

            return response;
        }

        public async Task<ResponseHttpBase<TeamDto>> CreateAsync(CreateTeamDto dto)
        {
            var response = new ResponseHttpBase<TeamDto>();
            try
            {
                // Check if code already exists
                if (await _repository.IsCodeExistsAsync(dto.Code))
                {
                    response.Status = false;
                    response.Message = "Code already exists";
                    return response;
                }

                var entity = new Team
                {
                    Code = dto.Code,
                    Name = dto.Name
                };

                await _repository.AddAsync(entity);

                response.Data = new TeamDto
                {
                    Id = entity.Id,
                    Code = entity.Code,
                    Name = entity.Name
                };
                response.Status = true;
                response.Message = "Team created successfully";
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.Message = ex.Message;
            }

            return response;
        }

        public async Task<ResponseHttpBase<TeamDto>> UpdateAsync(UpdateTeamDto dto)
        {
            var response = new ResponseHttpBase<TeamDto>();
            try
            {
                var entity = await _repository.FirstOrDefaultAsync(x => x.Id == dto.Id);
                if (entity == null)
                {
                    response.Status = false;
                    response.Message = "Team not found";
                    return response;
                }

                // Check if code already exists (exclude current id)
                if (await _repository.IsCodeExistsAsync(dto.Code, dto.Id))
                {
                    response.Status = false;
                    response.Message = "Code already exists";
                    return response;
                }

                entity.Code = dto.Code;
                entity.Name = dto.Name;

                await _repository.UpdateAsync(entity);

                response.Data = new TeamDto
                {
                    Id = entity.Id,
                    Code = entity.Code,
                    Name = entity.Name
                };
                response.Status = true;
                response.Message = "Team updated successfully";
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.Message = ex.Message;
            }

            return response;
        }

        public async Task<ResponseHttpBaseBool> DeleteAsync(int id)
        {
            var response = new ResponseHttpBaseBool();
            try
            {
                var entity = await _repository.FirstOrDefaultAsync(x => x.Id == id);
                if (entity == null)
                {
                    response.Status = false;
                    response.Message = "Team not found";
                    return response;
                }

                _repository.Remove(entity);

                response.Data = true;
                response.Status = true;
                response.Message = "Team deleted successfully";
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.Message = ex.Message;
                response.Data = false;
            }

            return response;
        }
    }
}

