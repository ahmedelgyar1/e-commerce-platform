using e_commerce_platform.Domain.Common;

namespace e_commerce_platform.Domain.Entities;

public class ProductVariant : ISoftDelete
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string SKU { get; set; } = string.Empty;
    public int Quantity { get; set; } = 0;
    public decimal? PriceOverride { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }

    public Product Product { get; set; } = null!;
    public ICollection<ProductVariantAttributeValue> AttributeValues { get; set; } = [];
}
