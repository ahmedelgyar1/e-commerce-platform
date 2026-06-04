namespace e_commerce_platform.Domain.Entities;

public class ProductAttribute
{
    public Guid ProductId { get; set; }
    public Guid AttributeId { get; set; }

    public Product Product { get; set; } = null!;
    public Attribute Attribute { get; set; } = null!;
}
