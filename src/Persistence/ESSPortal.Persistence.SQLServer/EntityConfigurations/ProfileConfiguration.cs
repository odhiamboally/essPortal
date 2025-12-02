using ESSPortal.Domain.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ESSPortal.Persistence.SQLServer.EntityConfigurations;
internal sealed class ProfileConfiguration : IEntityTypeConfiguration<Profile>
{
    public void Configure(EntityTypeBuilder<Profile> builder)
    {
        // Table name
        builder.ToTable("Profiles");

        // Primary key
        builder.HasKey(e => e.Id);

        // Unique index on Emp_No (Business Central employee number)
        builder.HasIndex(e => e.Emp_No)
               .IsUnique()
               .HasDatabaseName("IX_Profiles_Emp_No");

        builder.HasIndex(e => e.Email)
               .HasDatabaseName("IX_Profiles_Email");

        // Property configurations
        builder.Property(e => e.Emp_No)
               .HasMaxLength(50)
               .IsRequired();

        builder.Property(e => e.Email)
               .HasMaxLength(256)
               .IsRequired(false);

        builder.Property(e => e.Name)
               .HasMaxLength(200)
               .IsRequired(false);

        // Relationship with Uploads
        builder.HasMany(e => e.Uploads)
               .WithOne(u => u.Profile)
               .HasForeignKey(u => u.ProfileId)
               .OnDelete(DeleteBehavior.Cascade);

    }
}
