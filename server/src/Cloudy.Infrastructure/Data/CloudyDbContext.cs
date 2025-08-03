using Cloudy.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Cloudy.Infrastructure.Data;

public class CloudyDbContext : DbContext
{
    public CloudyDbContext(DbContextOptions<CloudyDbContext> options)
        : base(options) { }

    public DbSet<Domain.Entities.File> Files { get; set; } = null!;
    public DbSet<Folder> Folders { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        // Map File.Metadata as an owned/value object
        builder.Entity<Domain.Entities.File>().OwnsOne(f => f.Metadata, m =>
        {
            m.Property(p => p.ContentType).HasColumnName("ContentType");
            m.Property(p => p.UploadedAt).HasColumnName("UploadedAt");
        });

        // Soft-delete filters (exclude IsDeleted = true)
        builder.Entity<Domain.Entities.File>()
               .HasQueryFilter(f => !f.IsDeleted);
        builder.Entity<Folder>()
               .HasQueryFilter(f => !f.IsDeleted);
    }
}
