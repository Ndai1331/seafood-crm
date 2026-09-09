using Microsoft.AspNetCore.Http;

namespace Application.Common
{
    /// <summary>
    /// Helper to extract current user information from HttpContext.
    /// </summary>
    public static class CurrentUserAccessor
    {
        /// <summary>
        /// Get current user id from HttpContext (claim primarysid). Returns null if not found.
        /// </summary>
        public static int? GetCurrentUserId(IHttpContextAccessor httpContextAccessor)
        {
            var userIdClaim = httpContextAccessor.HttpContext?.User?.Claims?
                .FirstOrDefault(x => x.Type.EndsWith("/primarysid", StringComparison.OrdinalIgnoreCase))?.Value;

            if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out var userId))
            {
                return userId;
            }

            return null;
        }

        /// <summary>
        /// Get current username from HttpContext (claim name). Returns null if not found.
        /// </summary>
        public static string? GetCurrentUserName(IHttpContextAccessor httpContextAccessor)
        {
            var name = httpContextAccessor.HttpContext?.User?.Identity?.Name;
            return string.IsNullOrWhiteSpace(name) ? null : name;
        }
    }
}
