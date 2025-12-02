using ESSPortal.Domain.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ESSPortal.Persistence.SQLServer.EntityConfigurations;
internal sealed class UserSessionConfiguration : IEntityTypeConfiguration<UserSession>
{
    public void Configure(EntityTypeBuilder<UserSession> builder)
    {
        // Table configuration with check constraints
        builder.ToTable("UserSessions", t =>
        {
            t.HasCheckConstraint("CK_UserSessions_ExpiresAt",
                "[ExpiresAt] > [CreatedAt]");

            t.HasCheckConstraint("CK_UserSessions_EndedAt",
                "[EndedAt] IS NULL OR [EndedAt] >= [CreatedAt]");

            t.HasCheckConstraint("CK_UserSessions_LastAccessedAt",
                "[LastAccessedAt] >= [CreatedAt]");
        });

        // Primary key
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
               .ValueGeneratedNever() // Since BaseEntity generates Guid
               .HasMaxLength(36);

        // Property configurations
        builder.Property(e => e.UserId)
               .HasMaxLength(450) // Reference to AppUser.Id
               .IsRequired();

        builder.Property(e => e.IpAddress)
               .HasMaxLength(50)
               .IsRequired();

        builder.Property(e => e.UserAgent)
               .HasMaxLength(500);

        builder.Property(e => e.EndReason)
               .HasMaxLength(100);

        builder.Property(e => e.IsActive)
               .HasDefaultValue(true);

        builder.Property(e => e.LastAccessedAt)
               .IsRequired();

        builder.Property(e => e.ExpiresAt)
               .IsRequired();

        builder.Property(e => e.EndedAt)
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

        // Foreign key relationship
        builder.HasOne(e => e.User)
               .WithMany() // Assuming AppUser doesn't have navigation back to UserSession
               .HasForeignKey(e => e.UserId)
               .OnDelete(DeleteBehavior.Cascade)
               .HasConstraintName("FK_UserSessions_Users_UserId");

        // Indexes
        builder.HasIndex(e => e.UserId)
               .HasDatabaseName("IX_UserSessions_UserId");

        builder.HasIndex(e => e.IsActive)
               .HasDatabaseName("IX_UserSessions_IsActive");

        builder.HasIndex(e => e.ExpiresAt)
               .HasDatabaseName("IX_UserSessions_ExpiresAt");

        builder.HasIndex(e => new { e.UserId, e.IsActive })
               .HasDatabaseName("IX_UserSessions_UserId_IsActive");

        builder.HasIndex(e => e.LastAccessedAt)
               .HasDatabaseName("IX_UserSessions_LastAccessedAt");

        builder.HasIndex(e => new { e.IsActive, e.ExpiresAt })
               .HasDatabaseName("IX_UserSessions_IsActive_ExpiresAt");

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

        builder.Property(e => e.LastAccessedAt)
               .HasConversion(
                   v => v.UtcDateTime,
                   v => new DateTimeOffset(v, TimeSpan.Zero));

        builder.Property(e => e.ExpiresAt)
               .HasConversion(
                   v => v.UtcDateTime,
                   v => new DateTimeOffset(v, TimeSpan.Zero));

        builder.Property(e => e.EndedAt)
               .HasConversion(
                   v => v.HasValue ? v.Value.UtcDateTime : (DateTime?)null,
                   v => v.HasValue ? new DateTimeOffset(v.Value, TimeSpan.Zero) : (DateTimeOffset?)null);

        builder.Property(e => e.DeletedAt)
               .HasConversion(
                   v => v.HasValue ? v.Value.UtcDateTime : (DateTime?)null,
                   v => v.HasValue ? new DateTimeOffset(v.Value, TimeSpan.Zero) : (DateTimeOffset?)null);
    }
}
