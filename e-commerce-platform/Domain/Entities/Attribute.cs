namespace e_commerce_platform.Domain.Entities;

public class Attribute
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Product Product { get; set; } = null!;
    public ICollection<AttributeValue> Values { get; set; } = [];
}
