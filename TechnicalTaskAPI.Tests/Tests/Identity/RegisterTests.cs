using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using TechnicalTaskAPI.Application.Identity.Commands;
using TechnicalTaskAPI.Application.Identity.Roles;
using TechnicalTaskAPI.ORM.Entities;
using TechnicalTaskAPI.Tests.Constants;
using TechnicalTaskAPI.Tests.Fixtures;

namespace TechnicalTaskAPI.Tests.Tests.Identity
{
    [Collection("Database collection")]
    public class RegisterTests
    {
        private readonly IMediator _mediator;
        private readonly UserManager<ApplicationUser> _userManager;

        public RegisterTests(DatabaseFixture fixture)
        {
            _mediator = fixture.ServiceProvider.GetRequiredService<IMediator>();
            _userManager = fixture.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        }

        [Fact]
        public async Task Register_Success()
        {
            var command = new Register.Command
            {
                Username = TestUserConstants.New_User_Username,
                Email = TestUserConstants.New_User_Email,
                Password = TestUserConstants.Default_Password
            };

            var result = await _mediator.Send(command);

            Assert.NotNull(result);
            Assert.Equal(TestUserConstants.New_User_Username, result.Username);
            Assert.Equal(TestUserConstants.New_User_Email, result.Email);
            Assert.Equal(Role.User, result.Role);

            var user = await _userManager.FindByEmailAsync(result.Email);
            Assert.NotNull(user);
            Assert.Equal(result.Username, user.UserName);
            Assert.Equal(result.Email, user.Email);
            Assert.Equal(Role.User, user.Role);
        }

        [Fact]
        public async Task Register_InvalidEmailFormat()
        {
            var command = new Register.Command
            {
                Username = TestUserConstants.New_User_Username,
                Email = TestUserConstants.InvalidEmailFormat,
                Password = TestUserConstants.Default_Password
            };

            var validator = new Register.Command.Validator();

            var validationResult = await validator.ValidateAsync(command);

            Assert.False(validationResult.IsValid);
            Assert.Contains(validationResult.Errors, e => e.PropertyName == nameof(command.Email));
        }

        [Fact]
        public async Task Register_ShortPassword()
        {
            var command = new Register.Command
            {
                Username = TestUserConstants.New_User_Username,
                Email = TestUserConstants.New_User_Email,
                Password = TestUserConstants.ShortPassword
            };

            var validator = new Register.Command.Validator();

            var validationResult = await validator.ValidateAsync(command);

            Assert.False(validationResult.IsValid);
            Assert.Contains(validationResult.Errors, e => e.PropertyName == nameof(command.Password));
        }

        [Fact]
        public async Task Register_LongPassword()
        {
            var command = new Register.Command
            {
                Username = TestUserConstants.New_User_Username,
                Email = TestUserConstants.New_User_Email,
                Password = TestUserConstants.LongPassword
            };

            var validator = new Register.Command.Validator();

            var validationResult = await validator.ValidateAsync(command);

            Assert.False(validationResult.IsValid);
            Assert.Contains(validationResult.Errors, e => e.PropertyName == nameof(command.Password));
        }

        [Fact]
        public async Task Register_MissingEmail()
        {
            var command = new Register.Command
            {
                Username = TestUserConstants.New_User_Username,
                Password = TestUserConstants.Default_Password
            };

            var validator = new Register.Command.Validator();

            var validationResult = await validator.ValidateAsync(command);

            Assert.False(validationResult.IsValid);
            Assert.Contains(validationResult.Errors, e => e.PropertyName == nameof(command.Email));
        }

        [Fact]
        public async Task Register_MissingPassword()
        {
            var command = new Register.Command
            {
                Username = TestUserConstants.New_User_Username,
                Email = TestUserConstants.New_User_Email,
            };

            var validator = new Register.Command.Validator();

            var validationResult = await validator.ValidateAsync(command);

            Assert.False(validationResult.IsValid);
            Assert.Contains(validationResult.Errors, e => e.PropertyName == nameof(command.Password));
        }

        [Fact]
        public async Task Register_MissingUsername()
        {
            var command = new Register.Command
            {
                Email = TestUserConstants.New_User_Email,
                Password = TestUserConstants.Default_Password
            };

            var validator = new Register.Command.Validator();

            var validationResult = await validator.ValidateAsync(command);

            Assert.False(validationResult.IsValid);
            Assert.Contains(validationResult.Errors, e => e.PropertyName == nameof(command.Username));
        }

        [Fact]
        public async Task Register_EmptyEmail()
        {
            var command = new Register.Command
            {
                Username = TestUserConstants.New_User_Username,
                Email = TestUserConstants.EmptyString,
                Password = TestUserConstants.Default_Password
            };

            var validator = new Register.Command.Validator();

            var validationResult = await validator.ValidateAsync(command);

            Assert.False(validationResult.IsValid);
            Assert.Contains(validationResult.Errors, e => e.PropertyName == nameof(command.Email));
        }

        [Fact]
        public async Task Register_EmptyPassword()
        {
            var command = new Register.Command
            {
                Username = TestUserConstants.New_User_Username,
                Email = TestUserConstants.New_User_Email,
                Password = TestUserConstants.EmptyString
            };

            var validator = new Register.Command.Validator();

            var validationResult = await validator.ValidateAsync(command);

            Assert.False(validationResult.IsValid);
            Assert.Contains(validationResult.Errors, e => e.PropertyName == nameof(command.Password));
        }

        [Fact]
        public async Task Register_EmptyUsername()
        {
            var command = new Register.Command
            {
                Username = TestUserConstants.EmptyString,
                Email = TestUserConstants.New_User_Email,
                Password = TestUserConstants.Default_Password
            };

            var validator = new Register.Command.Validator();

            var validationResult = await validator.ValidateAsync(command);

            Assert.False(validationResult.IsValid);
            Assert.Contains(validationResult.Errors, e => e.PropertyName == nameof(command.Username));
        }
    }
}
