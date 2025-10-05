using AutoMapper;
using Grpc.Core;
using PlatformService.Data;
using PlatformService.Protos;

namespace PlatformService.SyncDataServices.Grpc;

public class GrpcPlatformService(IPlatformRepo platformRepo, IMapper mapper)
    : GrpcPlatform.GrpcPlatformBase // inherited from the generated code
{
    public override async Task<PlatformResponse> GetAllPlatforms(
        GetAllRequest request,
        ServerCallContext callContext
    )
    {
        var response = new PlatformResponse();
        var platforms = await platformRepo.GetAllPlatforms();
        foreach (var platform in platforms)
        {
            response.Platform.Add(mapper.Map<GrpcPlatformModel>(platform));
        }
        return response;
    }
}
