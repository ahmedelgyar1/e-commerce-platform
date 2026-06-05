using e_commerce_platform.Domain.Common;
using e_commerce_platform.Domain.Enums;

namespace e_commerce_platform.Domain.Entities;

public class Product : ISoftDelete
{
    public Guid Id { get; set; }
    public Guid MerchantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ProductStatus Status { get; set; } = ProductStatus.Draft;
    public decimal BasePrice { get; set; }
    public string? ImageUrl { get; set; }
    public string? ImagePublicId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }

    public Merchant Merchant { get; set; } = null!;
    public ICollection<ProductVariant> Variants { get; set; } = [];
    public ICollection<Attribute> Attributes { get; set; } = [];
}
