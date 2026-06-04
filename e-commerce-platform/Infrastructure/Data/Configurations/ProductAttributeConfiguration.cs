using e_commerce_platform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace e_commerce_platform.Infrastructure.Data.Configurations;

public class ProductAttributeConfiguration : IEntityTypeConfiguration<ProductAttribute>
{
    public void Configure(EntityTypeBuilder<ProductAttribute> builder)
    {
        builder.ToTable("ProductAttributes");

        builder.HasKey(pa => new { pa.ProductId, pa.AttributeId });

        builder.HasOne(pa => pa.Product)
            .WithMany(p => p.ProductAttributes)
            .HasForeignKey(pa => pa.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(pa => pa.Attribute)
            .WithMany(a => a.ProductAttributes)
            .HasForeignKey(pa => pa.AttributeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
