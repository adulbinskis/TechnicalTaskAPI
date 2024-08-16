using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TechnicalTaskAPI.ORM.Entities;
using TechnicalTaskAPI.ORM.Services;
using MediatR;
using TechnicalTaskAPI.Application.Identity.Commands;
using TechnicalTaskAPI.Application.Identity.Services;
using Microsoft.Extensions.Configuration;
using TechnicalTaskAPI.Application.Services.Interfaces;
using TechnicalTaskAPI.Application.Services;

namespace TechnicalTaskAPI.Tests.Fixtures
{
    public class DatabaseFixture : IDisposable
    {
        public IConfiguration Configuration { get; private set; }
        public ServiceProvider ServiceProvider { get; private set; }

        private readonly string _connectionString = "Data Source=localhost;Initial Catalog=TehnicalTaskDbTests;Integrated Security=True;Trust Server Certificate=True";

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

            // DateTimeService
            serviceCollection.AddScoped<IDateTimeService, DateTimeService>();

            // Add logging
            serviceCollection.AddLogging();

            // Add DbContext
            serviceCollection.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(_connectionString));

            // Add Identity
            serviceCollection.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            // Add AutoMapper
            serviceCollection.AddAutoMapper(typeof(Program));

            // Add MediatR Commands and Queries
            serviceCollection.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Authenticate).Assembly));
            serviceCollection.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Register).Assembly));
            serviceCollection.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Product).Assembly));

            ServiceProvider = serviceCollection.BuildServiceProvider();

            using (var scope = ServiceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                dbContext.Database.EnsureDeleted();
                dbContext.Database.EnsureCreated();
            }
        }

        public void Dispose()
        {
            using (var scope = ServiceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                ClearDatabase(dbContext);
            }

            ServiceProvider.Dispose();
        }

        private void ClearDatabase(ApplicationDbContext dbContext)
        {
            var tableNames = dbContext.Model.GetEntityTypes()
                .Select(t => t.GetTableName())
                .Distinct()
                .ToList();

            foreach (var tableName in tableNames)
            {
                dbContext.Database.ExecuteSqlRaw($"ALTER TABLE [{tableName}] NOCHECK CONSTRAINT ALL");

                dbContext.Database.ExecuteSqlRaw($"DELETE FROM [{tableName}]");

                dbContext.Database.ExecuteSqlRaw($"ALTER TABLE [{tableName}] CHECK CONSTRAINT ALL");
            }

            dbContext.SaveChanges();
        }
    }
}
