using System;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TechnicalTaskAPI.Application.Identity.Commands;
using TechnicalTaskAPI.Application.Identity.Models;
using TechnicalTaskAPI.Application.Identity.Roles;
using TechnicalTaskAPI.Application.Identity.Services;
using TechnicalTaskAPI.ORM.Entities;
using TechnicalTaskAPI.ORM.Services;
using TechnicalTaskAPI.Tests.Fixtures;
using Xunit;

namespace TechnicalTaskAPI.Tests.Tests.Identity
{
    public class RefreshTests : IClassFixture<DatabaseFixture>
    {
        private readonly IMediator _mediator;
        private readonly ApplicationDbContext _context;

        public RefreshTests(DatabaseFixture fixture)
        {
            _mediator = fixture.ServiceProvider.GetRequiredService<IMediator>();
            _context = fixture.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        }

        private async Task SeedUserWithRefreshToken(string refreshToken, DateTime expirationDate)
        {
            var user = new ApplicationUser
            {
                UserName = "testuserrefresh",
                Email = "testuserrefresh@example.com",
                RefreshToken = refreshToken,
                RefreshTokenExpirationDate = expirationDate,
                Role = Role.User
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }

        [Fact]
        public async Task Refresh_Success()
        {
            // Arrange
            await SeedUserWithRefreshToken("valid_refresh_token", DateTime.UtcNow.AddDays(1));
            var command = new Refresh.Command { RefreshToken = "valid_refresh_token" };

            // Act
            var result = await _mediator.Send(command);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("testuserrefresh@example.com", result.Email);
            Assert.Equal("testuserrefresh", result.UserName);
            Assert.NotNull(result.Token);
            Assert.NotNull(result.RefreshToken);

            // Verify database changes
            var user = await _context.Users.SingleOrDefaultAsync(u => u.Email == "testuserrefresh@example.com");
            Assert.NotNull(user);
            Assert.Equal(result.RefreshToken, user.RefreshToken);
            Assert.True(user.RefreshTokenExpirationDate > DateTime.UtcNow);
        }

        [Fact]
        public async Task Refresh_NullToken_ThrowsException()
        {
            // Arrange
            var command = new Refresh.Command { RefreshToken = null };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _mediator.Send(command));
            Assert.Equal("Token is null", exception.Message);
        }

        [Fact]
        public async Task Refresh_UserNotFound_ThrowsException()
        {
            // Arrange
            var command = new Refresh.Command { RefreshToken = "non_existent_refresh_token" };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _mediator.Send(command));
            Assert.Equal("User is null", exception.Message);
        }

        [Fact]
        public async Task Refresh_InvalidToken_ThrowsException()
        {
            // Arrange
            await SeedUserWithRefreshToken("valid_refresh_token", DateTime.UtcNow.AddDays(1));
            var command = new Refresh.Command { RefreshToken = "invalid_refresh_token" };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _mediator.Send(command));
            Assert.Equal("User is null", exception.Message);
        }

        [Fact]
        public async Task Refresh_ExpiredToken_ThrowsException()
        {
            // Arrange
            await SeedUserWithRefreshToken("expired_refresh_token", DateTime.UtcNow.AddDays(-1));
            var command = new Refresh.Command { RefreshToken = "expired_refresh_token" };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _mediator.Send(command));
            Assert.Equal("Token not valid", exception.Message);
        }

        [Fact]
        public async Task Refresh_ValidToken_RevokesOldToken()
        {
            // Arrange
            await SeedUserWithRefreshToken("valid_refresh_token", DateTime.UtcNow.AddDays(1));
            var command = new Refresh.Command { RefreshToken = "valid_refresh_token" };

            // Act
            var result = await _mediator.Send(command);

            // Assert
            Assert.NotNull(result);

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == "testuserrefresh@example.com");
            Assert.NotNull(user);
            Assert.NotEqual("valid_refresh_token", user.RefreshToken);
            Assert.Equal(result.RefreshToken, user.RefreshToken);
        }
    }
}
