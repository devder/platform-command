using CommandService.Models;

namespace CommandService.Data;

public interface ICommandRepo
{
    bool SaveChanges();

    // Platforms
    Task<List<Platform>> GetAllPlatforms();
    Task<Platform> CreatePlatform(Platform platform);
    Task<bool> PlatformExists(int platformId);

    // Commands
    Task<List<Command>> GetCommandsForPlatform(int platformId);
    Task<Command?> GetCommand(int platformId, int commandId);
    Task<Command> CreateCommand(int platformId, Command command);
}
