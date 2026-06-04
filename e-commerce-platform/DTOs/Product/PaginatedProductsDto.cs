namespace e_commerce_platform.DTOs.Product;

public class PaginatedProductsDto
{
    public List<ProductDto> Items { get; set; } = [];
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}
