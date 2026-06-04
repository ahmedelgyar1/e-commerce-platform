namespace e_commerce_platform.Domain.Entities;

public class ProductVariantAttributeValue
{
    public Guid ProductVariantId { get; set; }
    public Guid AttributeValueId { get; set; }

    public ProductVariant ProductVariant { get; set; } = null!;
    public AttributeValue AttributeValue { get; set; } = null!;
}
