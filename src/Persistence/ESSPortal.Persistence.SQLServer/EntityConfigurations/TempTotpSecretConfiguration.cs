using ESSPortal.Domain.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ESSPortal.Persistence.SQLServer.EntityConfigurations;
public class TempTotpSecretConfiguration : IEntityTypeConfiguration<TempTotpSecret>
{
    public void Configure(EntityTypeBuilder<TempTotpSecret> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.UserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(e => e.EncryptedSecret)
            .IsRequired()
            .HasMaxLength(500);

        builder.HasIndex(e => e.UserId);

        builder.HasIndex(e => e.ExpiresAt);

        builder.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
