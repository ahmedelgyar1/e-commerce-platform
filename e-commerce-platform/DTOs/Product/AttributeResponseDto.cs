namespace e_commerce_platform.DTOs.Product;

public class AttributeResponseDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public List<AttributeValueResponseDto> Values { get; set; } = [];
}
