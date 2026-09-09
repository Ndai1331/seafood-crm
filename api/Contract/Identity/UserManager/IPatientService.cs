
namespace Contract.Identity.UserManager
{
    public interface IPatientService
    {
        public Task<ApiResponseBase<List<UserDto>>> GetListByRoles(UserFilterPagingModel filter);
        public Task<ApiResponseBase<UserWithNavigationPropertiesDto>> GetWithNavigationProperties(int id);
        public Task<ApiResponseBase<UserDto>> CreateUserWithNavigationPropertiesAsync(CreateUserDto input);
        public Task<ApiResponseBase<UserDto>> UpdateUserWithNavigationPropertiesAsync(UpdateUserDto input, int id);
        public Task<ApiResponseBase<UserDto>> DeleteAsync(int id);
        public Task<UserDto> CreateAsync(CreateUserDto input);
        public Task<UserDto> UpdateAsync(UpdateUserDto input, int id);
    }
}
