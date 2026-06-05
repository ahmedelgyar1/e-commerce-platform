using System.ComponentModel.DataAnnotations;
using e_commerce_platform.Domain.Enums;

namespace e_commerce_platform.Application.DTOs.Product;

public class CreateProductRequest
{
    [Required]
    [MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string Description { get; set; } = string.Empty;

    [Range(0.01, double.MaxValue)]
    public decimal BasePrice { get; set; }

    public ProductStatus Status { get; set; } = ProductStatus.Draft;

    public IFormFile? Image { get; set; }
}
