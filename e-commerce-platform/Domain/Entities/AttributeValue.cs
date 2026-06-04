namespace e_commerce_platform.Domain.Entities;

public class AttributeValue
{
    public Guid Id { get; set; }
    public Guid AttributeId { get; set; }
    public string Value { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Attribute Attribute { get; set; } = null!;
    public ICollection<ProductVariantAttributeValue> ProductVariantAttributeValues { get; set; } = [];
}
