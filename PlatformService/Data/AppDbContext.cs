using Microsoft.EntityFrameworkCore;
using PlatformService.Models;

namespace PlatformService.Data;

public class AppDbContext(DbContextOptions<AppDbContext> dbContextOptions)
    : DbContext(dbContextOptions)
{
    public DbSet<Platform> Platforms => Set<Platform>();
}
