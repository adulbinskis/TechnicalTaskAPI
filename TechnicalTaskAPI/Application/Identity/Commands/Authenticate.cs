using MediatR;
using Microsoft.AspNetCore.Identity;
using TechnicalTaskAPI.Application.Identity.Services;
using TechnicalTaskAPI.ORM.Entities;
using TechnicalTaskAPI.ORM.Services;
using TechnicalTaskAPI.Application.Identity.Models;
using FluentValidation;

namespace TechnicalTaskAPI.Application.Identity.Commands
{
    public class Authenticate : IRequestHandler<Authenticate.Command, AuthResponseWithTokens>
    {
        public class Command : IRequest<AuthResponseWithTokens>
        {
            public string Email { get; set; }
            public string Password { get; set; }
            public class Validator : AbstractValidator<Command>
            {
                public Validator()
                {
                    RuleFor(x => x.Email).NotEmpty().EmailAddress();
                    RuleFor(x => x.Password).MinimumLength(6).MaximumLength(254);
                }
            }
        }

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly ITokenService _tokenService;

        public Authenticate(UserManager<ApplicationUser> userManager, ApplicationDbContext context, ITokenService tokenService)
        {
            _userManager = userManager;
            _context = context;
            _tokenService = tokenService;
        }

        public async Task<AuthResponseWithTokens> Handle(Command request, CancellationToken cancellationToken)
        {
            var managedUser = await _userManager.FindByEmailAsync(request.Email);
            if (managedUser == null || !await _userManager.CheckPasswordAsync(managedUser, request.Password))
            {
                return null;
            }

            var userInDb = _context.Users.FirstOrDefault(u => u.Email == request.Email);
            if (userInDb == null)
            {
                return null;
            }

            var accessToken = _tokenService.CreateToken(userInDb);

            var refreshToken = await _tokenService.GenerateRefreshTokenAsync(userInDb, cancellationToken);

            await _context.SaveChangesAsync();

            return new AuthResponseWithTokens
            {
                UserId = userInDb.Id,
                Email = userInDb.Email,
                UserName = userInDb.UserName,

                Token = accessToken.Token,
                RefreshToken = refreshToken.Token,

                RefreshTokenExpirationDate = refreshToken.TokenExpirationDate,
                AccessTokenExpirationDate = accessToken.TokenExpirationDate,
            };
        }
    }
}
