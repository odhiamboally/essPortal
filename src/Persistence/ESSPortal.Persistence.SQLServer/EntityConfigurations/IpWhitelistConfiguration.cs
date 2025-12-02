using ESSPortal.Domain.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ESSPortal.Persistence.SQLServer.EntityConfigurations;
internal sealed class IpWhitelistConfiguration : IEntityTypeConfiguration<IpWhitelist>
{
    public void Configure(EntityTypeBuilder<IpWhitelist> builder)
    {
        // Table configuration
        builder.ToTable("IpWhitelists");

        // Primary key
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
               .ValueGeneratedNever() // Since BaseEntity generates Guid
               .HasMaxLength(36);

        // Property configurations
        builder.Property(e => e.IpAddress)
               .HasMaxLength(50)
               .IsRequired();

        builder.Property(e => e.Reason)
               .HasMaxLength(500)
               .IsRequired();

        builder.Property(e => e.AddedBy)
               .HasMaxLength(450); // Reference to AppUser.Id

        builder.Property(e => e.IsActive)
               .HasDefaultValue(true);

        builder.Property(e => e.IsAdminWhitelist)
               .HasDefaultValue(false);

        // Base Entity properties
        builder.Property(e => e.CreatedAt)
               .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(e => e.CreatedBy)
               .HasMaxLength(450);

        builder.Property(e => e.UpdatedBy)
               .HasMaxLength(450);

        builder.Property(e => e.DeletedBy)
               .HasMaxLength(450);

        builder.Property(e => e.IsDeleted)
               .HasDefaultValue(false);

        builder.Property(e => e.RowVersion)
               .IsRowVersion();

        // Indexes
        builder.HasIndex(e => e.IpAddress)
               .HasDatabaseName("IX_IpWhitelists_IpAddress");

        builder.HasIndex(e => e.IsActive)
               .HasDatabaseName("IX_IpWhitelists_IsActive");

        builder.HasIndex(e => e.IsAdminWhitelist)
               .HasDatabaseName("IX_IpWhitelists_IsAdminWhitelist");

        builder.HasIndex(e => new { e.IsActive, e.IsAdminWhitelist })
               .HasDatabaseName("IX_IpWhitelists_IsActive_IsAdminWhitelist");

        // Global query filter for soft delete
        builder.HasQueryFilter(e => !e.IsDeleted);
    }
}
