using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cloudy.Infrastructure.Data.Configurations;

public class FileConfiguration : IEntityTypeConfiguration<Domain.Entities.File>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.File> builder)
    {
        builder.ToTable("files");
        builder.HasKey(x => x.Id);
        
        // IBlobStorable properties
        builder.Property(x => x.Bucket).IsRequired();
        builder.Property(x => x.ObjectKey).IsRequired();
        
        // User relationship
        builder.Property(x => x.UserId).IsRequired();
        builder.HasOne(x => x.User)
               .WithMany()
               .HasForeignKey(x => x.UserId)
               .OnDelete(DeleteBehavior.Cascade);
        
        // Unique constraint on storage
        builder.HasIndex(x => new { x.Bucket, x.ObjectKey }).IsUnique();
        
        // Existing metadata configuration
        builder.OwnsOne(f => f.Metadata, m =>
        {
            m.Property(p => p.ContentType).HasColumnName("ContentType");
            m.Property(p => p.UploadedAt).HasColumnName("UploadedAt");
        });
        
        // Soft-delete filter
        builder.HasQueryFilter(f => !f.IsDeleted);
    }
}
