using ESSPortal.Domain.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ESSPortal.Persistence.SQLServer.EntityConfigurations;
public class UserTotpSecretConfiguration : IEntityTypeConfiguration<UserTotpSecret>
{
    public void Configure(EntityTypeBuilder<UserTotpSecret> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.UserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(e => e.EncryptedSecret)
            .IsRequired()
            .HasMaxLength(500);

        builder.HasIndex(e => e.UserId);

        builder.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
