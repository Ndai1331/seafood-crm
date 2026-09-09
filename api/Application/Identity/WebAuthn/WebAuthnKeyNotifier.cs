using Domain.Identity.Users;
using Microsoft.Extensions.Logging;
using Volo.Abp.DependencyInjection;

namespace Application.Identity.WebAuthn
{
    /// <summary>
    /// Logs security-key attach/remove events. In-app notifications were dropped with the SEO stack.
    /// </summary>
    public class WebAuthnKeyNotifier : ITransientDependency
    {
        private readonly ILogger<WebAuthnKeyNotifier> _logger;

        public WebAuthnKeyNotifier(ILogger<WebAuthnKeyNotifier> logger)
        {
            _logger = logger;
        }

        public Task KeyRegisteredAsync(User user, string deviceName)
        {
            _logger.LogInformation("Security key '{Device}' registered for user {UserId}", deviceName, user.Id);
            return Task.CompletedTask;
        }

        public Task KeyRemovedAsync(User user, string deviceName, bool removedByAdmin)
        {
            _logger.LogInformation("Security key '{Device}' removed for user {UserId} (admin={Admin})", deviceName, user.Id, removedByAdmin);
            return Task.CompletedTask;
        }
    }
}
