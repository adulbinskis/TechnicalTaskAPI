using Microsoft.AspNetCore.Identity;
using TechnicalTaskAPI.Application.Identity.Roles;

namespace TechnicalTaskAPI.ORM.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public Role Role { get; set; }
        //public List<Question> Questions { get; set; }
        //public List<Answer> Answers { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpirationDate { get; set; }
    }
}
