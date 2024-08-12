using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using TechnicalTaskAPI.ORM.Entities;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using TechnicalTaskAPI.ORM.Services;
using TechnicalTaskAPI.Application.Identity.Models;

namespace TechnicalTaskAPI.Application.Identity.Services
{
    public interface ITokenService
    {
        TokenResponse CreateToken(ApplicationUser user);
        Task<TokenResponse> GenerateRefreshTokenAsync(ApplicationUser user, CancellationToken cancellationToken);
        bool ValidateRefreshTokenAsync(string refreshToken, ApplicationUser user);
        Task RevokeRefreshTokenAsync(string refreshToken);
    }
    public class TokenService : ITokenService
    {
        // Adjustexpiration if needed
        private const double ExpirationMinutes = 1;
        private readonly ILogger<TokenService> _logger;
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TokenService(ILogger<TokenService> logger, IConfiguration configuration, ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            _configuration = configuration;
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public TokenResponse CreateToken(ApplicationUser user)
        {
            var expiration = DateTime.UtcNow.AddMinutes(ExpirationMinutes);

            var token = CreateJwtToken(
                  CreateClaims(user),
                  CreateSigningCredentials(),
                  expiration
              );
            var tokenHandler = new JwtSecurityTokenHandler();

            _logger.LogInformation("JWT Token created");

            var tokenSerilizated =  tokenHandler.WriteToken(token);

            var tokenResponse = new TokenResponse { Token = tokenSerilizated, TokenExpirationDate = expiration };

            return tokenResponse;
        }

        private JwtSecurityToken CreateJwtToken(List<Claim> claims, SigningCredentials credentials, DateTime expiration)
        {
            var issuer = _configuration["Jwt:Issuer"];
            var audience = _configuration["Jwt:Audience"];

            return new JwtSecurityToken(
                issuer,
                audience,
                claims,
                expires: expiration,
                signingCredentials: credentials
            );
        }

        private List<Claim> CreateClaims(ApplicationUser user)
        {
            return new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, EpochTime.GetIntDate(DateTime.UtcNow).ToString(), ClaimValueTypes.Integer64),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };
        }

        private SigningCredentials CreateSigningCredentials()
        {
            var key = _configuration["Jwt:Key"];
            var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));

            return new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);
        }

        public async Task<TokenResponse> GenerateRefreshTokenAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
            var expiration = DateTime.UtcNow.AddDays(7);

            user.RefreshToken = refreshToken;

            user.RefreshTokenExpirationDate = expiration;

            _context.Users.Update(user);

            await _context.SaveChangesAsync();

            var tokenResponse = new TokenResponse 
            {
                Token = refreshToken,
                TokenExpirationDate = expiration
            };

            return tokenResponse;
        }
        public bool ValidateRefreshTokenAsync(string refreshToken, ApplicationUser user)
        {
            if (user.RefreshTokenExpirationDate < DateTime.UtcNow)
            {
                return false;
            }

            return true;
        }

        public async Task RevokeRefreshTokenAsync(string refreshToken)
        {
            var userList = await _context.Users.ToListAsync();
            var user = userList.Where(a => a.RefreshToken == refreshToken).FirstOrDefault();

            if (user != null)
            {
                user.RefreshToken = null;
                user.RefreshTokenExpirationDate = null;

                await _context.SaveChangesAsync();
            }else 
             {
                throw new Exception("User dont have token");
            }
        }
    }
}
