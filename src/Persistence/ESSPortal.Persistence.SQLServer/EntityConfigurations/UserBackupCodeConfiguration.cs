using ESSPortal.Domain.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ESSPortal.Persistence.SQLServer.EntityConfigurations;
public class UserBackupCodeConfiguration : IEntityTypeConfiguration<UserBackupCode>
{
    public void Configure(EntityTypeBuilder<UserBackupCode> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.UserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(e => e.HashedCode)
            .IsRequired()
            .HasMaxLength(500);

        builder.HasIndex(e => e.UserId);

        builder.HasIndex(e => new { e.UserId, e.HashedCode });

        builder.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
