using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Departments;
using Contract.Departments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    
    [ApiController]
    [Route("api/department/")]
    [Authorize]
    public class DepartmentController : IDepartmentService
    {
        private DepartmentService _departmentService;
        public DepartmentController(DepartmentService positionService)
        {
            _departmentService = positionService;
        }
        
        [HttpPost]
        public async Task<DepartmentDto> CreateAsync(CreateUpdateDepartmentDto input)
        {
            return await _departmentService.CreateAsync(input);
        }

        [HttpPut]
        [Route("{id}")]
        public async Task<DepartmentDto> UpdateAsync(CreateUpdateDepartmentDto input, int id)
        {
            return  await _departmentService.UpdateAsync(input,id);
        }

        [HttpDelete]
        [Route("{id}")]
        public async Task DeleteAsync(int id)
        {
            await _departmentService.DeleteAsync(id);
        }

        [HttpGet]
        public async Task<List<DepartmentDto>> GetListAsync()
        {
            return await _departmentService.GetListAsync();
        }
    }
}