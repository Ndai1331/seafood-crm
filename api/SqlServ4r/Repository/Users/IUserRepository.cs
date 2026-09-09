using Contract.Identity.UserManager;
using Domain.Identity.Users;

namespace SqlServ4r.Repository.Users
{
    public interface IUserRepository
    {
        bool CheckDuplicateInformation(
            string userCode
        );
        bool  CheckDuplicateInformation(
            string userCode,
            int id
        );

        Task<List<UserWithNavigationProperties>> GetListWithNavigationProperties(
        );
        Task<UserWithNavigationProperties> GetWithNavigationProperties(int id);
        Task<List<User>> GetListByRoles(UserFilterPagingModel? filter = null);
    }
}
