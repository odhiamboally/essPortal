using ESSPortal.Domain.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ESSPortal.Persistence.SQLServer.EntityConfigurations;
internal sealed class UploadConfiguration : IEntityTypeConfiguration<Upload>
{
    public void Configure(EntityTypeBuilder<Upload> builder)
    {
        builder.ToTable("Uploads");

        builder.HasKey(e => e.Id);

        // Add proper constraints and indexes
        builder.HasOne(e => e.Profile)  
           .WithMany(p => p.Uploads)
           .HasForeignKey(e => e.ProfileId)
           .OnDelete(DeleteBehavior.Cascade)
           .HasConstraintName("FK_Uploads_Profiles");

        builder.HasIndex(e => e.ProfileId)
               .HasDatabaseName("IX_Uploads_ProfileId");

        // Property configurations
        builder.Property(e => e.ProfileId)
               .HasMaxLength(450)  // Changed to match Profile.Id length
               .IsRequired();

        builder.Property(e => e.Path)
               .HasMaxLength(1000)
               .IsRequired();

        builder.Property(e => e.FileName)
               .HasMaxLength(255)
               .IsRequired();

        builder.Property(e => e.FileType)
               .HasMaxLength(50)
               .IsRequired();

        builder.Property(e => e.UploadName)
               .HasMaxLength(255)
               .IsRequired(false);

        // Add CreatedAt to the entity, don't use shadow properties
        builder.Property(e => e.CreatedAt)
               .HasDefaultValueSql("GETUTCDATE()");

    }
}
