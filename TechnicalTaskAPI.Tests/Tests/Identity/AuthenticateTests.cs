using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TechnicalTaskAPI.Application.Identity.Commands;
using TechnicalTaskAPI.Application.Identity.Roles;
using TechnicalTaskAPI.ORM.Entities;
using TechnicalTaskAPI.ORM.Services;
using TechnicalTaskAPI.Tests.Fixtures;
using Xunit;

namespace TechnicalTaskAPI.Tests.Tests.Identity
{
    public class AuthenticateTests : IClassFixture<DatabaseFixture>
    {
        private readonly IMediator _mediator;
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;

        public AuthenticateTests(DatabaseFixture fixture)
        {
            _mediator = fixture.ServiceProvider.GetRequiredService<IMediator>();
            _dbContext = fixture.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            _userManager = fixture.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        }

        private async Task ResetDatabase()
        {
            _dbContext.Users.RemoveRange(_dbContext.Users);
            await _dbContext.SaveChangesAsync();

            var user = new ApplicationUser
            {
                UserName = "testuser",
                Email = "testuser@example.com",
                Role = Role.User,
                EmailConfirmed = true,
                TwoFactorEnabled = false,
                LockoutEnabled = false
            };

            var result = await _userManager.CreateAsync(user, "P@ssw0rd");
            if (!result.Succeeded)
            {
                throw new Exception("Failed to create user in seeding");
            }

            await _dbContext.SaveChangesAsync();
        }

        [Fact]
        public async Task Authenticate_Success()
        {
            await ResetDatabase();
            var command = new Authenticate.Command { Email = "testuser@example.com", Password = "P@ssw0rd" };

            var result = await _mediator.Send(command);

            Assert.NotNull(result);
            Assert.Equal("testuser@example.com", result.Email);
            Assert.Equal("testuser", result.UserName);
            Assert.NotNull(result.Token);
            Assert.NotNull(result.RefreshToken);
        }

        [Fact]
        public async Task Authenticate_InvalidEmailFormat()
        {
            var command = new Authenticate.Command { Email = "invalid-email", Password = "P@ssw0rd" };
            var validator = new Authenticate.Command.Validator();

            var validationResult = await validator.ValidateAsync(command);

            Assert.False(validationResult.IsValid);
            Assert.Contains(validationResult.Errors, e => e.PropertyName == nameof(command.Email));
        }

        [Fact]
        public async Task Authenticate_IncorrectPassword()
        {
            await ResetDatabase();
            var command = new Authenticate.Command { Email = "testuser@example.com", Password = "wrongpassword" };

            var result = await _mediator.Send(command);

            Assert.Null(result);
        }

        [Fact]
        public async Task Authenticate_NonExistingUser()
        {
            await ResetDatabase();
            var command = new Authenticate.Command { Email = "nonexisting@example.com", Password = "P@ssw0rd" };

            var result = await _mediator.Send(command);

            Assert.Null(result);
        }

        [Fact]
        public async Task Authenticate_MissingEmail()
        {
            var command = new Authenticate.Command { Password = "P@ssw0rd" };
            var validator = new Authenticate.Command.Validator();

            var validationResult = await validator.ValidateAsync(command);

            Assert.False(validationResult.IsValid);
            Assert.Contains(validationResult.Errors, e => e.PropertyName == nameof(command.Email));
        }

        [Fact]
        public async Task Authenticate_MissingPassword()
        {
            var command = new Authenticate.Command { Email = "testuser@example.com" };
            var validator = new Authenticate.Command.Validator();

            var validationResult = await validator.ValidateAsync(command);

            Assert.False(validationResult.IsValid);
            Assert.Contains(validationResult.Errors, e => e.PropertyName == nameof(command.Password));
        }

        [Fact]
        public async Task Authenticate_EmptyEmail()
        {
            var command = new Authenticate.Command { Email = "", Password = "P@ssw0rd" };
            var validator = new Authenticate.Command.Validator();

            var validationResult = await validator.ValidateAsync(command);

            Assert.False(validationResult.IsValid);
            Assert.Contains(validationResult.Errors, e => e.PropertyName == nameof(command.Email));
        }

        [Fact]
        public async Task Authenticate_EmptyPassword()
        {
            var command = new Authenticate.Command { Email = "testuser@example.com", Password = "" };
            var validator = new Authenticate.Command.Validator();

            var validationResult = await validator.ValidateAsync(command);

            Assert.False(validationResult.IsValid);
            Assert.Contains(validationResult.Errors, e => e.PropertyName == nameof(command.Password));
        }

        [Fact]
        public async Task Authenticate_ShortPassword()
        {
            var command = new Authenticate.Command { Email = "testuser@example.com", Password = "short" };
            var validator = new Authenticate.Command.Validator();

            var validationResult = await validator.ValidateAsync(command);

            Assert.False(validationResult.IsValid);
            Assert.Contains(validationResult.Errors, e => e.PropertyName == nameof(command.Password));
        }

        [Fact]
        public async Task Authenticate_LongPassword()
        {
            var command = new Authenticate.Command { Email = "testuser@example.com", Password = new string('a', 255) };
            var validator = new Authenticate.Command.Validator();

            var validationResult = await validator.ValidateAsync(command);

            Assert.False(validationResult.IsValid);
            Assert.Contains(validationResult.Errors, e => e.PropertyName == nameof(command.Password));
        }
    }
}
