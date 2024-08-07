using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using TechnicalTaskAPI.Application.Identity.Commands;
using TechnicalTaskAPI.Application.Identity.Models;
using TechnicalTaskAPI.Application.Identity.Roles;
using TechnicalTaskAPI.ORM.Entities;
using TechnicalTaskAPI.Tests.Fixtures;
using Xunit;

namespace TechnicalTaskAPI.Tests.Tests.Identity
{
    public class RegisterTests : IClassFixture<DatabaseFixture>
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
            var command = new Register.Command { Username = "newuser", Email = "newuser@example.com", Password = "P@ssw0rd1" };

            var result = await _mediator.Send(command);

            Assert.NotNull(result);
            Assert.Equal("newuser", result.Username);
            Assert.Equal("newuser@example.com", result.Email);
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
            var command = new Register.Command { Username = "newuser", Email = "invalid-email", Password = "P@ssw0rd1" };
            var validator = new Register.Command.Validator();

            var validationResult = await validator.ValidateAsync(command);

            Assert.False(validationResult.IsValid);
            Assert.Contains(validationResult.Errors, e => e.PropertyName == nameof(command.Email));
        }

        [Fact]
        public async Task Register_ShortPassword()
        { 
            var command = new Register.Command { Username = "newuser", Email = "newuser@example.com", Password = "short" };
            var validator = new Register.Command.Validator();

            var validationResult = await validator.ValidateAsync(command);

            Assert.False(validationResult.IsValid);
            Assert.Contains(validationResult.Errors, e => e.PropertyName == nameof(command.Password));
        }

        [Fact]
        public async Task Register_LongPassword()
        { 
            var command = new Register.Command { Username = "newuser", Email = "newuser@example.com", Password = new string('a', 255) };
            var validator = new Register.Command.Validator();

            var validationResult = await validator.ValidateAsync(command);

            Assert.False(validationResult.IsValid);
            Assert.Contains(validationResult.Errors, e => e.PropertyName == nameof(command.Password));
        }

        [Fact]
        public async Task Register_MissingEmail()
        { 
            var command = new Register.Command { Username = "newuser", Password = "P@ssw0rd1" };
            var validator = new Register.Command.Validator();

            var validationResult = await validator.ValidateAsync(command);

            Assert.False(validationResult.IsValid);
            Assert.Contains(validationResult.Errors, e => e.PropertyName == nameof(command.Email));
        }

        [Fact]
        public async Task Register_MissingPassword()
        { 
            var command = new Register.Command { Username = "newuser", Email = "newuser@example.com" };
            var validator = new Register.Command.Validator();

            var validationResult = await validator.ValidateAsync(command);

            Assert.False(validationResult.IsValid);
            Assert.Contains(validationResult.Errors, e => e.PropertyName == nameof(command.Password));
        }

        [Fact]
        public async Task Register_MissingUsername()
        { 
            var command = new Register.Command { Email = "newuser@example.com", Password = "P@ssw0rd1" };
            var validator = new Register.Command.Validator();

            var validationResult = await validator.ValidateAsync(command);

            Assert.False(validationResult.IsValid);
            Assert.Contains(validationResult.Errors, e => e.PropertyName == nameof(command.Username));
        }

        [Fact]
        public async Task Register_EmptyEmail()
        { 
            var command = new Register.Command { Username = "newuser", Email = "", Password = "P@ssw0rd1" };
            var validator = new Register.Command.Validator();

            var validationResult = await validator.ValidateAsync(command);

            Assert.False(validationResult.IsValid);
            Assert.Contains(validationResult.Errors, e => e.PropertyName == nameof(command.Email));
        }

        [Fact]
        public async Task Register_EmptyPassword()
        { 
            var command = new Register.Command { Username = "newuser", Email = "newuser@example.com", Password = "" };
            var validator = new Register.Command.Validator();

            var validationResult = await validator.ValidateAsync(command);

            Assert.False(validationResult.IsValid);
            Assert.Contains(validationResult.Errors, e => e.PropertyName == nameof(command.Password));
        }

        [Fact]
        public async Task Register_EmptyUsername()
        { 
            var command = new Register.Command { Username = "", Email = "newuser@example.com", Password = "P@ssw0rd1" };
            var validator = new Register.Command.Validator();

            var validationResult = await validator.ValidateAsync(command);

            Assert.False(validationResult.IsValid);
            Assert.Contains(validationResult.Errors, e => e.PropertyName == nameof(command.Username));
        }
    }
}
