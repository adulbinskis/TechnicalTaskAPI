using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using TechnicalTaskAPI.Application.Identity.Commands;
using TechnicalTaskAPI.Application.Identity.Roles;
using TechnicalTaskAPI.ORM.Entities;
using TechnicalTaskAPI.ORM.Services;
using TechnicalTaskAPI.Tests.Constants;
using TechnicalTaskAPI.Tests.Fixtures;

namespace TechnicalTaskAPI.Tests.Tests.Identity
{
    [Collection("Database collection")]
    public class AuthenticateTests
    {
        private readonly IMediator _mediator;
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;

        public AuthenticateTests(DatabaseFixture fixture)
        {
            _mediator = fixture.ServiceProvider.GetRequiredService<IMediator>();
            _dbContext = fixture.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            _userManager = fixture.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            SeedData().Wait();
        }

        private async Task SeedData()
        {
            var existingUser = await _userManager.FindByEmailAsync(TestUserConstants.Default_Email);
            if (existingUser == null)
            {
                var user = new ApplicationUser
                {
                    UserName = TestUserConstants.Default_Username,
                    Email = TestUserConstants.Default_Email,
                    Role = Role.User,
                    EmailConfirmed = true,
                    TwoFactorEnabled = false,
                    LockoutEnabled = false
                };

                var result = await _userManager.CreateAsync(user, TestUserConstants.Default_Password);

                if (!result.Succeeded)
                {
                    Console.WriteLine("Failed to create user in seeding " + result.Errors);
                }

                await _dbContext.SaveChangesAsync();
            }
            else
            {
                Console.WriteLine("User already exists, skipping seeding.");
            }
        }

        [Fact]
        public async Task Authenticate_Success()
        {
            var command = new Authenticate.Command 
            { 
                Email = TestUserConstants.Default_Email, 
                Password = TestUserConstants.Default_Password 
            };

            var result = await _mediator.Send(command);

            Assert.NotNull(result);
            Assert.Equal(TestUserConstants.Default_Email, result.Email);
            Assert.Equal(TestUserConstants.Default_Username, result.UserName);
            Assert.NotNull(result.Token);
            Assert.NotNull(result.RefreshToken);
        }

        [Fact]
        public async Task Authenticate_InvalidEmailFormat()
        {
            var command = new Authenticate.Command { 
                Email = TestUserConstants.InvalidEmailFormat, 
                Password = TestUserConstants.Default_Password 
            };

            var validator = new Authenticate.Command.Validator();

            var validationResult = await validator.ValidateAsync(command);

            Assert.False(validationResult.IsValid);
            Assert.Contains(validationResult.Errors, e => e.PropertyName == nameof(command.Email));
        }

        [Fact]
        public async Task Authenticate_IncorrectPassword()
        {
            var command = new Authenticate.Command 
            { 
                Email = TestUserConstants.Default_Email, 
                Password = TestUserConstants.IncorrectPassword 
            };

            var result = await _mediator.Send(command);

            Assert.Null(result);
        }

        [Fact]
        public async Task Authenticate_NonExistingUser()
        {
            var command = new Authenticate.Command 
            { 
                Email = TestUserConstants.NonExistingUser, 
                Password = TestUserConstants.Default_Password 
            };

            var result = await _mediator.Send(command);

            Assert.Null(result);
        }

        [Fact]
        public async Task Authenticate_MissingEmail()
        {
            var command = new Authenticate.Command 
            { 
                Password = TestUserConstants.Default_Password 
            };

            var validator = new Authenticate.Command.Validator();

            var validationResult = await validator.ValidateAsync(command);

            Assert.False(validationResult.IsValid);
            Assert.Contains(validationResult.Errors, e => e.PropertyName == nameof(command.Email));
        }

        [Fact]
        public async Task Authenticate_MissingPassword()
        {
            var command = new Authenticate.Command 
            { 
                Email = TestUserConstants.Default_Email
            };

            var validator = new Authenticate.Command.Validator();

            var validationResult = await validator.ValidateAsync(command);

            Assert.False(validationResult.IsValid);
            Assert.Contains(validationResult.Errors, e => e.PropertyName == nameof(command.Password));
        }

        [Fact]
        public async Task Authenticate_EmptyEmail()
        {
            var command = new Authenticate.Command 
            { 
                Email = TestUserConstants.EmptyString, 
                Password = TestUserConstants.Default_Password 
            };

            var validator = new Authenticate.Command.Validator();

            var validationResult = await validator.ValidateAsync(command);

            Assert.False(validationResult.IsValid);
            Assert.Contains(validationResult.Errors, e => e.PropertyName == nameof(command.Email));
        }

        [Fact]
        public async Task Authenticate_EmptyPassword()
        {
            var command = new Authenticate.Command 
            { 
                Email = TestUserConstants.Default_Email, 
                Password = TestUserConstants.EmptyString 
            };

            var validator = new Authenticate.Command.Validator();

            var validationResult = await validator.ValidateAsync(command);

            Assert.False(validationResult.IsValid);
            Assert.Contains(validationResult.Errors, e => e.PropertyName == nameof(command.Password));
        }

        [Fact]
        public async Task Authenticate_ShortPassword()
        {
            var command = new Authenticate.Command 
            { 
                Email = TestUserConstants.Default_Email, 
                Password = TestUserConstants.ShortPassword 
            };

            var validator = new Authenticate.Command.Validator();

            var validationResult = await validator.ValidateAsync(command);

            Assert.False(validationResult.IsValid);
            Assert.Contains(validationResult.Errors, e => e.PropertyName == nameof(command.Password));
        }

        [Fact]
        public async Task Authenticate_LongPassword()
        {
            var command = new Authenticate.Command 
            { 
                Email = TestUserConstants.Default_Email,
                Password = TestUserConstants.LongPassword
            };

            var validator = new Authenticate.Command.Validator();

            var validationResult = await validator.ValidateAsync(command);

            Assert.False(validationResult.IsValid);
            Assert.Contains(validationResult.Errors, e => e.PropertyName == nameof(command.Password));
        }
    }
}
