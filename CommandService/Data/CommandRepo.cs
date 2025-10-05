using CommandService.Models;
using Microsoft.EntityFrameworkCore;

namespace CommandService.Data;

public class CommandRepo(AppDbContext appDbContext) : ICommandRepo
{
    public async Task<Command> CreateCommand(int platformId, Command command)
    {
        ArgumentNullException.ThrowIfNull(command);
        command.PlatformId = platformId;
        await appDbContext.Commands.AddAsync(command);
        return command;
    }

    public async Task<Platform> CreatePlatform(Platform platform)
    {
        ArgumentNullException.ThrowIfNull(platform);
        await appDbContext.AddAsync(platform);
        return platform;
    }

    public Task<List<Platform>> GetAllPlatforms()
    {
        return appDbContext.Platforms.ToListAsync();
    }

    public Task<List<Command>> GetCommandsForPlatform(int platformId)
    {
        return appDbContext
            .Commands.Where(c => c.PlatformId == platformId)
            .OrderBy(c => c.Platform != null ? c.Platform.Name : string.Empty)
            .ToListAsync();
    }

    public Task<Command?> GetCommand(int platformId, int commandId)
    {
        return appDbContext
            .Commands.Where(c => c.PlatformId == platformId && c.Id == commandId)
            .FirstOrDefaultAsync();
    }

    Task<bool> ICommandRepo.PlatformExists(int platformId)
    {
        return appDbContext.Platforms.AnyAsync(p => p.Id == platformId);
    }

    public Task<bool> ExternalPlatformExists(int externalPlatformId)
    {
        return appDbContext.Platforms.AnyAsync(p => p.ExternalId == externalPlatformId);
    }

    public bool SaveChanges()
    {
        return appDbContext.SaveChanges() >= 0;
    }
}
