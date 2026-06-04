using System.ComponentModel.DataAnnotations;

namespace e_commerce_platform.DTOs.Product;

public class UpdateAttributeRequest
{
    [MaxLength(100)]
    public string? Name { get; set; }

    public int? DisplayOrder { get; set; }
}
