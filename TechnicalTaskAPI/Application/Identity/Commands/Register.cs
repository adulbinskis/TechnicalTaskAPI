using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TechnicalTaskAPI.Application.Identity.Models;
using TechnicalTaskAPI.Application.Identity.Roles;
using TechnicalTaskAPI.ORM.Entities;
using TechnicalTaskAPI.ORM.Services;

namespace TechnicalTaskAPI.Application.Identity.Commands
{
    public class Register : IRequestHandler<Register.Command, RegistrationResponse>
    {
        public class Command : IRequest<RegistrationResponse>
        {
            public string Username { get; set; }
            public string Email { get; set; }
            public string Password { get; set; }

            public class Validator : AbstractValidator<Command>
            {
                public Validator()
                {
                    RuleFor(x => x.Email).NotEmpty().EmailAddress();
                    RuleFor(x => x.Password).NotEmpty().MinimumLength(6).MaximumLength(254);
                    RuleFor(x => x.Username).NotEmpty().MinimumLength(3).MaximumLength(254);
                }
            }
        }

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public Register(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<RegistrationResponse> Handle(Command request, CancellationToken cancellationToken)
        {
            var result = await _userManager.CreateAsync(
                new ApplicationUser { 
                    UserName = request.Username, 
                    Email = request.Email, 
                    Role = Role.User,
                    EmailConfirmed = true,
                    TwoFactorEnabled = false,
                    LockoutEnabled = false,
                },
                request.Password
            );

            if (result.Succeeded)
            {
                await _context.SaveChangesAsync(cancellationToken);

                return new RegistrationResponse
                {
                    Username = request.Username,
                    Email = request.Email,
                    Role = Role.User
                };
            }

            return null;
        }
    }
}
