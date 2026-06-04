using System.ComponentModel.DataAnnotations;

namespace e_commerce_platform.DTOs.Product;

public class UpdateAttributeValueRequest
{
    [MaxLength(150)]
    public string? Value { get; set; }

    public int? DisplayOrder { get; set; }
}
