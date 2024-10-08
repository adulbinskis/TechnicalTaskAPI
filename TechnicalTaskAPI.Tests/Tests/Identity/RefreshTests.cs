﻿using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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
    public class RefreshTests
    {
        private readonly IMediator _mediator;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public RefreshTests(DatabaseFixture fixture)
        {
            _mediator = fixture.ServiceProvider.GetRequiredService<IMediator>();
            _context = fixture.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            _userManager = fixture.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        }

        private async Task SeedData(string refreshToken, DateTime expiration)
        {
            var existingUser = await _userManager.FindByEmailAsync(TestUserConstants.Refresh_User_Email);
            if (existingUser == null)
            {
                var user = new ApplicationUser
                {
                    UserName = TestUserConstants.Refresh_User_Username,
                    Email = TestUserConstants.Refresh_User_Email,
                    Role = Role.User,
                    EmailConfirmed = true,
                    TwoFactorEnabled = false,
                    LockoutEnabled = false,
                    RefreshToken = refreshToken,
                    RefreshTokenExpirationDate = expiration
                };

                var result = await _userManager.CreateAsync(user, TestUserConstants.Default_Password);

                if (!result.Succeeded)
                {
                    Console.WriteLine("Failed to create user in seeding " + result.Errors);
                }
            }
            else
            {
                existingUser.RefreshToken = refreshToken;
                existingUser.RefreshTokenExpirationDate = expiration;
            }
        }

        [Fact]
        public async Task Refresh_Success()
        {
            await SeedData(TestUserConstants.ValidRefreshToken, DateTime.UtcNow.AddDays(1));

            var command = new Refresh.Command 
            { 
                RefreshToken = TestUserConstants.ValidRefreshToken 
            };

            var result = await _mediator.Send(command);

            Assert.NotNull(result);
            Assert.Equal(TestUserConstants.Refresh_User_Email, result.Email);
            Assert.Equal(TestUserConstants.Refresh_User_Username, result.UserName);
            Assert.NotNull(result.Token);
            Assert.NotNull(result.RefreshToken);

            var user = await _context.Users.SingleOrDefaultAsync(u => u.Email == TestUserConstants.Refresh_User_Email);
            Assert.NotNull(user);
            Assert.Equal(result.RefreshToken, user.RefreshToken);
            Assert.True(user.RefreshTokenExpirationDate > DateTime.UtcNow);
        }

        [Fact]
        public async Task Refresh_NullToken_ThrowsException()
        {
            var command = new Refresh.Command 
            {
                RefreshToken = null 
            };

            var exception = await Assert.ThrowsAsync<Exception>(() => _mediator.Send(command));
            Assert.Equal("Token is null", exception.Message);
        }

        [Fact]
        public async Task Refresh_UserNotFound_ThrowsException()
        {
            var command = new Refresh.Command 
            { 
                RefreshToken = TestUserConstants.NonExistentRefreshToken
            };

            var exception = await Assert.ThrowsAsync<Exception>(() => _mediator.Send(command));
            Assert.Equal("User is null", exception.Message);
        }

        [Fact]
        public async Task Refresh_InvalidToken_ThrowsException()
        {
            await SeedData(TestUserConstants.ValidRefreshToken, DateTime.UtcNow.AddDays(1));

            var command = new Refresh.Command 
            {
                RefreshToken = TestUserConstants.InvalidRefreshToken 
            };

            var exception = await Assert.ThrowsAsync<Exception>(() => _mediator.Send(command));
            Assert.Equal("User is null", exception.Message);
        }

        [Fact]
        public async Task Refresh_ExpiredToken_ThrowsException()
        {
            await SeedData(TestUserConstants.ExpiredRefreshToken, DateTime.UtcNow.AddDays(-1));

            var command = new Refresh.Command 
            { 
                RefreshToken = TestUserConstants.ExpiredRefreshToken 
            };

            var exception = await Assert.ThrowsAsync<Exception>(() => _mediator.Send(command));
            Assert.Equal("Token not valid", exception.Message);
        }

        [Fact]
        public async Task Refresh_ValidToken_RevokesOldToken()
        {
            await SeedData(TestUserConstants.ValidRefreshToken, DateTime.UtcNow.AddDays(1));

            var command = new Refresh.Command 
            { 
                RefreshToken = TestUserConstants.ValidRefreshToken 
            };

            var result = await _mediator.Send(command);

            Assert.NotNull(result);

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == TestUserConstants.Refresh_User_Email);
            Assert.NotNull(user);
            Assert.NotEqual(TestUserConstants.ValidRefreshToken, user.RefreshToken);
            Assert.Equal(result.RefreshToken, user.RefreshToken);
        }
    }
}
