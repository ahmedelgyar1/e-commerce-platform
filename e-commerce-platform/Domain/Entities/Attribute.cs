namespace e_commerce_platform.Domain.Entities;

public class Attribute
{
    public Guid Id { get; set; }
    public Guid? MerchantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Merchant? Merchant { get; set; }
    public ICollection<AttributeValue> Values { get; set; } = [];
    public ICollection<ProductAttribute> ProductAttributes { get; set; } = [];
}
