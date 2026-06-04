using e_commerce_platform.Application.DTOs.Product;

namespace e_commerce_platform.Application.Interfaces;

public interface IProductVariantService
{
    Task<VariantResponseDto> CreateVariantAsync(Guid productId, CreateVariantRequest request, Guid merchantId);
    Task<PaginatedVariantsDto> GetVariantsAsync(Guid productId, VariantQueryParameters queryParams);
    Task<VariantResponseDto> GetVariantByIdAsync(Guid productId, Guid variantId);
    Task<VariantResponseDto> UpdateVariantAsync(Guid productId, Guid variantId, UpdateVariantRequest request, Guid merchantId);
    Task DeleteVariantAsync(Guid productId, Guid variantId, Guid merchantId);
}
