using System.ComponentModel.DataAnnotations;

namespace e_commerce_platform.DTOs.Product;

public class CreateAttributeRequest
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public int DisplayOrder { get; set; }

    public List<CreateAttributeValueRequest> Values { get; set; } = [];
}
