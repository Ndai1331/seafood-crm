using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using Application.Identity.Common;
using Core.Exceptions;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Volo.Abp.DependencyInjection;

namespace Application.Identity.WebAuthn
{
    /// <summary>Identity carried by a short-lived ceremony token.</summary>
    public record WebAuthnTokenIdentity(int UserId, string Jti);

    /// <summary>
    /// Validates the short-lived tokens that authorise a single WebAuthn ceremony.
    ///
    /// The expected token_type is always passed in by the caller and always checked. These tokens
    /// are not interchangeable: an enrollment token must not be able to finish a login, and a
    /// login token must not be able to register a key.
    /// </summary>
    public class WebAuthnTokenValidator : ITransientDependency
    {
        private readonly IConfiguration _configuration;

        public WebAuthnTokenValidator(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public WebAuthnTokenIdentity Validate(string? token, string expectedTokenType)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                throw Unauthorized();
            }

            var parameters = new TokenValidationParameters
            {
                ValidIssuer = _configuration["Jwt:Issuer"],
                ValidAudience = TwoFactorTokens.ResolveAudience(_configuration),
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? string.Empty)),
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            try
            {
                var principal = new JwtSecurityTokenHandler().ValidateToken(token, parameters, out _);

                if (principal.FindFirst(TwoFactorTokens.TokenTypeClaim)?.Value != expectedTokenType)
                {
                    throw Unauthorized();
                }

                var userIdValue = principal.FindFirst(ClaimTypes.PrimarySid)?.Value;
                var jti = principal.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;

                if (!int.TryParse(userIdValue, out var userId) || string.IsNullOrWhiteSpace(jti))
                {
                    throw Unauthorized();
                }

                return new WebAuthnTokenIdentity(userId, jti);
            }
            catch (SecurityTokenException)
            {
                throw Unauthorized();
            }
            catch (ArgumentException)
            {
                throw Unauthorized();
            }
        }

        /// <summary>Deliberately identical for every failure: the caller learns nothing from which.</summary>
        private static GlobalException Unauthorized() =>
            new("Phiên xác thực đã hết hạn hoặc không hợp lệ.", HttpStatusCode.Unauthorized);
    }
}
