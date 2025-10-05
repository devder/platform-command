using System.Text;
using System.Text.Json;
using PlatformService.Dtos;

namespace PlatformService.SyncDataServices.Http;

public class HttpCommandDataClient(HttpClient httpClient, IConfiguration configuration1)
    : ICommandDataClient
{
    public async Task SendPlatformToCommand(PlatformReadDto platformReadDto)
    {
        var httpContent = new StringContent(
            JsonSerializer.Serialize(platformReadDto),
            Encoding.UTF8,
            "application/json"
        );

        var res = await httpClient.PostAsync(
            $"{configuration1["CommandService"]}/api/c/platforms",
            httpContent
        );

        if (res.IsSuccessStatusCode)
        {
            Console.WriteLine("--> ✅ Sync POST to CommandService was Ok");
        }
        else
        {
            Console.WriteLine("--> ❌ Sync POST to CommandService was NOT Ok");
        }
    }
}
