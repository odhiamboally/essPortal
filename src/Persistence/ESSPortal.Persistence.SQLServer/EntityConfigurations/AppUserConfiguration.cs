using ESSPortal.Domain.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ESSPortal.Persistence.SQLServer.EntityConfigurations;
internal sealed class AppUserConfiguration : IEntityTypeConfiguration<AppUser>
{
    public void Configure(EntityTypeBuilder<AppUser> builder)
    {
        // Table configuration
        builder.ToTable("Users");

        // Check constraints using the updated approach
        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_AspNetUsers_FailedLoginAttempts", "[FailedLoginAttempts] >= 0");
            t.HasCheckConstraint("CK_AspNetUsers_Gender", "[Gender] IN ('Male', 'Female', 'Other', 'PreferNotToSay') OR [Gender] IS NULL");
        });

        // Primary key (inherited from IdentityUser)
        builder.HasKey(e => e.Id);

        // Indexes for performance
        builder.HasIndex(e => e.EmployeeNumber)
               .IsUnique()
               .HasDatabaseName("IX_AspNetUsers_EmployeeNumber");

        builder.HasIndex(e => e.Email)
               .IsUnique()
               .HasDatabaseName("IX_AspNetUsers_Email");

        builder.HasIndex(e => new { e.IsDeleted, e.IsActive })
               .HasDatabaseName("IX_AspNetUsers_IsDeleted_IsActive");

        builder.HasIndex(e => e.LastLoginAt)
               .HasDatabaseName("IX_AspNetUsers_LastLoginAt");

        builder.HasIndex(e => e.CreatedAt)
               .HasDatabaseName("IX_AspNetUsers_CreatedAt");

        builder.HasIndex(e => e.Department)
               .HasDatabaseName("IX_AspNetUsers_Department");

        // Property configurations
        builder.Property(e => e.EmployeeNumber)
               .HasMaxLength(50)
               .IsRequired(false);

        builder.Property(e => e.FirstName)
               .HasMaxLength(100)
               .IsRequired(false);

        builder.Property(e => e.LastName)
               .HasMaxLength(100)
               .IsRequired(false);

        builder.Property(e => e.Gender)
               .HasMaxLength(20)
               .IsRequired(false);

        builder.Property(e => e.ProfilePictureUrl)
               .HasMaxLength(500)
               .IsRequired(false);

        builder.Property(e => e.Department)
               .HasMaxLength(200)
               .IsRequired(false);

        builder.Property(e => e.JobTitle)
               .HasMaxLength(200)
               .IsRequired(false);

        builder.Property(e => e.ManagerId)
               .HasMaxLength(450)
               .IsRequired(false);

        builder.Property(e => e.CreatedBy)
               .HasMaxLength(100)
               .IsRequired(false);

        builder.Property(e => e.UpdatedBy)
               .HasMaxLength(100)
               .IsRequired(false);

        builder.Property(e => e.DeletedBy)
               .HasMaxLength(100)
               .IsRequired(false);

        // Default values
        builder.Property(e => e.CreatedAt)
               .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(e => e.IsActive)
               .HasDefaultValue(true);

        builder.Property(e => e.IsDeleted)
               .HasDefaultValue(false);

        builder.Property(e => e.FailedLoginAttempts)
               .HasDefaultValue(0);

        builder.Property(e => e.RequirePasswordChange)
               .HasDefaultValue(false);

        // Self-referencing relationship for Manager-Employee hierarchy
        builder.HasOne(e => e.Manager)
               .WithMany(e => e.DirectReports)
               .HasForeignKey(e => e.ManagerId)
               .OnDelete(DeleteBehavior.Restrict)
               .HasConstraintName("FK_AspNetUsers_Manager");

        // Relationships with other entities
        builder.HasMany(e => e.TrustedDevices)
               .WithOne(d => d.User)
               .HasForeignKey(d => d.UserId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.RefreshTokens)
               .WithOne(rt => rt.User)
               .HasForeignKey(rt => rt.UserId)
               .OnDelete(DeleteBehavior.Cascade);

        // Link AppUser to Business Central Profile
        builder.HasOne(e => e.Profile)  
               .WithOne(p => p.User)         // Profile needs User navigation property
               .HasForeignKey<Profile>(p => p.UserId)  // Profile has UserId FK
               .OnDelete(DeleteBehavior.Cascade)
               .HasConstraintName("FK_Profiles_Users");

        // Ignore computed properties (they're marked as [NotMapped] but this is explicit)
        builder.Ignore(e => e.FullName);
        builder.Ignore(e => e.DisplayName);

        // Global query filter for soft delete
        builder.HasQueryFilter(e => !e.IsDeleted);


       

    }


    
}
