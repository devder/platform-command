using PlatformService.Models;

namespace PlatformService.Data;

public interface IPlatformRepo
{
    bool SaveChanges();
    Task<List<Platform>> GetAllPlatforms();
    Task<Platform?> GetPlatformById(int id);
    Task CreatePlatform(Platform platform);
}
