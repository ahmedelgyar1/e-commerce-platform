using e_commerce_platform.DTOs.Product;

namespace e_commerce_platform.Services.Interfaces;

public interface IProductAttributeService
{
    Task<AttributeResponseDto> AddAttributeAsync(Guid productId, CreateAttributeRequest request, Guid merchantId);
    Task<List<AttributeResponseDto>> GetAttributesByProductIdAsync(Guid productId);
    Task<AttributeResponseDto> UpdateAttributeAsync(Guid productId, Guid attributeId, UpdateAttributeRequest request, Guid merchantId);
    Task DeleteAttributeAsync(Guid productId, Guid attributeId, Guid merchantId);

    Task<AttributeValueResponseDto> AddAttributeValueAsync(Guid productId, Guid attributeId, CreateAttributeValueRequest request, Guid merchantId);
    Task<AttributeValueResponseDto> UpdateAttributeValueAsync(Guid productId, Guid attributeId, Guid valueId, UpdateAttributeValueRequest request, Guid merchantId);
    Task DeleteAttributeValueAsync(Guid productId, Guid attributeId, Guid valueId, Guid merchantId);
}
