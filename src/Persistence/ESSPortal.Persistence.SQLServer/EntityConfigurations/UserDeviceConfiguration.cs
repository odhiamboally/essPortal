using ESSPortal.Domain.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ESSPortal.Persistence.SQLServer.EntityConfigurations;
internal sealed class UserDeviceConfiguration : IEntityTypeConfiguration<UserDevice>
{
    public void Configure(EntityTypeBuilder<UserDevice> builder)
    {
        // Table configuration
        builder.ToTable("UserDevices", t =>
        {
            // Check constraints
            t.HasCheckConstraint("CK_UserDevices_TrustedUntil",
                "[TrustedUntil] IS NULL OR [TrustedUntil] > [CreatedAt]");

            t.HasCheckConstraint("CK_UserDevices_LastUsedAt",
                "[LastUsedAt] IS NULL OR [LastUsedAt] >= [CreatedAt]");
        });

        // Primary key
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
               .ValueGeneratedOnAdd();

        // Indexes
        builder.HasIndex(e => new { e.UserId, e.DeviceFingerprint })
               .IsUnique()
               .HasDatabaseName("IX_UserDevices_UserId_DeviceFingerprint");

        builder.HasIndex(e => e.UserId)
               .HasDatabaseName("IX_UserDevices_UserId");

        builder.HasIndex(e => e.IsTrusted)
               .HasDatabaseName("IX_UserDevices_IsTrusted");

        builder.HasIndex(e => e.LastUsedAt)
               .HasDatabaseName("IX_UserDevices_LastUsedAt");

        builder.HasIndex(e => e.TrustedUntil)
               .HasDatabaseName("IX_UserDevices_TrustedUntil");

        // Property configurations
        builder.Property(e => e.UserId)
               .IsRequired()
               .HasMaxLength(450); // Standard length for ASP.NET Identity user IDs

        builder.Property(e => e.DeviceFingerprint)
               .IsRequired()
               .HasMaxLength(100);

        builder.Property(e => e.DeviceName)
               .IsRequired()
               .HasMaxLength(200);

        builder.Property(e => e.IpAddress)
               .HasMaxLength(45) // IPv6 addresses can be up to 45 characters
               .IsRequired(false);

        builder.Property(e => e.UserAgent)
               .HasMaxLength(1000)
               .IsRequired(false);

        // Default values
        builder.Property(e => e.CreatedAt)
               .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(e => e.IsTrusted)
               .HasDefaultValue(false);

        // Foreign key relationship
        builder.HasOne(e => e.User)
               .WithMany(u => u.TrustedDevices)
               .HasForeignKey(e => e.UserId)
               .OnDelete(DeleteBehavior.Cascade)
               .HasConstraintName("FK_UserDevices_AspNetUsers");

        // Value conversions (if needed)
        builder.Property(e => e.CreatedAt)
               .HasConversion(
                   v => v.UtcDateTime,
                   v => new DateTimeOffset(v, TimeSpan.Zero));

        builder.Property(e => e.LastUsedAt)
               .HasConversion(
                   v => v.HasValue ? v.Value.UtcDateTime : (DateTime?)null,
                   v => v.HasValue ? new DateTimeOffset(v.Value, TimeSpan.Zero) : (DateTimeOffset?)null);

        builder.Property(e => e.TrustedUntil)
               .HasConversion(
                   v => v.HasValue ? v.Value.UtcDateTime : (DateTime?)null,
                   v => v.HasValue ? new DateTimeOffset(v.Value, TimeSpan.Zero) : (DateTimeOffset?)null);
    }
}
