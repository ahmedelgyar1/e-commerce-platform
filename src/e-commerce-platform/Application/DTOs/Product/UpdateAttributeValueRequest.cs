using System.ComponentModel.DataAnnotations;

namespace e_commerce_platform.Application.DTOs.Product;

public class UpdateAttributeValueRequest
{
    [MaxLength(150)]
    public string? Value { get; set; }

    public int? DisplayOrder { get; set; }
}
