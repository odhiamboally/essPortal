using ESSPortal.Domain.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ESSPortal.Persistence.SQLServer.EntityConfigurations;
internal sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        // Table configuration with check constraints
        builder.ToTable("RefreshTokens", t =>
        {
            t.HasCheckConstraint("CK_RefreshTokens_ExpiresAt",
                "[ExpiresAt] > [CreatedAt]");

            t.HasCheckConstraint("CK_RefreshTokens_RevokedAt",
                "[RevokedAt] IS NULL OR [RevokedAt] >= [CreatedAt]");

        });

        // Primary key
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
               .ValueGeneratedOnAdd();

        // Property configurations
        builder.Property(e => e.UserId)
               .IsRequired()
               .HasMaxLength(450); // Standard length for ASP.NET Identity user IDs

        builder.Property(e => e.Token)
               .IsRequired()
               .HasMaxLength(500); // JWT tokens can be quite long

        builder.Property(e => e.RevokedByIp)
               .HasMaxLength(45) // IPv6 addresses can be up to 45 characters
               .IsRequired(false);

        builder.Property(e => e.ReplacedByToken)
               .HasMaxLength(500)
               .IsRequired(false);

        // Default values
        builder.Property(e => e.CreatedAt)
               .HasDefaultValueSql("GETUTCDATE()");


        // Foreign key relationship
        builder.HasOne(e => e.User)
               .WithMany(u => u.RefreshTokens)
               .HasForeignKey(e => e.UserId)
               .OnDelete(DeleteBehavior.Cascade)
               .HasConstraintName("FK_RefreshTokens_AspNetUsers");

        // Value conversions for DateTimeOffset properties
        builder.Property(e => e.ExpiresAt)
               .HasConversion(
                   v => v.UtcDateTime,
                   v => new DateTimeOffset(v, TimeSpan.Zero));

        builder.Property(e => e.CreatedAt)
               .HasConversion(
                   v => v.UtcDateTime,
                   v => new DateTimeOffset(v, TimeSpan.Zero));

        builder.Property(e => e.RevokedAt)
               .HasConversion(
                   v => v.HasValue ? v.Value.UtcDateTime : (DateTime?)null,
                   v => v.HasValue ? new DateTimeOffset(v.Value, TimeSpan.Zero) : (DateTimeOffset?)null);

        builder.Property(e => e.UsedAt)
               .HasConversion(
                    v => v.HasValue ? v.Value.UtcDateTime : (DateTime?)null,
                    v => v.HasValue ? new DateTimeOffset(v.Value, TimeSpan.Zero) : (DateTimeOffset?)null);

        builder.Property(e => e.RevokedReason)
               .HasMaxLength(200)
               .IsRequired(false);

        builder.Property(e => e.CreatedByIp)
               .HasMaxLength(45)
               .IsRequired(false);

        builder.Property(e => e.TokenFamily)
               .HasMaxLength(50)
               .IsRequired(false);


        // Indexes for performance and uniqueness
        builder.HasIndex(e => e.Token)
               .IsUnique()
               .HasDatabaseName("IX_RefreshTokens_Token");

        builder.HasIndex(e => new { e.UserId, e.IsRevoked, e.ExpiresAt })
               .HasDatabaseName("IX_RefreshTokens_UserId_IsRevoked_ExpiresAt");

        builder.HasIndex(e => e.UserId)
               .HasDatabaseName("IX_RefreshTokens_UserId");

        builder.HasIndex(e => e.ExpiresAt)
               .HasDatabaseName("IX_RefreshTokens_ExpiresAt");

        builder.HasIndex(e => e.CreatedAt)
               .HasDatabaseName("IX_RefreshTokens_CreatedAt");

        builder.HasIndex(e => e.TokenFamily)
           .HasDatabaseName("IX_RefreshTokens_TokenFamily");

        builder.HasIndex(e => new { e.UserId, e.TokenFamily, e.ExpiresAt })
               .HasDatabaseName("IX_RefreshTokens_UserId_TokenFamily_ExpiresAt");


        builder.Ignore(e => e.IsActive);
        builder.Ignore(e => e.IsUsed);
        builder.Ignore(e => e.IsExpired);
        builder.Ignore(e => e.IsRevoked);



    }
}
