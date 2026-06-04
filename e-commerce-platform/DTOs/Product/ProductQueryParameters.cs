using e_commerce_platform.Domain.Enums;

namespace e_commerce_platform.DTOs.Product;

public class ProductQueryParameters
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? Search { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public ProductStatus? Status { get; set; }
    public Guid? MerchantId { get; set; }
}
