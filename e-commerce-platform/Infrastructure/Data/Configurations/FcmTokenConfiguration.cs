using e_commerce_platform.Domain.Entities;
using e_commerce_platform.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace e_commerce_platform.Infrastructure.Data.Configurations;

public class FcmTokenConfiguration : IEntityTypeConfiguration<FcmToken>
{
    public void Configure(EntityTypeBuilder<FcmToken> builder)
    {
        builder.ToTable("FcmTokens");

        builder.HasKey(f => f.Id);

        builder.Property(f => f.DeviceToken)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(f => f.DeviceType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(f => f.CreatedAt).IsRequired();

        builder.HasOne(f => f.User)
            .WithMany(u => u.FcmTokens)
            .HasForeignKey(f => f.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
