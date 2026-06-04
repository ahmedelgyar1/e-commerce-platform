using e_commerce_platform.Domain.Entities;
using e_commerce_platform.DTOs.Product;
using e_commerce_platform.Services.Interfaces;
using AttributeEntity = e_commerce_platform.Domain.Entities.Attribute;

namespace e_commerce_platform.Services.Implementations;

public class ProductAttributeService : IProductAttributeService
{
    private readonly IProductRepository _productRepository;
    private readonly IAttributeRepository _attributeRepository;
    private readonly IAttributeValueRepository _attributeValueRepository;
    private readonly ILogger<ProductAttributeService> _logger;

    public ProductAttributeService(
        IProductRepository productRepository,
        IAttributeRepository attributeRepository,
        IAttributeValueRepository attributeValueRepository,
        ILogger<ProductAttributeService> logger)
    {
        _productRepository = productRepository;
        _attributeRepository = attributeRepository;
        _attributeValueRepository = attributeValueRepository;
        _logger = logger;
    }

    public async Task<AttributeResponseDto> AddAttributeAsync(Guid productId, CreateAttributeRequest request, Guid merchantId)
    {
        await VerifyProductOwnershipAsync(productId, merchantId);

        var nameTrimmed = request.Name.Trim();
        var exists = await _attributeRepository.ExistsByNameAsync(productId, nameTrimmed);
        if (exists)
        {
            throw new InvalidOperationException($"Attribute '{nameTrimmed}' already exists for this product.");
        }

        var valuesList = new List<AttributeValue>();
        if (request.Values != null && request.Values.Count > 0)
        {
            var valueSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var v in request.Values)
            {
                var valTrimmed = v.Value.Trim();
                if (!valueSet.Add(valTrimmed))
                {
                    throw new InvalidOperationException($"Duplicate value '{valTrimmed}' in the request list.");
                }

                valuesList.Add(new AttributeValue
                {
                    Id = Guid.NewGuid(),
                    Value = valTrimmed,
                    DisplayOrder = v.DisplayOrder,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        var attribute = new AttributeEntity
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            Name = nameTrimmed,
            DisplayOrder = request.DisplayOrder,
            CreatedAt = DateTime.UtcNow,
            Values = valuesList
        };

        await _attributeRepository.AddAsync(attribute);
        await _attributeRepository.SaveChangesAsync();

        return MapToAttributeDto(attribute);
    }

    public async Task<List<AttributeResponseDto>> GetAttributesByProductIdAsync(Guid productId)
    {
        var attributes = await _attributeRepository.GetByProductIdAsync(productId);
        return attributes.Select(MapToAttributeDto).ToList();
    }

    public async Task<AttributeResponseDto> UpdateAttributeAsync(Guid productId, Guid attributeId, UpdateAttributeRequest request, Guid merchantId)
    {
        await VerifyProductOwnershipAsync(productId, merchantId);

        var attribute = await _attributeRepository.GetWithValuesAsync(attributeId);
        if (attribute == null || attribute.ProductId != productId)
        {
            throw new KeyNotFoundException("Attribute not found for this product.");
        }

        if (request.Name != null)
        {
            var nameTrimmed = request.Name.Trim();
            if (!string.Equals(attribute.Name, nameTrimmed, StringComparison.OrdinalIgnoreCase))
            {
                var exists = await _attributeRepository.ExistsByNameAsync(productId, nameTrimmed);
                if (exists)
                {
                    throw new InvalidOperationException($"Another attribute named '{nameTrimmed}' already exists for this product.");
                }
                attribute.Name = nameTrimmed;
            }
        }

        if (request.DisplayOrder.HasValue)
        {
            attribute.DisplayOrder = request.DisplayOrder.Value;
        }

        _attributeRepository.Update(attribute);
        await _attributeRepository.SaveChangesAsync();

        return MapToAttributeDto(attribute);
    }

    public async Task DeleteAttributeAsync(Guid productId, Guid attributeId, Guid merchantId)
    {
        await VerifyProductOwnershipAsync(productId, merchantId);

        var attribute = await _attributeRepository.GetByIdAsync(attributeId);
        if (attribute == null || attribute.ProductId != productId)
        {
            throw new KeyNotFoundException("Attribute not found for this product.");
        }

        _attributeRepository.Delete(attribute);
        await _attributeRepository.SaveChangesAsync();
    }

    public async Task<AttributeValueResponseDto> AddAttributeValueAsync(Guid productId, Guid attributeId, CreateAttributeValueRequest request, Guid merchantId)
    {
        await VerifyProductOwnershipAsync(productId, merchantId);

        var attribute = await _attributeRepository.GetByIdAsync(attributeId);
        if (attribute == null || attribute.ProductId != productId)
        {
            throw new KeyNotFoundException("Attribute not found for this product.");
        }

        var valTrimmed = request.Value.Trim();
        var exists = await _attributeValueRepository.ExistsByValueAsync(attributeId, valTrimmed);
        if (exists)
        {
            throw new InvalidOperationException($"Value '{valTrimmed}' already exists for this attribute.");
        }

        var value = new AttributeValue
        {
            Id = Guid.NewGuid(),
            AttributeId = attributeId,
            Value = valTrimmed,
            DisplayOrder = request.DisplayOrder,
            CreatedAt = DateTime.UtcNow
        };

        await _attributeValueRepository.AddAsync(value);
        await _attributeValueRepository.SaveChangesAsync();

        return MapToValueDto(value);
    }

    public async Task<AttributeValueResponseDto> UpdateAttributeValueAsync(Guid productId, Guid attributeId, Guid valueId, UpdateAttributeValueRequest request, Guid merchantId)
    {
        await VerifyProductOwnershipAsync(productId, merchantId);

        var attribute = await _attributeRepository.GetByIdAsync(attributeId);
        if (attribute == null || attribute.ProductId != productId)
        {
            throw new KeyNotFoundException("Attribute not found for this product.");
        }

        var value = await _attributeValueRepository.GetByIdAsync(valueId);
        if (value == null || value.AttributeId != attributeId)
        {
            throw new KeyNotFoundException("Attribute value not found.");
        }

        if (request.Value != null)
        {
            var valTrimmed = request.Value.Trim();
            if (!string.Equals(value.Value, valTrimmed, StringComparison.OrdinalIgnoreCase))
            {
                var exists = await _attributeValueRepository.ExistsByValueAsync(attributeId, valTrimmed);
                if (exists)
                {
                    throw new InvalidOperationException($"Another value '{valTrimmed}' already exists for this attribute.");
                }
                value.Value = valTrimmed;
            }
        }

        if (request.DisplayOrder.HasValue)
        {
            value.DisplayOrder = request.DisplayOrder.Value;
        }

        _attributeValueRepository.Update(value);
        await _attributeValueRepository.SaveChangesAsync();

        return MapToValueDto(value);
    }

    public async Task DeleteAttributeValueAsync(Guid productId, Guid attributeId, Guid valueId, Guid merchantId)
    {
        await VerifyProductOwnershipAsync(productId, merchantId);

        var attribute = await _attributeRepository.GetByIdAsync(attributeId);
        if (attribute == null || attribute.ProductId != productId)
        {
            throw new KeyNotFoundException("Attribute not found for this product.");
        }

        var value = await _attributeValueRepository.GetByIdAsync(valueId);
        if (value == null || value.AttributeId != attributeId)
        {
            throw new KeyNotFoundException("Attribute value not found.");
        }

        _attributeValueRepository.Delete(value);
        await _attributeValueRepository.SaveChangesAsync();
    }

    private async Task VerifyProductOwnershipAsync(Guid productId, Guid merchantId)
    {
        var product = await _productRepository.GetByIdAsync(productId);
        if (product == null)
        {
            throw new KeyNotFoundException("Product not found.");
        }

        if (product.MerchantId != merchantId)
        {
            throw new UnauthorizedAccessException("You do not own this product.");
        }
    }

    private AttributeResponseDto MapToAttributeDto(AttributeEntity attribute)
    {
        return new AttributeResponseDto
        {
            Id = attribute.Id,
            ProductId = attribute.ProductId,
            Name = attribute.Name,
            DisplayOrder = attribute.DisplayOrder,
            Values = attribute.Values.OrderBy(v => v.DisplayOrder).Select(MapToValueDto).ToList()
        };
    }

    private AttributeValueResponseDto MapToValueDto(AttributeValue value)
    {
        return new AttributeValueResponseDto
        {
            Id = value.Id,
            AttributeId = value.AttributeId,
            Value = value.Value,
            DisplayOrder = value.DisplayOrder
        };
    }
}
