using ESSPortal.Domain.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ESSPortal.Persistence.SQLServer.EntityConfigurations;
public class UserProfileConfiguration : IEntityTypeConfiguration<UserProfile>
{
    public void Configure(EntityTypeBuilder<UserProfile> builder)
    {
        // Table name
        builder.ToTable("UserProfiles");

        // Primary key
        builder.HasKey(x => x.Id);

        // Properties
        builder.Property(x => x.Id)
            .IsRequired()
            .HasMaxLength(36);

        builder.Property(x => x.UserId)
            .IsRequired()
            .HasMaxLength(450);

        // Contact Information
        builder.Property(x => x.CountryRegionCode)
            .HasMaxLength(10);

        builder.Property(x => x.PhysicalAddress)
            .HasMaxLength(500);

        builder.Property(x => x.TelephoneNo)
            .HasMaxLength(20);

        builder.Property(x => x.MobileNo)
            .HasMaxLength(20);

        builder.Property(x => x.PostalAddress)
            .HasMaxLength(500);

        builder.Property(x => x.PostCode)
            .HasMaxLength(20);

        builder.Property(x => x.City)
            .HasMaxLength(100);

        builder.Property(x => x.ContactEMailAddress)
            .HasMaxLength(256);

        // Banking Information
        builder.Property(x => x.BankAccountType)
            .HasMaxLength(50);

        builder.Property(x => x.KBABankCode)
            .HasMaxLength(10);

        builder.Property(x => x.KBABranchCode)
            .HasMaxLength(10);

        builder.Property(x => x.BankAccountNo)
            .HasMaxLength(20);

        // Base Entity properties
        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.CreatedBy)
            .HasMaxLength(450);


        builder.Property(x => x.UpdatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedBy)
            .HasMaxLength(450);


        builder.Property(x => x.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.DeletedBy)
            .HasMaxLength(450);

        builder.Property(x => x.RowVersion)
            .IsRowVersion();

        // Relationships
        builder.HasOne(x => x.User)
            .WithOne() // Assuming AppUser doesn't have navigation back to UserProfile
            .HasForeignKey<UserProfile>(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => x.UserId)
            .IsUnique()
            .HasDatabaseName("IX_UserProfiles_UserId");

        builder.HasIndex(x => x.IsDeleted)
            .HasDatabaseName("IX_UserProfiles_IsDeleted");

        builder.HasIndex(x => x.CreatedAt)
            .HasDatabaseName("IX_UserProfiles_CreatedAt");
    }
}
