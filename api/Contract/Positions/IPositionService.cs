using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Contract.Positions
{
    public interface IPositionService
    {
        Task<PositionDto> CreateAsync(CreateUpdatePositionDto input);
        Task<PositionDto> UpdateAsync(CreateUpdatePositionDto input,int id);
        Task DeleteAsync(int id);
        Task<List<PositionDto>> GetListAsync();

    }
}