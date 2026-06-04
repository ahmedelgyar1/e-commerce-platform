using System.ComponentModel.DataAnnotations;
using e_commerce_platform.Domain.Enums;

namespace e_commerce_platform.DTOs.Product;

public class UpdateProductRequest
{
    [MaxLength(150)]
    public string? Name { get; set; }

    [MaxLength(2000)]
    public string? Description { get; set; }

    [Range(0.01, double.MaxValue)]
    public decimal? BasePrice { get; set; }

    public ProductStatus? Status { get; set; }
}
