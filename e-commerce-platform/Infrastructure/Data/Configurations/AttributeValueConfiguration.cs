using e_commerce_platform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace e_commerce_platform.Infrastructure.Data.Configurations;

public class AttributeValueConfiguration : IEntityTypeConfiguration<AttributeValue>
{
    public void Configure(EntityTypeBuilder<AttributeValue> builder)
    {
        builder.ToTable("AttributeValues");

        builder.HasKey(av => av.Id);

        builder.Property(av => av.Value)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(av => av.CreatedAt)
            .IsRequired();

        builder.HasOne(av => av.Attribute)
            .WithMany(a => a.Values)
            .HasForeignKey(av => av.AttributeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
