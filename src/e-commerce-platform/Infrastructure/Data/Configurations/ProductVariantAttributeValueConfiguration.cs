using e_commerce_platform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace e_commerce_platform.Infrastructure.Data.Configurations;

public class ProductVariantAttributeValueConfiguration : IEntityTypeConfiguration<ProductVariantAttributeValue>
{
    public void Configure(EntityTypeBuilder<ProductVariantAttributeValue> builder)
    {
        builder.ToTable("ProductVariantAttributeValues");

        builder.HasKey(pvav => new { pvav.ProductVariantId, pvav.AttributeValueId });

        builder.HasOne(pvav => pvav.ProductVariant)
            .WithMany(pv => pv.AttributeValues)
            .HasForeignKey(pvav => pvav.ProductVariantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(pvav => pvav.AttributeValue)
            .WithMany(av => av.ProductVariantAttributeValues)
            .HasForeignKey(pvav => pvav.AttributeValueId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
