using MediatR;
using Microsoft.EntityFrameworkCore;
using TechnicalTaskAPI.Application.Identity.Models;
using TechnicalTaskAPI.Application.Identity.Services;
using TechnicalTaskAPI.ORM.Services;

namespace TechnicalTaskAPI.Application.Identity.Commands
{
    public class Refresh : IRequestHandler<Refresh.Command, AuthResponseWithTokens>
    {
        public class Command : IRequest<AuthResponseWithTokens>
        {
            public string RefreshToken { get; set; }
        }

        private readonly ITokenService _tokenService;
        private readonly ApplicationDbContext _context;

        public Refresh(ITokenService tokenService, ApplicationDbContext context)
        {
            _tokenService = tokenService;
            _context = context;
        }

        public async Task<AuthResponseWithTokens> Handle(Command request, CancellationToken cancellationToken)
        {
            if (request.RefreshToken == null) 
            {
                throw new Exception("Token is null");
            }

            var user = await _context.Users.FirstOrDefaultAsync(x => x.RefreshToken == request.RefreshToken);

            if (user == null)
            {
                throw new Exception("User is null");
            }

            var isValid = _tokenService.ValidateRefreshTokenAsync(request.RefreshToken, user);

            if (!isValid) 
            {
                await _tokenService.RevokeRefreshTokenAsync(request.RefreshToken);
                throw new Exception("Token not valid");
            }

            var accessToken = _tokenService.CreateToken(user);

            var refreshToken = await _tokenService.GenerateRefreshTokenAsync(user, cancellationToken);

            var response = new AuthResponseWithTokens 
            {
                UserId = user.Id,
                Email = user.Email,
                UserName = user.UserName,

                Token = accessToken.Token,
                RefreshToken = refreshToken.Token,

                RefreshTokenExpirationDate = refreshToken.TokenExpirationDate,
                AccessTokenExpirationDate = accessToken.TokenExpirationDate,
            };

            return response;
        }

    }
}
