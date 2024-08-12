﻿using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TechnicalTaskAPI.Application.Identity.Commands;
using TechnicalTaskAPI.Application.Identity.Services;
using TechnicalTaskAPI.ORM.Entities;
using TechnicalTaskAPI.ORM.Services;
using TechnicalTaskAPI.Tests.Constants;
using TechnicalTaskAPI.Tests.Fixtures;

namespace TechnicalTaskAPI.Tests.Tests.Identity
{
    [Collection("Database collection")]
    public class LogoutTests
    {
        private readonly IMediator _mediator;
        private readonly ApplicationDbContext _context;
        private readonly ITokenService _tokenService;

        public LogoutTests(DatabaseFixture fixture)
        {
            _mediator = fixture.ServiceProvider.GetRequiredService<IMediator>();
            _context = fixture.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            _tokenService = fixture.ServiceProvider.GetRequiredService<ITokenService>();
        }

        private async Task SeedUserWithRefreshToken(string refreshToken, DateTime expirationDate)
        {
            var user = new ApplicationUser
            {
                UserName = TestUserConstants.Logout_User_Username,
                Email = TestUserConstants.Logout_User_Email,
                RefreshToken = refreshToken,
                RefreshTokenExpirationDate = expirationDate
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }

        [Fact]
        public async Task Logout_Success()
        {
            await SeedUserWithRefreshToken(TestUserConstants.LogoutValidRefreshToken, DateTime.UtcNow.AddDays(1));

            var command = new Logout.Command 
            {
                RefreshToken = TestUserConstants.LogoutValidRefreshToken 
            };

            var result = await _mediator.Send(command);

            Assert.True(result);

            var user = await _context.Users.SingleOrDefaultAsync(u => u.Email == TestUserConstants.Logout_User_Email);
            Assert.NotNull(user);
            Assert.Null(user.RefreshToken);
            Assert.Null(user.RefreshTokenExpirationDate);
        }

        [Fact]
        public async Task Logout_NullToken_ThrowsException()
        {
            var command = new Logout.Command 
            { 
                RefreshToken = null 
            };

            var exception = await Assert.ThrowsAsync<Exception>(() => _mediator.Send(command));
            Assert.Equal("Token is null", exception.Message);
        }

        [Fact]
        public async Task Logout_UserNotFound_ThrowsException()
        {
            var command = new Logout.Command 
            { 
                RefreshToken = TestUserConstants.NonExistentRefreshToken 
            };

            var exception = await Assert.ThrowsAsync<Exception>(() => _mediator.Send(command));
            Assert.Equal("User dont have token", exception.Message);
        }

        [Fact]
        public async Task Logout_ValidToken_RevokesToken()
        {
            await SeedUserWithRefreshToken(TestUserConstants.LogoutValidRefreshToken, DateTime.UtcNow.AddDays(1));

            var command = new Logout.Command 
            {
                RefreshToken = TestUserConstants.LogoutValidRefreshToken 
            };

            var result = await _mediator.Send(command);

            Assert.True(result);

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == TestUserConstants.Logout_User_Email);
            Assert.NotNull(user);
            Assert.Null(user.RefreshToken);
            Assert.Null(user.RefreshTokenExpirationDate);
        }
    }
}

