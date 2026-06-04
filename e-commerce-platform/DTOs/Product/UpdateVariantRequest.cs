using System.ComponentModel.DataAnnotations;

namespace e_commerce_platform.DTOs.Product;

public class UpdateVariantRequest
{
    [Range(0, int.MaxValue)]
    public int? Quantity { get; set; }

    [Range(0.01, double.MaxValue)]
    public decimal? PriceOverride { get; set; }

    public bool? IsActive { get; set; }
}
