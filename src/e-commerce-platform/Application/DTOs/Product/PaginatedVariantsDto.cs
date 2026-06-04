namespace e_commerce_platform.Application.DTOs.Product;

public class PaginatedVariantsDto
{
    public List<VariantResponseDto> Items { get; set; } = [];
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}
