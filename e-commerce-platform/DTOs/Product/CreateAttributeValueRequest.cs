using System.ComponentModel.DataAnnotations;

namespace e_commerce_platform.DTOs.Product;

public class CreateAttributeValueRequest
{
    [Required]
    [MaxLength(150)]
    public string Value { get; set; } = string.Empty;

    [Required]
    public int DisplayOrder { get; set; }
}
