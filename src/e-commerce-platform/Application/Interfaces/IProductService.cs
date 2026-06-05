using e_commerce_platform.Application.DTOs.Product;

namespace e_commerce_platform.Application.Interfaces;

public interface IProductService
{
    Task<ProductDto> CreateProductAsync(CreateProductRequest request, Guid merchantId);
    Task<ProductDto?> GetProductByIdAsync(Guid id);
    Task<ProductDto> UpdateProductAsync(Guid id, UpdateProductRequest request, Guid merchantId);
    Task DeleteProductAsync(Guid id, Guid merchantId);
    Task<PaginatedProductsDto> GetProductsAsync(ProductQueryParameters queryParams);
    Task<ProductDto> UploadProductImageAsync(Guid productId, IFormFile image, Guid merchantId);
}
