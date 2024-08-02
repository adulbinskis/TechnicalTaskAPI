using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
//using TechnicalTaskAPI.ORM.Configurations;
using TechnicalTaskAPI.ORM.Entities;

namespace TechnicalTaskAPI.ORM.Services
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }
        // dotnet ef migrations add AddedQandATables -c ApplicationDbContext -o ORM\Migrations
        // dotnet ef database update -c ApplicationDbContext

        //public DbSet<Question> Questions { get; set; }
        //public DbSet<Answer> Answers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //modelBuilder.ApplyConfiguration(new QuestionConfiguration());
            //modelBuilder.ApplyConfiguration(new AnswerConfiguration());
        }
    }
}
