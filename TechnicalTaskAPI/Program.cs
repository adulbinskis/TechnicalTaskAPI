using Microsoft.AspNetCore.Hosting;
using TechnicalTaskAPI.ORM.Entities;
using TechnicalTaskAPI;
using TechnicalTaskAPI.ORM.Services;

namespace TechnicalTaskAPI
{
    public class Program
    {
        private static async Task Main(string[] args)
        {
            var host = Host
                .CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })
                .Build();

            using var scope = host.Services.CreateScope();
            var services = scope.ServiceProvider;
            var dbInitializationService = services.GetRequiredService<IDbInitializationService>();
            await dbInitializationService.SeedAdminAsync();
            await host.RunAsync();
        }
    }
}