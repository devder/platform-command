using CommandService.Models;
using CommandService.SyncDataServices.Grpc;

namespace CommandService.Data;

public static class PrepDb
{
    public static async Task PrepPopulation( // makes it an extension of WebApplication
        this WebApplication app
    )
    {
        using var scope = app.Services.CreateScope();
        var grpcClient = scope.ServiceProvider.GetService<IPlatformDataClient>();
        List<Platform>? platforms = null;
        if (grpcClient != null)
        {
            platforms = await grpcClient.ReturnAllPlatforms();
            await SeedData(scope.ServiceProvider.GetService<ICommandRepo>()!, platforms);
        }
    }

    private static async Task SeedData(ICommandRepo commandRepo, List<Platform> platforms)
    {
        Console.WriteLine("--> Seeding new platforms...");
        foreach (var platform in platforms)
        {
            if (!await commandRepo.PlatformExists(platform.Id))
            {
                await commandRepo.CreatePlatform(platform);
            }
            commandRepo.SaveChanges();
        }
    }
}
