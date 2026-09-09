using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Linq;
using Application.Identity.Common;
using Contract;
using Contract.Identity.UserManager;
using SqlServ4r.EntityFramework;
using Application.Identity.WebAuthn;
using Contract.Identity.WebAuthn;
using Core.Const;
using Core.Enum;
using Core.Exceptions;
using Core.Extension;
using Domain.Identity.Roles;
using Domain.Identity.Users;
using Domain.UserDepartments;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using SqlServ4r.Repository.Departments;
using SqlServ4r.Repository.Positions;
using SqlServ4r.Repository.RoleClaims;
using SqlServ4r.Repository.UserDepartments;
using SqlServ4r.Repository.UserRoles;
using SqlServ4r.Repository.Users;
using SqlServ4r.Repository.UserTokens;
using Volo.Abp.DependencyInjection;
using Core.Helper;

namespace Application.Identity.UserManager
{
    public partial class UserManagerService : ServiceBase, IUserManagerService, ITransientDependency
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly RoleClaimRepository _roleClaimRepository;
        private readonly IConfiguration _configuration;
        private readonly UserRoleRepository _userRoleRepository;
        private readonly UserTokenRepository _userTokenRepository;
        private readonly UserRepository _userRepository;
        private readonly UserDepartmentRepository _userDepartmentRepository;
        private readonly PositionRepository _positionRepository;
        private readonly DepartmentRepository _departmentRepository;
        private readonly IWebAuthnSettingService _webAuthnSettingService;
        private readonly DreamContext _dreamContext;
        private readonly ILogger<UserManagerService> _logger;
        public UserManagerService(
            UserManager<User> userManager,
            RoleManager<Role> roleManager,
            RoleClaimRepository roleClaimRepository,
            UserRoleRepository userRoleRepository,
            UserTokenRepository userTokenRepository,
            UserRepository userRepository,
            UserDepartmentRepository userDepartmentRepository,
            PositionRepository positionRepository,
            DepartmentRepository departmentRepository,
            IWebAuthnSettingService webAuthnSettingService,
            DreamContext dreamContext,
            ILogger<UserManagerService> logger,
            IConfiguration configuration
        )
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _roleClaimRepository = roleClaimRepository;
            _userRoleRepository = userRoleRepository;
            _userTokenRepository = userTokenRepository;
            _userRepository = userRepository;
            _userDepartmentRepository = userDepartmentRepository;
            _positionRepository = positionRepository;
            _departmentRepository = departmentRepository;
            _webAuthnSettingService = webAuthnSettingService;
            _dreamContext = dreamContext;
            _logger = logger;
        }

        public async Task<List<UserWithNavigationPropertiesDto>> GetListWithNavigationAsync()
        {
            var users = await _userRepository.GetListWithNavigationProperties();
            return ObjectMapper.Map<
                List<UserWithNavigationProperties>,
                List<UserWithNavigationPropertiesDto>
            >(users);
        }


        public async Task<List<UserBasicInfoDto>> GetBasicInfoUsersWithNavigationAsync(
            PhoneBookFilter filter
        )
        {
            var users = await _userRepository.GetUserBasicInfoWithNavigationProperties(filter.Text);
            return users;
        }

        public async Task<UserWithNavigationPropertiesDto> GetWithNavigationProperties(int id)
        {
            return ObjectMapper.Map<UserWithNavigationProperties, UserWithNavigationPropertiesDto>(
                await _userRepository.GetWithNavigationProperties(id)
            );
        }

        public async Task<UserDto> CreateUserWithNavigationPropertiesAsync(CreateUserDto input)
        {
            var existedUser = await _userManager.FindByNameAsync(input.UserCode);

            if (existedUser != null)
            {
                throw new GlobalException(
                    "Đã tồn tại tài khoản trên hệ thống. Vui lòng liên hệ với quản trị viên dể kích hoạt tài khoản.",
                    HttpStatusCode.BadRequest
                );
            }

            var dto = await CreateAsync(input);
            await UpdateRolesForUser(input.UserName, input.Roles);
            // var userDepartments = new List<UserDepartment>();
            // foreach (var item in input.DepartmentIds)
            // {
            //     userDepartments.Add(new UserDepartment() { UserId = dto.Id, DepartmentId = item });
            // }
            // await _userDepartmentRepository.AddRangeAsync(userDepartments);
            return dto;
        }

        public async Task<UserDto> UpdateUserWithNavigationPropertiesAsync(
            UpdateUserDto input,
            int id
        )
        {
            var item = await _userManager.FindByIdAsync(id.ToString());

            if (item == null)
            {
                throw new GlobalException(HttpMessage.NotFound, HttpStatusCode.BadRequest);
            }

            if (item.UserName != input.UserName)
            {
                var existedUser = await _userManager.FindByNameAsync(input.UserName);

                if (existedUser != null)
                {
                    throw new GlobalException(
                        "Đã tồn tại tài khoản trên hệ thống. Vui lòng liên hệ với quản trị viên dể kích hoạt tài khoản.",
                        HttpStatusCode.BadRequest
                    );
                }
            }

            var wasActive = item.IsActive;
            var user = ObjectMapper.Map(input, item);
            user.CreatedBy = item.CreatedBy;

            // Revoke session when the account is being deactivated so the old
            // refresh token cannot be exchanged for new access tokens.
            if (wasActive && !user.IsActive)
            {
                user.RefreshToken = null;
            }

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                throw new GlobalException(
                    result.Errors?.FirstOrDefault().Description,
                    HttpStatusCode.BadRequest
                );
            }

            if (input.IsSetPassword)
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var reuslt = await _userManager.ResetPasswordAsync(user, token, input.Password);
                if (!reuslt.Succeeded)
                {
                    throw new GlobalException(
                        reuslt.Errors?.FirstOrDefault().Description,
                        HttpStatusCode.BadRequest
                    );
                }
            }

            await UpdateRolesForUser(user.UserName, input.Roles);
            await _updateUserDepartmentForUser(user.Id, input.DepartmentIds);
            return ObjectMapper.Map<User, UserDto>(item);
        }


        public async Task<UserDto> UpdateBasicInfoFromDoctorInfo(
            UpdateUserDto input,
            int id
        )
        {
            var item = await _userManager.FindByIdAsync(id.ToString());

            if (item == null)
            {
                throw new GlobalException(HttpMessage.NotFound, HttpStatusCode.BadRequest);
            }

            if (item.UserName != input.UserName)
            {
                var existedUser = await _userManager.FindByNameAsync(input.UserName);

                if (existedUser != null)
                {
                    throw new GlobalException(
                        "Đã tồn tại tài khoản trên hệ thống. Vui lòng liên hệ với quản trị viên dể kích hoạt tài khoản.",
                        HttpStatusCode.BadRequest
                    );
                }
            }


            item.FirstName = input.FirstName;
            item.LastName = input.LastName;
            item.Email = input.Email;
            item.PhoneNumber = input.PhoneNumber;
            item.DOB = input.DOB;
            item.UserCode = input.UserCode;
            item.Gender = input.Gender;
            item.ModifiedBy = item.ModifiedBy;
            var result = await _userManager.UpdateAsync(item);

            if (!result.Succeeded)
            {
                throw new GlobalException(
                    result.Errors?.FirstOrDefault().Description,
                    HttpStatusCode.BadRequest
                );
            }

            return ObjectMapper.Map<User, UserDto>(item);
        }

        public async Task<List<UserIdentityDto>> GetBasicSeoUserInfosAsync(int? currentUserId = null,
         string? currentUserRole = null, UserType? userType = null, int? teamId = null)
        {
            var query = _userRepository
                .GetQueryable()
                .Include(x=>x.Position)
                .AsQueryable()
                .Where(x =>
                    !x.IsDelete
                    && x.IsActive
                    && x.Position != null
                    && (teamId.HasValue ? x.TeamId == teamId.Value : 1 == 1)
                );

            // If current user role is "SEO", only return current user's info
            if (currentUserRole == "SEO" && currentUserId.HasValue)
            {
                query = query.Where(x => x.Id == currentUserId.Value);
            }

            // Filter by UserType if specified
            if (userType.HasValue)
            {
                query = query.Where(x => x.UserType == userType.Value);
            }

            var users = await query.ToListAsync();
            return ObjectMapper.Map<List<User>, List<UserIdentityDto>>(users);
        }

        public async Task<List<UserIdentityDto>> GetBasicUserInfosAsync(
        )
        {
           var users = await _userRepository
                .GetQueryable()
                .Include(x=>x.Position)
                .AsQueryable()
                .Where(x =>
                    !x.IsDelete
                    && x.IsActive
                )
                .ToListAsync();
                return ObjectMapper.Map<List<User>, List<UserIdentityDto>>(users);
        }

        public async Task DeleteWithNavigationAsync(int id)
        {
            var item = await _userManager.FindByIdAsync(id.ToString());
            if (item == null)
                throw new GlobalException(HttpMessage.NotFound, HttpStatusCode.BadRequest);

            item.IsActive = false;
            item.RefreshToken = null;
            var userResult = await _userManager.UpdateAsync(item);
            if (!userResult.Succeeded)
            {
                throw new GlobalException(HttpMessage.CheckInformation, HttpStatusCode.BadRequest);
            }
        }

        public async Task<List<UserDto>> GetListAsync()
        {
            var users = await _userManager.Users.ToListAsync();

            return ObjectMapper.Map<List<User>, List<UserDto>>(users);
        }

        public async Task<UserDto> CreateAsync(CreateUserDto input)
        {
            // Không nuốt exception: lỗi tạo user (trùng username, sai policy...) phải
            // được ném ra để caller/middleware trả message thật về UI thay vì null
            // (null từng khiến UpdateRolesForUser ném NullReferenceException).
            (input.PhoneNumber, input.UserCode, input.Email) = TrimText(
                input.PhoneNumber,
                input.UserCode,
                input.Email
            );
            if (input.AvatarURL.IsNullOrWhiteSpace())
            {
                input.AvatarURL = _configuration["Media:DEFAULT_Avatar_URL"];
            }
            var user = ObjectMapper.Map<CreateUserDto, User>(input);

            var result = await _userManager.CreateAsync(user, input.Password);
            if (!result.Succeeded)
            {
                throw new GlobalException(
                    result.Errors?.FirstOrDefault()?.Description ?? "Không thể tạo người dùng.",
                    HttpStatusCode.BadRequest
                );
            }

            return ObjectMapper.Map<User, UserDto>(user);
        }

        public async Task<UserDto> UpdateAsync(UpdateUserDto input, int id)
        {
            (input.PhoneNumber, input.UserCode, input.Email) = TrimText(
                input.PhoneNumber,
                input.UserCode,
                input.Email
            );

            var item = await _userManager.FindByIdAsync(id.ToString());

            if (item == null)
            {
                throw new GlobalException(HttpMessage.NotFound, HttpStatusCode.BadRequest);
            }

            var user = ObjectMapper.Map(input, item);
            user.CreatedBy =
                input.CreatedBy == null || input.CreatedBy == null
                    ? item.CreatedBy
                    : input.CreatedBy;
            user.ModifiedBy =
                input.ModifiedBy == null || input.ModifiedBy == null
                    ? item.ModifiedBy
                    : input.ModifiedBy;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                throw new GlobalException(
                    result.Errors?.FirstOrDefault().Description,
                    HttpStatusCode.BadRequest
                );
            }

            return ObjectMapper.Map<User, UserDto>(user);
            ;
        }

        private (string Phone, string Code, string? Email) TrimText(
            string phone,
            string code,
            string email
        )
        {
            return (phone.Trim(), code.Trim(), email?.Trim());
        }

        public async Task DeleteAsync(int id)
        {
            var item = await _userManager.FindByIdAsync(id.ToString());
            if (item == null)
            {
                throw new GlobalException(HttpMessage.NotFound, HttpStatusCode.BadRequest);
            }
            item.IsDelete = true;
            item.RefreshToken = null;
            await _userRepository.UpdateAsync(item);
        }

        public async Task<TokenDto> SignInAsync(UserModel input)
        {
            var policy = await _webAuthnSettingService.GetAppliedAsync();

            // Wording, not logic: this message now reaches the screen verbatim, and "Check again
            // information" told a Vietnamese user nothing. Both branches below say the same thing
            // on purpose — an unknown account and a wrong password must not be told apart.
            var user = await _userManager.FindByNameAsync(input.UserName);
            if (user == null || !user.IsActive || user.IsDelete)
            {
                throw new GlobalException("Tài khoản hoặc mật khẩu không đúng.", HttpStatusCode.BadRequest);
            }

            var result = await _userManager.CheckPasswordAsync(user, input.Password);
            if (!result)
            {
                throw new GlobalException("Tài khoản hoặc mật khẩu không đúng.", HttpStatusCode.BadRequest);
            }

            // The switch has to bite here, not only on the login screen. Hiding the password form
            // while this method still accepts a password would make the whole setting decorative:
            // anyone who can post to the endpoint walks straight past it.
            //
            // Checked AFTER the password, not before: the answer would otherwise tell an anonymous
            // caller which accounts hold the exception. Accounts carrying it keep the password door
            // while the switch is off — that is how somebody without a key signs in to register one.
            if (!policy.AllowPasswordLogin && !user.PasswordLoginAllowed)
            {
                // BadRequest, not Forbidden, and the reason is the client rather than REST: the
                // browser layer turns 400 into an exception carrying this message, while 403 falls
                // through its default branch as a bare null — which the login screen can only
                // render as "wrong password". People retyped a correct password until they gave up.
                // Every other refusal on this path already answers 400.
                throw new GlobalException(
                    "Đăng nhập bằng mật khẩu đang tắt với tài khoản này. Hãy dùng khoá bảo mật, "
                  + "hoặc nhờ quản trị viên cấp quyền.",
                    HttpStatusCode.BadRequest);
            }

            // The switch has to bite here, not only on the login screen. Hiding the password form
            // while this method still accepts a password would make the whole setting decorative:
            // anyone who can post to the endpoint walks straight past it.
            //
            // Checked AFTER the password, not before: the answer would otherwise tell an anonymous
            // caller which accounts hold the exception. Accounts carrying it keep the password door
            // while the switch is off — that is how somebody without a key signs in to register one.
            if (!policy.AllowPasswordLogin && !user.PasswordLoginAllowed)
            {
                throw new GlobalException(
                    "Đăng nhập bằng mật khẩu đang tắt. Hãy dùng khoá bảo mật.",
                    HttpStatusCode.Forbidden);
            }

            // No second-factor branch here any more: IssueTokenForUserAsync owns that decision for
            // every login path, so no path can quietly skip what this one enforces.
            return await IssueTokenForUserAsync(user.Id, input);
        }

        public async Task<TokenDto> IssueTokenForUserAsync(int userId, UserModel? loginContext = null, SecondFactorProof proof = SecondFactorProof.None)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null || !user.IsActive || user.IsDelete)
            {
                throw new GlobalException(HttpMessage.CheckInformation, HttpStatusCode.BadRequest);
            }

            // Every way into the system ends up here — password, SSO, refresh — so this is where
            // the second factor is enforced. Putting it on any one of those paths instead is how
            // SSO ended up bypassing TOTP entirely.
            var pending = await ResolvePendingSecondFactorAsync(user, proof);
            if (pending != null)
            {
                return pending;
            }

            string accessToken = await GenerateTokenByUser(user);
            string refreshToken = GenerateRefreshToken();

            user.RefreshToken = refreshToken;
            var persisted = await _userManager.UpdateAsync(user);
            if (!persisted.Succeeded)
            {
                // Handing back a refresh token the server did not store would leave the caller
                // holding a credential that can never be redeemed.
                throw new GlobalException(HttpMessage.Conflict, HttpStatusCode.TooManyRequests);
            }

            await CreateUserToken(
                user,
                loginContext?.LoginProvider ?? "Website",
                loginContext?.DeviceId ?? "AccessToken",
                loginContext?.Token ?? ""
            );

            return new TokenDto()
            {
                UserId = user.Id.ToString(),
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
        }

        public async Task<TokenDto> ImpersonateAsync(int targetUserId, int impersonatorId)
        {
            var target = await _userManager.FindByIdAsync(targetUserId.ToString());
            if (target == null || !target.IsActive || target.IsDelete)
            {
                throw new GlobalException(HttpMessage.CheckInformation, HttpStatusCode.BadRequest);
            }

            // Only a peer SUPER_ADMIN account is off-limits; every lower role (ADMIN included) may be entered.
            var targetRoles = await _userManager.GetRolesAsync(target);
            if (targetRoles.Any(r => r == Core.Const.RoleNames.SuperAdmin))
            {
                throw new GlobalException(
                    "Không thể đăng nhập hộ tài khoản SUPER_ADMIN.", HttpStatusCode.Forbidden);
            }

            // Token carries impersonator_id for audit/banner; short exp, no refresh token.
            var accessToken = await GenerateTokenByUser(target, impersonatorId);
            return new TokenDto()
            {
                UserId = target.Id.ToString(),
                AccessToken = accessToken,
            };
        }

        public async Task<ApiResponseBase<bool>> CreateTokenFireBase(TokenFribaseModel token)
        {
            ApiResponseBase<bool> result = new ApiResponseBase<bool>();
            try
            {
                if (token is null || token.AccessToken is null)
                    throw new GlobalException(
                        HttpMessage.Unauthorized,
                        HttpStatusCode.Unauthorized
                    );
                var principal = GetPrincipalFromExpiredToken(token.AccessToken.Replace("\"", ""));
                var userName = principal.Identity.Name;

                var user = await _userManager.FindByNameAsync(userName);
                if (user != null)
                {
                    return await CreateUserToken(
                        user,
                        token.LoginProvider ?? "Website",
                        token.DeviceId ?? "AccessToken",
                        token.FireBaseToken ?? ""
                    );
                }
                else
                {
                    result.Data = false;
                    result.Message = "User not found";
                    return result;
                }
            }
            catch (Exception ex)
            {
                result.Data = false;
                result.Message = ex.Message;
                return result;
            }
        }
        private async Task<ApiResponseBase<bool>> CreateUserToken(
            User user,
            string loginProvider,
            string name,
            string value
        )
        {
            ApiResponseBase<bool> result = new ApiResponseBase<bool>();
            try
            {
                //check token exits
                var userToken = await _userTokenRepository.FirstOrDefaultAsync(x =>
                    x.UserId == user.Id && x.LoginProvider == loginProvider
                );
                //if exits ignore, not exits create new token
                if (userToken == null)
                {
                    await _userManager.SetAuthenticationTokenAsync(
                        user,
                        loginProvider,
                        name,
                        value
                    );
                }
                else
                {
                    userToken.Value = value;
                    await _userTokenRepository.UpdateAsync(userToken);
                }
                result.Data = true;
                return result;
            }
            catch (Exception ex)
            {
                result.Data = false;
                result.Message = ex.Message;
                return result;
            }
        }

        public async Task<UserDto> SignUpAsync(CreateUserDto input)
        {
            var user = ObjectMapper.Map<CreateUserDto, User>(input);

            var result = await _userManager.CreateAsync(user, input.Password);
            if (!result.Succeeded)
            {
                throw new GlobalException(
                    result.Errors.FirstOrDefault().Description,
                    HttpStatusCode.BadRequest
                );
            }

            return ObjectMapper.Map<User, UserDto>(user);
        }

        public async Task<UserDto> UpdateProfile(UpdateUserProfileRequestDto input)
        {
            var item = await _userManager.FindByIdAsync(input.Id.ToString());
            if (item == null)
                throw new GlobalException(HttpMessage.NotFound, HttpStatusCode.BadRequest);
            var user = ObjectMapper.Map(input, item);

            await _userRepository.UpdateAsync(item);
            return ObjectMapper.Map<User, UserDto>(item);
        }

        public async Task<bool> SetNewPasswordAsync(NewUserPasswordDto input)
        {
            var user = await _userManager.FindByNameAsync(input.UserName);
            if (user == null)
            {
                throw new GlobalException(HttpMessage.CheckInformation, HttpStatusCode.BadRequest);
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, input.NewPassword);
            if (!result.Succeeded)
            {
                throw new GlobalException(
                    result.Errors.FirstOrDefault().Description,
                    HttpStatusCode.BadRequest
                );
            }

            return true;
        }

        public async Task UpdateRolesForUser(string userName, List<string> roles)
        {
            var user = await _userManager.FindByNameAsync(userName);
            var oldRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, oldRoles);
            await _userManager.AddToRolesAsync(user, roles);
        }

        private async Task _updateUserDepartmentForUser(int userId, List<int> newDepartmentId)
        {
            var oldDepartmentUsers = await _userDepartmentRepository.GetListAsync(x =>
                x.UserId == userId
            );
            _userDepartmentRepository.RemoveRange(oldDepartmentUsers);

            var newUserDepartments = new List<UserDepartment>();
            foreach (var item in newDepartmentId)
            {
                newUserDepartments.Add(
                    new UserDepartment() { UserId = userId, DepartmentId = item }
                );
            }

            await _userDepartmentRepository.AddRangeAsync(newUserDepartments);
        }

        public Task<UserProfileModel> UpdateUserProfileAsync(UserProfileModel userProfileModel)
        {
            throw new NotImplementedException();
        }

        public async Task<TokenDto> RefreshTokenAsync(TokenModel token)
        {
            if (token is null)
                throw new GlobalException(HttpMessage.Unauthorized, HttpStatusCode.Unauthorized);

            var principal = GetPrincipalFromExpiredToken(token.AccessToken);

            var userName = principal.Identity.Name;

            var user = await _userManager.FindByNameAsync(userName);

            if (user == null || !user.IsActive || user.IsDelete)
                throw new GlobalException(HttpMessage.Unauthorized, HttpStatusCode.Unauthorized);

            // Logout nulls out user.RefreshToken. Comparing with != would make a request that
            // also sends null compare equal, handing a live session to whoever asks.
            if (string.IsNullOrEmpty(user.RefreshToken) || string.IsNullOrEmpty(token.RefreshToken))
                throw new GlobalException(HttpMessage.Unauthorized, HttpStatusCode.Unauthorized);

            if (!CryptographicOperations.FixedTimeEquals(
                    Encoding.UTF8.GetBytes(user.RefreshToken),
                    Encoding.UTF8.GetBytes(token.RefreshToken)))
                throw new GlobalException(HttpMessage.Unauthorized, HttpStatusCode.Unauthorized);

            // Routed through the choke point rather than minting a token here. Refresh tokens carry
            // no expiry of their own, so a session opened before the second factor became mandatory
            // could otherwise be renewed forever and never be asked for a key. When a factor is
            // owed, the caller gets the same flags the login path returns instead of a session.
            return await IssueTokenForUserAsync(user.Id, proof: SecondFactorProof.ExistingSession);
        }

        public async Task Logout(UserLogOutModel input)
        {
            var userToken = await _userTokenRepository.FirstOrDefaultAsync(x =>
                x.UserId == input.UserId && x.Name == (input.DeviceId)
            );
            var user = await _userManager.FindByIdAsync(input.UserId.ToString());

            if (userToken != null)
            {
                user.RefreshToken = null;
                await _userManager.UpdateAsync(user);

                await _userManager.RemoveAuthenticationTokenAsync(
                    user,
                    userToken.LoginProvider,
                    userToken.Name
                );
            }
        }

        public async Task LogoutWebsite(WebisteUserLogOutModel token)
        {
            var principal = GetPrincipalFromExpiredToken(token.AccessToken.Replace("\"", ""));
            var userName = principal.Identity.Name;
            var user = await _userManager.FindByNameAsync(userName);
            var userToken = await _userTokenRepository.FirstOrDefaultAsync(x =>
                x.UserId == user.Id && (x.Name == (token.DeviceId)
                || x.Name == "AccessToken")
            );
            if (userToken != null)
            {
                string provider = !string.IsNullOrEmpty(userToken.LoginProvider)
                    ? userToken.LoginProvider
                    : "Website";

                user.RefreshToken = null;
                await _userManager.UpdateAsync(user);
                await _userManager.RemoveAuthenticationTokenAsync(user, provider, userToken.Name);
            }
        }

        private async Task<string> GenerateTokenByUser(User user, int? impersonatorId = null)
        {
            var secretKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["Jwt:Key"])
            );
            var signinCredentials = new SigningCredentials(
                secretKey,
                SecurityAlgorithms.HmacSha256
            );
            var userRoles = await _userManager.GetRolesAsync(user);
            var userRole = await _userRoleRepository.GetListAsync(x => x.UserId == user.Id);

            var userRoleClaims = _roleClaimRepository.GetRoleClaimsByRoles(
                userRole.Select(x => x.RoleId).ToList()
            );
            var userClaims = userRoleClaims.Select(x => new Claim(x.ClaimType, x.ClaimValue));

            List<Claim> claims = new List<Claim>();

            claims.AddRange(userClaims);
            foreach (var item in userRoles)
            {
                claims.Add(new Claim(ClaimTypes.Role, item));
            }

            claims.Add(new Claim(ClaimTypes.PrimarySid, user.Id.ToString()));
            claims.Add(new Claim(ClaimTypes.Name, user.UserName));
            claims.Add(new Claim(ClaimTypes.Surname, user.FirstName + " " + user.LastName));
            claims.Add(new Claim("Code", user.UserCode));

            // Impersonation: mark the token so the UI banner and audit trail know who is acting,
            // and keep it short-lived (JWT can't be revoked mid-session).
            if (impersonatorId.HasValue)
            {
                claims.Add(new Claim(Core.Const.ImpersonationClaim.ImpersonatorId, impersonatorId.Value.ToString()));
            }

            var tokeOptions = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: impersonatorId.HasValue ? DateTime.UtcNow.AddMinutes(30) : DateTime.UtcNow.AddHours(24),
                signingCredentials: signinCredentials
            );
            var tokenString = new JwtSecurityTokenHandler().WriteToken(tokeOptions);

            return tokenString;
        }


        /// <summary>
        /// Returns a TokenDto describing what the caller still has to do, or null when the
        /// session may be issued.
        /// </summary>
        private async Task<TokenDto?> ResolvePendingSecondFactorAsync(User user, SecondFactorProof proof)
        {
            var settings = await _webAuthnSettingService.GetAppliedAsync();

            // Guarded for the same reason the settings read is: the two DDL scripts are applied by
            // hand and independently, so "settings table exists, keys table does not" is a real
            // window. Unguarded, every sign-in throws during it.
            var (hasYubikey, hasPasskey) = settings.IsEnabled
                ? await LookupSecurityKeysAsync(user.Id)
                : (false, false);
            var hasKey = hasYubikey || hasPasskey;
            var hasTotp = _configuration.GetValue<bool>("Totp:Enabled") && user.IsTotpEnabled;

            var step = WebAuthnLoginDecision.Decide(
                featureEnabled: settings.IsEnabled,
                enforced: settings.IsEnforced,
                userHasSecurityKey: hasKey,
                userHasTotp: hasTotp,
                proof: proof);

            return step switch
            {
                WebAuthnLoginStep.RequireSecurityKey => new TokenDto
                {
                    UserId = user.Id.ToString(),
                    RequiresSecurityKey = true,
                    TemporaryToken = GenerateTemporaryToken(user, WebAuthnTokenTypes.SecurityKeyStep),
                    // Which doors can still finish this sign-in. Built and filtered here — the
                    // login screen only renders it, so hiding a factor client-side is never the
                    // thing keeping it closed.
                    AvailableSecondFactors = WebAuthnLoginDecision.AvailableSecondFactors(
                        hasYubikey, hasPasskey, hasTotp, proof)
                },
                WebAuthnLoginStep.RequireEnrollment => new TokenDto
                {
                    UserId = user.Id.ToString(),
                    RequiresSecurityKeyEnrollment = true,
                    TemporaryToken = GenerateTemporaryToken(user, WebAuthnTokenTypes.Enrollment, GetEnrollTokenExpiryMinutes())
                },
                WebAuthnLoginStep.RequireTotp => new TokenDto
                {
                    UserId = user.Id.ToString(),
                    RequiresTwoFactor = true,
                    TemporaryToken = GenerateTemporaryToken(user, TotpTemporaryTokenType)
                },
                _ => null
            };
        }

        /// <summary>
        /// Which kinds of key this person holds. Per table rather than one bool, because the login
        /// screen draws a different door for each: a YubiKey wants its touch prompt, a passkey
        /// wants the browser ceremony. For the gate itself either kind counts.
        /// </summary>
        private async Task<(bool HasYubikey, bool HasPasskey)> LookupSecurityKeysAsync(int userId)
        {
            try
            {
                var hasYubikey = await _dreamContext.UserYubikeys.AnyAsync(x => x.UserId == userId);
                var hasPasskey = await _dreamContext.UserSecurityKeys.AnyAsync(x => x.UserId == userId);
                return (hasYubikey, hasPasskey);
            }
            catch (Exception ex)
            {
                // Degrade to "no key" rather than breaking the login path outright.
                _logger.LogWarning(ex, "Could not read the security-key tables; treating user {UserId} as having none.", userId);
                return (false, false);
            }
        }

        private int GetEnrollTokenExpiryMinutes()
        {
            var minutes = _configuration.GetValue<int?>("WebAuthn:EnrollTokenExpiryMinutes") ?? 10;
            return minutes <= 0 ? 10 : minutes;
        }

        private const string TotpTemporaryTokenType = "totp_temp";

        public async Task<string?> CreateCeremonyTokenAsync(int userId, string tokenType, int expiryMinutes)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null || !user.IsActive || user.IsDelete)
            {
                return null;
            }

            return GenerateTemporaryToken(user, tokenType, expiryMinutes);
        }

        private string GenerateTemporaryToken(User user) => GenerateTemporaryToken(user, TotpTemporaryTokenType);

        private string GenerateTemporaryToken(User user, string tokenType, int? expiryMinutesOverride = null)
        {
            var secretKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["Jwt:Key"])
            );
            var signinCredentials = new SigningCredentials(
                secretKey,
                SecurityAlgorithms.HmacSha256
            );
            var expiryMinutes = expiryMinutesOverride
                ?? _configuration.GetValue<int?>("Totp:TemporaryTokenExpiryMinutes")
                ?? 5;
            if (expiryMinutes <= 0)
            {
                expiryMinutes = 5;
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.PrimarySid, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
                new Claim(TwoFactorTokens.TokenTypeClaim, tokenType),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N"))
            };

            // Deliberately NOT Jwt:Audience. The bearer scheme validates the session audience, so
            // issuing this half-authenticated token under a separate one is what stops it from
            // authenticating against every [Authorize] endpoint before the second factor is proven.
            var tokenOptions = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: TwoFactorTokens.ResolveAudience(_configuration),
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
                signingCredentials: signinCredentials
            );

            return new JwtSecurityTokenHandler().WriteToken(tokenOptions);
        }

        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }

        public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false, //you might want to validate the audience and issuer depending on your use case
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(_configuration["Jwt:Key"])
                ),
                ValidateLifetime = false //here we are saying that we don't care about the token's expiration date
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken securityToken;
            var principal = tokenHandler.ValidateToken(
                token,
                tokenValidationParameters,
                out securityToken
            );
            var jwtSecurityToken = securityToken as JwtSecurityToken;
            if (
                jwtSecurityToken == null
                || !jwtSecurityToken.Header.Alg.Equals(
                    SecurityAlgorithms.HmacSha256,
                    StringComparison.InvariantCultureIgnoreCase
                )
            )
                throw new SecurityTokenException("Invalid token");

            // Guarding here rather than at one call site: this helper deliberately skips audience
            // and lifetime checks, so it is the one place every caller shares. A half-authenticated
            // token must not be accepted by ANY of them.
            if (principal.FindFirst(TwoFactorTokens.TokenTypeClaim) != null)
                throw new SecurityTokenException("Invalid token");

            return principal;
        }

        public async Task<List<UserDto>> GetListByRoles(UserFilterPagingModel? filter = null)
        {
            var users = await _userRepository.GetListByRoles(filter);

            return ObjectMapper.Map<List<User>, List<UserDto>>(users);
        }

        public async Task<ApiResponseBase<List<UserDto>>> GetListByFilterAsync(
            UserFilterPagingModel filter
        )
        {
            ApiResponseBase<List<UserDto>> result = new ApiResponseBase<List<UserDto>>();
            try
            {
                List<UserDto> users = new List<UserDto>();
                //var userRoles = _userRoleRepository.GetQueryable().Where(x=>x.)

            users = await (
                from u in _userRepository.GetQueryable().Where(x => !x.IsDelete)
                join u_r in _userRoleRepository.GetQueryable()
                    on u.Id equals u_r.UserId
                    into u_rs
                from urss in u_rs.DefaultIfEmpty()
                where  (filter.Gender.HasValue ? u.Gender == filter.Gender : 1 == 1)
                    && (
                        filter.RoleIds != null && filter.RoleIds.Count > 0
                            ? filter.RoleIds.Any(r => r == urss.RoleId)
                            : 1 == 1
                    )
                    && (filter.DobFrom.HasValue ? u.DOB >= filter.DobFrom : 1 == 1)
                    && (filter.DobTo.HasValue ? u.DOB <= filter.DobTo : 1 == 1)
                    && (filter.TeamId.HasValue ? u.TeamId == filter.TeamId : 1 == 1)
                    && (
                        !string.IsNullOrEmpty(filter.FilterText)
                            ? (u.Email.Contains(filter.FilterText))
                                || (u.PhoneNumber.Contains(filter.FilterText))
                                || (u.UserCode.Contains(filter.FilterText))
                                || (u.LastName + " " + u.FirstName).Contains(filter.FilterText)
                            : 1 == 1
                    )
                orderby u.CreatedDate descending 
                select new UserDto
                {
                    Id = u.Id,
                    UserCode = u.UserCode,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    FullName = $"{u.FirstName} {u.LastName}",
                    DOB = u.DOB,
                    PhoneNumber = u.PhoneNumber,
                    Gender = u.Gender,
                    Email = u.Email,
                    TeamId = u.TeamId,
                    TeamCode = u.Team != null ? u.Team.Code : null,
                    TeamName = u.Team != null ? u.Team.Name : null
                }
            )
                .Distinct()
                .Skip(filter.Skip)
                .Take(filter.Take).ToListAsync();
                result.Data = users;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
            }
            return result;
        }
        private async Task<List<int>> GetChildrenDeparment(int parentId)
        {
            var departmentIds = new List<int>();
            departmentIds.Add(parentId);

            var items = await _departmentRepository.GetListAsync(x => x.ParentCode == parentId);

            foreach (var f in items)
            {
                departmentIds.AddRange(await GetChildrenDeparment(f.Id));
            }

            return departmentIds;
        }




        public async Task<UserDto> CloneUserAsync(CloneUserDto input)
        {
            var user = await _userManager.FindByIdAsync(input.Id.ToString());
            if (user == null)
            {
                throw new GlobalException(HttpMessage.NotFound, HttpStatusCode.BadRequest);
            }

            var roles = await _userManager.GetRolesAsync(user);
            var roleStrings = new List<string>();
            foreach (var role in roles)
            {
                roleStrings.Add(role);
            }

            var newUser = new User() {
                UserName = $"CLONE_{user.UserCode}",
                UserCode = $"CLONE_{user.UserCode}",
                FirstName = user.FirstName,
                LastName = user.LastName,
                Gender = user.Gender,
                DOB = user.DOB,
                PhoneNumber = user.PhoneNumber,
                Email = user.Email,
                IsActive = true,
                IsDelete = false,
                CreatedDate = DateTime.Now,
                ModifiedDate = DateTime.Now,
                CreatedBy = user.CreatedBy,
                ModifiedBy = user.ModifiedBy,
                AvatarURL = user.AvatarURL,
                Address = user.Address,
                DependsId = user.DependsId,
                Relationship = user.Relationship,
                UserDepartments = user.UserDepartments,

                TeamId = user.TeamId,
                PositionId = user.PositionId,
            };


            await _userManager.CreateAsync(newUser, "123456");

            await UpdateRolesForUser(newUser.UserName, roleStrings);

            return ObjectMapper.Map<User, UserDto>(newUser);
        }
    }
}
