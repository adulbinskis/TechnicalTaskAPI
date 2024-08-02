using MediatR;
using Microsoft.EntityFrameworkCore;
using TechnicalTaskAPI.Application.Identity.Models;
using TechnicalTaskAPI.Application.Identity.Services;
using TechnicalTaskAPI.ORM.Services;

namespace TechnicalTaskAPI.Application.Identity.Commands
{
    public class Logout : IRequestHandler<Logout.Command, bool>
    {
        public class Command : IRequest<bool>
        {
            public string RefreshToken { get; set; }
        }

        private readonly TokenService _tokenService;
        private readonly ApplicationDbContext _context;

        public Logout(TokenService tokenService, ApplicationDbContext context)
        {
            _tokenService = tokenService;
            _context = context;
        }

        public async Task<bool> Handle(Command request, CancellationToken cancellationToken)
        {
            if (request.RefreshToken == null) 
            {
                throw new Exception("Token is null");
            }

            await _tokenService.RevokeRefreshTokenAsync(request.RefreshToken);

            return true;
        }

    }
}
