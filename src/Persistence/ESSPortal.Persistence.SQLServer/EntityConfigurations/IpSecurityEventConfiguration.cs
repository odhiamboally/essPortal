using ESSPortal.Domain.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ESSPortal.Persistence.SQLServer.EntityConfigurations;
internal sealed class IpSecurityEventConfiguration : IEntityTypeConfiguration<IpSecurityEvent>
{
    public void Configure(EntityTypeBuilder<IpSecurityEvent> builder)
    {
        // Table configuration
        builder.ToTable("IpSecurityEvents");

        // Primary key
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
               .ValueGeneratedNever() // Since BaseEntity generates Guid
               .HasMaxLength(36);

        // Property configurations
        builder.Property(e => e.IpAddress)
               .HasMaxLength(50)
               .IsRequired();

        builder.Property(e => e.Operation)
               .HasMaxLength(100)
               .IsRequired();

        builder.Property(e => e.Result)
               .HasMaxLength(100)
               .IsRequired();

        builder.Property(e => e.UserId)
               .HasMaxLength(450); // Reference to AppUser.Id

        builder.Property(e => e.UserAgent)
               .HasMaxLength(500);

        builder.Property(e => e.Country)
               .HasMaxLength(100);

        builder.Property(e => e.City)
               .HasMaxLength(100);

        builder.Property(e => e.Timestamp)
               .IsRequired();

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

        // Indexes for performance
        builder.HasIndex(e => e.IpAddress)
               .HasDatabaseName("IX_IpSecurityEvents_IpAddress");

        builder.HasIndex(e => e.Timestamp)
               .HasDatabaseName("IX_IpSecurityEvents_Timestamp");

        builder.HasIndex(e => e.Operation)
               .HasDatabaseName("IX_IpSecurityEvents_Operation");

        builder.HasIndex(e => new { e.IpAddress, e.Timestamp })
               .HasDatabaseName("IX_IpSecurityEvents_IpAddress_Timestamp");

        builder.HasIndex(e => e.UserId)
               .HasDatabaseName("IX_IpSecurityEvents_UserId");

        builder.HasIndex(e => new { e.Operation, e.Result })
               .HasDatabaseName("IX_IpSecurityEvents_Operation_Result");

        builder.HasIndex(e => new { e.UserId, e.Timestamp })
               .HasDatabaseName("IX_IpSecurityEvents_UserId_Timestamp");

        // Global query filter for soft delete
        builder.HasQueryFilter(e => !e.IsDeleted);

        // Value conversions for DateTimeOffset properties
        builder.Property(e => e.CreatedAt)
               .HasConversion(
                   v => v.UtcDateTime,
                   v => new DateTimeOffset(v, TimeSpan.Zero));

        builder.Property(e => e.UpdatedAt)
               .HasConversion(
                   v => v.UtcDateTime,
                   v => new DateTimeOffset(v, TimeSpan.Zero));

        builder.Property(e => e.Timestamp)
               .HasConversion(
                   v => v.UtcDateTime,
                   v => new DateTimeOffset(v, TimeSpan.Zero));

        builder.Property(e => e.DeletedAt)
               .HasConversion(
                   v => v.HasValue ? v.Value.UtcDateTime : (DateTime?)null,
                   v => v.HasValue ? new DateTimeOffset(v.Value, TimeSpan.Zero) : (DateTimeOffset?)null);
    }
}