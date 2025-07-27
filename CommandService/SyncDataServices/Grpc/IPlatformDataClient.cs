using CommandService.Models;

namespace CommandService.SyncDataServices.Grpc;

public interface IPlatformDataClient
{
    Task<List<Platform>> ReturnAllPlatforms();
}
