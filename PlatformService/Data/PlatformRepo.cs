using Microsoft.EntityFrameworkCore;
using PlatformService.Models;

namespace PlatformService.Data;

public class PlatformRepo(AppDbContext context) : IPlatformRepo
{
    readonly AppDbContext _context = context;

    public async Task CreatePlatform(Platform platform)
    {
        ArgumentNullException.ThrowIfNull(platform);

        await _context.Platforms.AddAsync(platform);
    }

    public async Task<List<Platform>> GetAllPlatforms()
    {
        return await _context.Platforms.ToListAsync();
    }

    public async Task<Platform?> GetPlatformById(int id)
    {
        return await _context.Platforms.FirstOrDefaultAsync(p => p.Id == id);
    }

    public bool SaveChanges()
    {
        return _context.SaveChanges() >= 0;
    }
}
