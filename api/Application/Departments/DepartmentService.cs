using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Contract.Departments;
using Core.Const;
using Core.Enum;
using Core.Exceptions;
using Domain.Departments;
using Microsoft.EntityFrameworkCore;
using SqlServ4r.Repository.Departments;
using SqlServ4r.Repository.UserDepartments;
using Volo.Abp.DependencyInjection;

namespace Application.Departments
{
    public class DepartmentService : ServiceBase,IDepartmentService,ITransientDependency
    {
       private readonly DepartmentRepository _departmentRepository;
       private readonly UserDepartmentRepository _userDepartmentRepository;


        public DepartmentService(DepartmentRepository departmentRepository,
            UserDepartmentRepository userDepartmentRepository)
        {
            _departmentRepository = departmentRepository;
            _userDepartmentRepository = userDepartmentRepository;
        }


        public async Task<DepartmentDto> CreateAsync(CreateUpdateDepartmentDto input)
        {
            (input.Name, input.Code) = TrimText(input.Name, input.Code);

            var department = ObjectMapper.Map<CreateUpdateDepartmentDto, Department>(input);
            await _departmentRepository.AddAsync(department);
            await CheckParentRootExistsAndDeleteIt(input.ParentCode);
       
            
            return ObjectMapper.Map<Department,DepartmentDto>(department);
        }

        public async Task CheckParentRootExistsAndDeleteIt(int? parentId)
        {
            var items = await _userDepartmentRepository.
                GetListAsync(x => x.DepartmentId == parentId);
            if (items.Count > 0)
            {
                _userDepartmentRepository.RemoveRange(items);
            }
        }

        public async Task<DepartmentDto> UpdateAsync(CreateUpdateDepartmentDto input, int id)
        {
            (input.Name, input.Code) = TrimText(input.Name, input.Code);

            var item = await _departmentRepository.FirstOrDefaultAsync(x => x.Id == id);

            if (item is null)
            {
                throw new GlobalException(HttpMessage.NotFound, HttpStatusCode.BadRequest);
            }
            
            var department = ObjectMapper.Map(input,item);
            await _departmentRepository.UpdateAsync(department);
            await CheckParentRootExistsAndDeleteIt(input.ParentCode);

            return ObjectMapper.Map<Department,DepartmentDto>(department);
        }

 

        public async Task DeleteAsync(int id)
        {
            var department = await _departmentRepository.FirstOrDefaultAsync(x => x.Id == id);

            if (department is null)
            {
                throw new GlobalException(HttpMessage.NotFound, HttpStatusCode.BadRequest);
            }
            
            
            var departments = 
                await _departmentRepository.GetListAsync(x => x.ParentCode == id);

            foreach (var item in departments)
            {
                item.ParentCode = null;
            }
            
            _departmentRepository.UpdateRange(departments);

           _departmentRepository.Remove(department);
           
            }

        public async Task<List<DepartmentDto>> GetListAsync()
        {
            var departments = await _departmentRepository.GetQueryable().
                OrderBy(x=>x.ODX).ToListAsync();
            return ObjectMapper.Map<List<Department>, List<DepartmentDto>>(departments);
        }
        
        private (string Name, string? Code) TrimText(string name, string code)
        {
            return (name.Trim(), code?.Trim().ToUpper());
        }
    }
}