using e_commerce_platform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace e_commerce_platform.Infrastructure.Data.Configurations;

public class ProductVariantConfiguration : IEntityTypeConfiguration<ProductVariant>
{
    public void Configure(EntityTypeBuilder<ProductVariant> builder)
    {
        builder.ToTable("ProductVariants");

        builder.HasKey(pv => pv.Id);

        builder.Property(pv => pv.SKU)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(pv => pv.SKU)
            .IsUnique();

        builder.Property(pv => pv.Quantity)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(pv => pv.PriceOverride)
            .HasColumnType("decimal(18,2)");

        builder.Property(pv => pv.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(pv => pv.CreatedAt)
            .IsRequired();

        builder.Property(pv => pv.UpdatedAt)
            .IsRequired();

        builder.HasOne(pv => pv.Product)
            .WithMany(p => p.Variants)
            .HasForeignKey(pv => pv.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
