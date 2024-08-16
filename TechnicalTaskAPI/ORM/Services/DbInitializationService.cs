using TechnicalTaskAPI.ORM.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using TechnicalTaskAPI.Application.Identity.Roles;

namespace TechnicalTaskAPI.ORM.Services
{
    public interface IDbInitializationService
    {
        Task SeedAdminAsync();
    }

    public class DbInitializationService : IDbInitializationService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _dbContext;
        private readonly IConfiguration _configuration;

        public DbInitializationService(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager, IConfiguration configuration)
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _configuration = configuration;
        }

        public async Task SeedAdminAsync()
        {
            var adminUserName = _configuration["SiteSettings:AdminUserName"];
            var adminEmail = _configuration["SiteSettings:AdminEmail"];
            var adminPassword = _configuration["SiteSettings:AdminPassword"];

            var adminUser = await _userManager.FindByNameAsync(adminUserName);
            if (adminUser == null)
            {
                var admin = new ApplicationUser
                {
                    UserName = adminUserName,
                    Email = adminEmail,
                    Role = Role.Admin,
                    EmailConfirmed = true,
                    TwoFactorEnabled = false,
                    LockoutEnabled = false,
                };

                var result = await _userManager.CreateAsync(admin, adminPassword);
                if (result.Succeeded)
                {
                    await _dbContext.SaveChangesAsync();
                }
                else
                {
                    throw new Exception("Failed to create admin user: " + string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
        }
    }
}
