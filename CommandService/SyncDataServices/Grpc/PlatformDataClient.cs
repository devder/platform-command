using AutoMapper;
using CommandService.Models;
using Grpc.Net.Client;
using PlatformService.Protos;

namespace CommandService.SyncDataServices.Grpc;

public class PlatformDataClient(IConfiguration configuration, IMapper mapper) : IPlatformDataClient
{
    private readonly string GrpcPlatformEndpoint = configuration["GrpcPlatform"]!;

    public async Task<List<Platform>> ReturnAllPlatforms()
    {
        Console.WriteLine($"--> Calling Grpc Service {GrpcPlatformEndpoint}");
        var channel = GrpcChannel.ForAddress(GrpcPlatformEndpoint);
        var client = new GrpcPlatform.GrpcPlatformClient(channel);
        var request = new GetAllRequest(); // GetAllRequest is the name in the proto file

        try
        {
            var reply = await client.GetAllPlatformsAsync(request);
            return mapper.Map<List<Platform>>(reply.Platform);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"--> Could not call gRPC server: {ex.Message}");
            return [];
        }
    }
}
