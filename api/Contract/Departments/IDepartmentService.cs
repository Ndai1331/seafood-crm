using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Contract.Departments
{
    public interface IDepartmentService
    {
        Task<DepartmentDto> CreateAsync(CreateUpdateDepartmentDto input);
        Task<DepartmentDto> UpdateAsync(CreateUpdateDepartmentDto input,int id);
        Task DeleteAsync(int id);
        Task<List<DepartmentDto>> GetListAsync();
        

    }
}