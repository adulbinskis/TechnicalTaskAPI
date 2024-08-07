using MediatR;
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

        private readonly ApplicationDbContext _context;
        private readonly ITokenService _tokenService;

        public Logout(ITokenService tokenService, ApplicationDbContext context)
        {
            _context = context;
            _tokenService = tokenService;

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
