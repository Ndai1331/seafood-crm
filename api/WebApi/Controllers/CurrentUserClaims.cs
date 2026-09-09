using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

/// <summary>
/// Reads the signed-in account out of the JWT.
/// <para>
/// The claim names vary by token issuer, which is why each lookup tries several. This is the one
/// home for that list — change an accepted claim name here, not in a controller.
/// </para>
/// <para>
/// <see cref="UserController"/> keeps its own private user-id reader on purpose: that one does not
/// accept <see cref="ClaimTypes.NameIdentifier"/>, so pointing it here would widen which tokens
/// resolve to an account. Narrowing that gap is an auth decision, not a refactor.
/// </para>
/// </summary>
public static class CurrentUserClaims
{
    /// <summary>The account id, or null when the request is not authenticated.</summary>
    public static int? UserId(this ControllerBase controller)
    {
        var claim = controller.HttpContext.User.FindFirst("sub")
            ?? controller.HttpContext.User.FindFirst("userId")
            ?? controller.HttpContext.User.FindFirst("primarysid")
            ?? controller.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)
            ?? controller.HttpContext.User.FindFirst("http://schemas.microsoft.com/ws/2008/06/identity/claims/primarysid");

        return claim != null && int.TryParse(claim.Value, out var id) ? id : null;
    }

    /// <summary>The role string, or null when the token carries none.</summary>
    public static string? Role(this ControllerBase controller)
    {
        var claim = controller.HttpContext.User.FindFirst("role")
            ?? controller.HttpContext.User.FindFirst(ClaimTypes.Role)
            ?? controller.HttpContext.User.FindFirst("http://schemas.microsoft.com/ws/2008/06/identity/claims/role");

        return claim?.Value;
    }
}
