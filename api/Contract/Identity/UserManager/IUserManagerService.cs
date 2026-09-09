using Contract.Identity.WebAuthn;
using Contract.Uploads;
using Core.Enum;

namespace Contract.Identity.UserManager
{
    public interface IUserManagerService
    {
        public Task<List<UserWithNavigationPropertiesDto>> GetListWithNavigationAsync();

        public Task<List<UserBasicInfoDto>> GetBasicInfoUsersWithNavigationAsync(
            PhoneBookFilter filter
        );

        public Task<UserWithNavigationPropertiesDto> GetWithNavigationProperties(int id);

        public Task<UserDto> CreateUserWithNavigationPropertiesAsync(CreateUserDto input);
        public Task<UserDto> UpdateUserWithNavigationPropertiesAsync(UpdateUserDto input, int id);
        public Task<UserDto> UpdateBasicInfoFromDoctorInfo(UpdateUserDto input, int id);
        public Task DeleteWithNavigationAsync(int id);
        public Task<List<UserDto>> GetListAsync();
        public Task<List<UserDto>> GetListByRoles(UserFilterPagingModel? filter = null);
        public Task<List<UserIdentityDto>> GetBasicUserInfosAsync(
        );
        public Task<List<UserIdentityDto>> GetBasicSeoUserInfosAsync(int? currentUserId = null, 
        string? currentUserRole = null, UserType? userType = null, int? teamId = null);

        public Task<UserDto> CreateAsync(CreateUserDto input);
        public Task<UserDto> UpdateAsync(UpdateUserDto input, int id);
        public Task DeleteAsync(int id);
        public Task<TokenDto> SignInAsync(UserModel input);

        /// <summary>
        /// Issue a short-lived, read-only impersonation token for <paramref name="targetUserId"/>.
        /// Caller (SUPER_ADMIN) is recorded via the impersonator_id claim. Target must not be
        /// ADMIN/SUPER_ADMIN. Enforced further up by the controller (SUPER_ADMIN only).
        /// </summary>
        public Task<TokenDto> ImpersonateAsync(int targetUserId, int impersonatorId);
        /// <summary>
        /// The single choke point every login path funnels through. Pass
        /// a non-None <paramref name="proof"/> ONLY from a completed second-factor ceremony; the
        /// default of None is what makes a future login path fail closed instead of quietly
        /// skipping the factor.
        /// </summary>
        public Task<TokenDto> IssueTokenForUserAsync(int userId, UserModel? loginContext = null, SecondFactorProof proof = SecondFactorProof.None);

        /// <summary>
        /// Mints one of the short-lived tokens that authorise a single ceremony step. Exposed so
        /// the YubiKey path can open its own entrance without rebuilding the JWT by hand — issuer,
        /// audience and signing key have to match exactly or WebAuthnTokenValidator rejects it.
        /// </summary>
        public Task<string?> CreateCeremonyTokenAsync(int userId, string tokenType, int expiryMinutes);




        public Task<UserDto> SignUpAsync(CreateUserDto input);
        public Task<UserDto> UpdateProfile(UpdateUserProfileRequestDto input);
        public Task<bool> SetNewPasswordAsync(NewUserPasswordDto input);
        public Task<TokenDto> RefreshTokenAsync(TokenModel token);
        public Task<ApiResponseBase<List<UserDto>>> GetListByFilterAsync(
            UserFilterPagingModel? filter = null
        );
        public Task<ApiResponseBase<bool>> CreateTokenFireBase(TokenFribaseModel token);
        public Task LogoutWebsite(WebisteUserLogOutModel token);
        Task Logout(UserLogOutModel input);

        public Task<UserDto> CloneUserAsync(CloneUserDto input);
    }
}
