﻿using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TechnicalTaskAPI.ORM.Entities;
using TechnicalTaskAPI.ORM.Services;
using MediatR;
using TechnicalTaskAPI.Application.Identity.Commands;
using TechnicalTaskAPI.Application.Identity.Services;
using Microsoft.Extensions.Configuration;
using TechnicalTaskAPI.Application.Identity.Roles;

namespace TechnicalTaskAPI.Tests.Fixtures
{
    public class DatabaseFixture : IDisposable
    {
        public IConfiguration Configuration { get; private set; }

        public ServiceProvider ServiceProvider { get; private set; }

        public DatabaseFixture()
        {
            var serviceCollection = new ServiceCollection();

            var configurationBuilder = new ConfigurationBuilder()
               .SetBasePath(AppContext.BaseDirectory)
               .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
               .AddEnvironmentVariables();

            Configuration = configurationBuilder.Build();
            serviceCollection.AddSingleton(Configuration);

            // Token Service
            serviceCollection.AddScoped<ITokenService,TokenService>();

            // Base Entity
            serviceCollection.AddScoped<IBaseEntityService, BaseEntityService>();
   

            // Add logging
            serviceCollection.AddLogging();

            // Add DbContext
            serviceCollection.AddDbContext<ApplicationDbContext>(options =>
                options.UseInMemoryDatabase("TestDatabase"));

            // Add Identity
            serviceCollection.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            // Add AutoMapper
            serviceCollection.AddAutoMapper(typeof(Program));

            // Add MediatR Commands and Queries
            serviceCollection.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Authenticate).Assembly));
            serviceCollection.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Register).Assembly));

            ServiceProvider = serviceCollection.BuildServiceProvider();

            using (var scope = ServiceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                dbContext.Database.EnsureCreated();
                SeedData(scope.ServiceProvider).Wait();
            }
        }

        private async Task SeedData(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var dbContext = serviceProvider.GetRequiredService<ApplicationDbContext>();

            var user = new ApplicationUser 
            { 
                UserName = "testuser", 
                Email = "testuser@example.com", 
                Role = Role.User, 
                EmailConfirmed = true, 
                TwoFactorEnabled = false, 
                LockoutEnabled = false
            };
           
            var result = await userManager.CreateAsync(user, "P@ssw0rd");

            if (!result.Succeeded)
            {
                throw new Exception("Failed to create user in seeding");
            }

            await dbContext.SaveChangesAsync();
        }

        public void Dispose()
        {
            using (var scope = ServiceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                dbContext.Database.EnsureDeleted();
            }

            ServiceProvider.Dispose();
        }
    }
}
