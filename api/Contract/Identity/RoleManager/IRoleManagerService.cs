using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Contract.Identity.RoleManager
{
    public interface IRoleManagerService
    {
        //Role
        Task<List<RoleDto>> GetListAsync();
        Task<List<RoleDefaultPageDto>> GetDefaultPagesAsync();
        Task SaveDefaultPagesAsync(List<RoleDefaultPageDto> input);
        Task<RoleDto> CreateAsync(CreateUpdateRoleDto input);
        Task<RoleDto> UpdateAsync(CreateUpdateRoleDto input,int id);
        Task DeleteAsync(int id);
        
        //  Task<RoleDto> CreateWithClaimsAsync(CreateUpdateRoleDto input);
        // Task<RoleDto> UpdateWithClaimsAsync(CreateUpdateRoleDto input,int id);
        //
        // public Task DeleteClaimByRoleIDAsync(int roleId);
        // public Task<CreateUpdateClaimRole> CreateClaimsAsync(List<CreateUpdateClaimRole> inputs);

        
        
        //role-claim
        public Task<List<RoleClaimDto>> GetClaimListAsync(int roleId);
        public Task<CreateUpdateClaimRole> CreateClaimAsync(CreateUpdateClaimRole input);
        public Task DeleteClaimAsync(RoleClaimModel input);


    }
}