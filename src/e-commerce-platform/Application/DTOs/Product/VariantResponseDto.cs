namespace e_commerce_platform.Application.DTOs.Product;

public class VariantResponseDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string SKU { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal? PriceOverride { get; set; }
    public bool IsActive { get; set; }
    public List<AttributeValueResponseDto> AttributeValues { get; set; } = [];
}
