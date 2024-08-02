using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

// See <https://github.com/dotnet/aspnetcore/issues/52286#issuecomment-1947672182>
namespace TechnicalTaskAPI.Application.Identity.Services
{
    public class BearerTokenHandler : TokenHandler
    {
        private readonly JwtSecurityTokenHandler _tokenHandler = new();

        public override Task<TokenValidationResult> ValidateTokenAsync(string token, TokenValidationParameters validationParameters)
        {
            try
            {
                _tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);

                if (validatedToken is not JwtSecurityToken jwtSecurityToken)
                    return Task.FromResult(new TokenValidationResult() { IsValid = false });

                return Task.FromResult(new TokenValidationResult
                {
                    IsValid = true,
                    ClaimsIdentity = new ClaimsIdentity(jwtSecurityToken.Claims, JwtBearerDefaults.AuthenticationScheme),
                    SecurityToken = jwtSecurityToken,
                });
            }

            catch (Exception e)
            {
                return Task.FromResult(new TokenValidationResult
                {
                    IsValid = false,
                    Exception = e,
                });
            }
        }
    }
}
