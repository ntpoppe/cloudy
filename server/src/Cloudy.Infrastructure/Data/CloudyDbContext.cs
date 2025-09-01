using Cloudy.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Cloudy.Infrastructure.Data;

public class CloudyDbContext : DbContext
{
    public CloudyDbContext(DbContextOptions<CloudyDbContext> options)
        : base(options) { }

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Domain.Entities.File> Files { get; set; } = null!;
    public DbSet<Folder> Folders { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        // Apply configurations from assembly
        builder.ApplyConfigurationsFromAssembly(typeof(CloudyDbContext).Assembly);
    }
}
