using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using TechnicalTaskAPI.ORM.Entities;

namespace TechnicalTaskAPI.ORM.Services
{
    public interface IBaseEntityService
    {
        Task SetCreatedPropertiesAsync(BaseEntity entity);
        Task SetUpdatedPropertiesAsync(BaseEntity entity);
    }

    public class BaseEntityService : IBaseEntityService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<ApplicationUser> _userManager;

        public BaseEntityService(IHttpContextAccessor httpContextAccessor, UserManager<ApplicationUser> userManager)
        {
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
        }

        public async Task SetCreatedPropertiesAsync(BaseEntity entity)
        {
            entity.CreatedAt = DateTimeOffset.UtcNow;
            var userId = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId != null)
            {
                entity.CreatedById = userId;
                entity.CreatedBy = await _userManager.FindByIdAsync(userId);
            }
        }

        public async Task SetUpdatedPropertiesAsync(BaseEntity entity)
        {
            entity.UpdatedAt = DateTimeOffset.UtcNow;
            var userId = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId != null)
            {
                entity.UpdatedById = userId;
                entity.UpdatedBy = await _userManager.FindByIdAsync(userId);
            }
        }
    }
}
