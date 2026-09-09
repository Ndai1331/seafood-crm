using Application.Identity.WebAuthn;
using System.Net;
using Contract.Identity.UserManager;
using Contract.Identity.WebAuthn;
using Contract.Identity.Yubico;
using Core.Exceptions;
using Domain.Identity.UserYubikeys;
using Domain.Identity.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SqlServ4r.EntityFramework;
using Volo.Abp.DependencyInjection;

namespace Application.Identity.Yubico
{
    /// <summary>
    /// YubiKey second factor, split between two authorities.
    ///
    /// Yubico's validation service says whether an OTP is genuine and has not been spent — the hard
    /// half, and the half that needs no secret of ours. This service says whose key it is, which
    /// Yubico cannot know. Neither answer is sufficient alone.
    /// </summary>
    public partial class YubikeyService : IYubikeyService, ITransientDependency
    {
        /// <summary>Five tries against a 4-digit PIN is 0.05% of the space. Fifteen minutes after that.</summary>
        private const int MaxPinAttempts = 5;
        private const int PinLockoutMinutes = 15;
        private const int MinPinLength = 4;
        private const int MaxPinLength = 20;

        private readonly DreamContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IUserManagerService _userManagerService;
        private readonly IWebAuthnSettingService _settingService;
        private readonly IPasswordHasher<User> _pinHasher;
        private readonly YubicoCloudClient _cloudClient;
        private readonly WebAuthnCeremonyStore _ceremonyStore;
        private readonly WebAuthnTokenValidator _tokenValidator;
        private readonly WebAuthnKeyNotifier _notifier;
        private readonly IConfiguration _configuration;
        private readonly ILogger<YubikeyService> _logger;

        public YubikeyService(
            DreamContext context,
            UserManager<User> userManager,
            IUserManagerService userManagerService,
            IWebAuthnSettingService settingService,
            IPasswordHasher<User> pinHasher,
            YubicoCloudClient cloudClient,
            WebAuthnCeremonyStore ceremonyStore,
            WebAuthnTokenValidator tokenValidator,
            WebAuthnKeyNotifier notifier,
            IConfiguration configuration,
            ILogger<YubikeyService> logger)
        {
            _context = context;
            _userManager = userManager;
            _userManagerService = userManagerService;
            _settingService = settingService;
            _pinHasher = pinHasher;
            _cloudClient = cloudClient;
            _ceremonyStore = ceremonyStore;
            _tokenValidator = tokenValidator;
            _notifier = notifier;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<YubikeyFeatureStatusDto> GetFeatureStatusAsync()
        {
            var settings = await _settingService.GetAppliedAsync();
            return new YubikeyFeatureStatusDto
            {
                Enabled = settings.IsEnabled,
                Enforced = settings.IsEnforced,
                PinRequired = true,
                AllowPasswordLogin = settings.AllowPasswordLogin
            };
        }

        public async Task<List<YubikeyDto>> GetKeysAsync(int userId)
        {
            var keys = await _context.UserYubikeys
                .AsNoTracking()
                .Where(x => x.UserId == userId)
                .OrderBy(x => x.CreatedAt)
                .ToListAsync();

            return keys.Select(k => new YubikeyDto
            {
                Id = k.Id,
                PublicId = k.PublicId,
                DeviceName = k.DeviceName,
                HasPin = k.PinHash != null,
                Confirmed = k.LastUsedAt.HasValue,
                CreatedAt = k.CreatedAt,
                LastUsedAt = k.LastUsedAt
            }).ToList();
        }

        public async Task DeleteKeyAsync(int userId, int keyId)
        {
            var key = await _context.UserYubikeys
                .FirstOrDefaultAsync(x => x.Id == keyId && x.UserId == userId)
                ?? throw new GlobalException("Không tìm thấy khoá bảo mật.", HttpStatusCode.NotFound);

            var settings = await _settingService.GetAppliedAsync();
            var remaining = await _context.UserYubikeys.CountAsync(x => x.UserId == userId) - 1;

            // Enforced plus zero keys equals locked out, with an admin as the only way back. Not
            // something a user should be able to do to themselves with one click.
            if (settings.IsEnabled && settings.IsEnforced && remaining <= 0)
            {
                throw new GlobalException(
                    "Không thể gỡ khoá cuối cùng. Hãy đăng ký khoá dự phòng trước.",
                    HttpStatusCode.BadRequest);
            }

            var user = await GetActiveUserAsync(userId);
            _context.UserYubikeys.Remove(key);
            await _context.SaveChangesAsync();

            await _notifier.KeyRemovedAsync(user, key.DeviceName, removedByAdmin: false);
        }

        private async Task<User> GetActiveUserAsync(int userId)
        {
            return await _userManager.Users.FirstOrDefaultAsync(x => x.Id == userId)
                ?? throw new GlobalException("Không tìm thấy tài khoản.", HttpStatusCode.NotFound);
        }
    }
}
