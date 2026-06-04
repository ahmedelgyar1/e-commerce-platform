namespace e_commerce_platform.Application.DTOs.Product;

public class AttributeValueResponseDto
{
    public Guid Id { get; set; }
    public Guid AttributeId { get; set; }
    public string Value { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
}
