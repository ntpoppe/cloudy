using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cloudy.Infrastructure.Data.Configurations;

public class FolderConfiguration : IEntityTypeConfiguration<Domain.Entities.Folder>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.Folder> builder)
    {
        builder.ToTable("folders");
        builder.HasKey(x => x.Id);
        
        // Properties
        builder.Property(x => x.Name).IsRequired().HasMaxLength(255);
        builder.Property(x => x.ParentFolderId).IsRequired(false);
        
        // Self-referencing relationship for parent folder
        builder.HasOne<Domain.Entities.Folder>()
               .WithMany()
               .HasForeignKey(x => x.ParentFolderId)
               .OnDelete(DeleteBehavior.Restrict);
        
        // Soft-delete filter
        builder.HasQueryFilter(f => !f.IsDeleted);
    }
}
