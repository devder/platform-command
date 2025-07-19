using PlatformService.Models;

namespace PlatformService.Data;

public static class PrepDb
{
    public static async Task MigrateDbAsync(this WebApplication app) // makes it an extension of WebApplication
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        if (!dbContext.Platforms.Any())
        {
            Console.WriteLine("--> Seeding data...");
            await dbContext.Platforms.AddRangeAsync(
                new Platform()
                {
                    Name = "Dot Net",
                    Publisher = "Microsoft",
                    Cost = "Free",
                },
                new Platform()
                {
                    Name = "SQL Server Express",
                    Publisher = "Microsoft",
                    Cost = "Free",
                },
                new Platform()
                {
                    Name = "Kubernetes",
                    Publisher = "Cloud Native Computing Foundation",
                    Cost = "Free",
                }
            );
            await dbContext.SaveChangesAsync();
            Console.WriteLine("--> Seeded data ✅");
        }
        else
        {
            Console.WriteLine("--> We already have data");
        }
    }
}
