using ESSPortal.Domain.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ESSPortal.Persistence.SQLServer.EntityConfigurations;
internal sealed class BlockedIpConfiguration : IEntityTypeConfiguration<BlockedIp>
{
    public void Configure(EntityTypeBuilder<BlockedIp> builder)
    {
        // Table configuration
        builder.ToTable("BlockedIps");

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

        builder.Property(e => e.BlockedBy)
               .HasMaxLength(450); // Reference to AppUser.Id

        builder.Property(e => e.IsActive)
               .HasDefaultValue(true);

        builder.Property(e => e.BlockedAt)
               .IsRequired();

        builder.Property(e => e.ExpiresAt)
               .IsRequired(false);

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
               .IsUnique()
               .HasDatabaseName("IX_BlockedIps_IpAddress");

        builder.HasIndex(e => e.IsActive)
               .HasDatabaseName("IX_BlockedIps_IsActive");

        builder.HasIndex(e => e.ExpiresAt)
               .HasDatabaseName("IX_BlockedIps_ExpiresAt");

        builder.HasIndex(e => e.BlockedAt)
               .HasDatabaseName("IX_BlockedIps_BlockedAt");

        builder.HasIndex(e => new { e.IsActive, e.ExpiresAt })
               .HasDatabaseName("IX_BlockedIps_IsActive_ExpiresAt");

        // Global query filter for soft delete
        builder.HasQueryFilter(e => !e.IsDeleted);
    }
}
