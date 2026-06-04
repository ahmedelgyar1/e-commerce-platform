using System.ComponentModel.DataAnnotations;

namespace e_commerce_platform.DTOs.Product;

public class CreateVariantRequest
{
    [Required]
    [MaxLength(100)]
    public string SKU { get; set; } = string.Empty;

    [Required]
    [Range(0, int.MaxValue)]
    public int Quantity { get; set; }

    [Range(0.01, double.MaxValue)]
    public decimal? PriceOverride { get; set; }

    public bool IsActive { get; set; } = true;

    [Required]
    [MinLength(1)]
    public List<Guid> AttributeValueIds { get; set; } = [];
}
