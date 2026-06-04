using e_commerce_platform.Domain.Entities;
using e_commerce_platform.DTOs.Product;
using e_commerce_platform.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace e_commerce_platform.Services.Implementations;

public class ProductVariantService : IProductVariantService
{
    private readonly IProductRepository _productRepository;
    private readonly IProductVariantRepository _variantRepository;
    private readonly IAttributeRepository _attributeRepository;
    private readonly ILogger<ProductVariantService> _logger;

    public ProductVariantService(
        IProductRepository productRepository,
        IProductVariantRepository variantRepository,
        IAttributeRepository attributeRepository,
        ILogger<ProductVariantService> logger)
    {
        _productRepository = productRepository;
        _variantRepository = variantRepository;
        _attributeRepository = attributeRepository;
        _logger = logger;
    }

    public async Task<VariantResponseDto> CreateVariantAsync(Guid productId, CreateVariantRequest request, Guid merchantId)
    {
        await VerifyProductOwnershipAsync(productId, merchantId);

        var skuTrimmed = request.SKU.Trim();
        var isSkuUnique = await _variantRepository.IsSkuUniqueAsync(skuTrimmed);
        if (!isSkuUnique)
        {
            throw new InvalidOperationException($"SKU '{skuTrimmed}' is already in use.");
        }

        var attributes = await _attributeRepository.GetByProductIdAsync(productId);
        if (attributes.Count == 0)
        {
            throw new InvalidOperationException("Cannot create variants for a product with no attributes.");
        }

        var validValueIds = attributes.SelectMany(a => a.Values).Select(v => v.Id).ToHashSet();
        foreach (var id in request.AttributeValueIds)
        {
            if (!validValueIds.Contains(id))
            {
                throw new InvalidOperationException($"Attribute value ID '{id}' is not valid for this product.");
            }
        }

        var requestedValuesMapped = attributes.SelectMany(a => a.Values)
            .Where(v => request.AttributeValueIds.Contains(v.Id))
            .ToList();

        if (requestedValuesMapped.Select(v => v.AttributeId).Distinct().Count() != attributes.Count)
        {
            throw new InvalidOperationException("The variant must specify exactly one value for each of the product's attributes.");
        }

        var existingVariants = await _variantRepository.GetVariantsQueryable(productId)
            .Include(pv => pv.AttributeValues)
            .ToListAsync();

        var requestSet = request.AttributeValueIds.ToHashSet();
        foreach (var ev in existingVariants)
        {
            var evSet = ev.AttributeValues.Select(av => av.AttributeValueId).ToHashSet();
            if (requestSet.SetEquals(evSet))
            {
                throw new InvalidOperationException("A variant with this combination of attributes already exists.");
            }
        }

        var variantId = Guid.NewGuid();
        var variant = new ProductVariant
        {
            Id = variantId,
            ProductId = productId,
            SKU = skuTrimmed,
            Quantity = request.Quantity,
            PriceOverride = request.PriceOverride,
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            AttributeValues = request.AttributeValueIds.Select(valId => new ProductVariantAttributeValue
            {
                ProductVariantId = variantId,
                AttributeValueId = valId
            }).ToList()
        };

        await _variantRepository.AddAsync(variant);
        await _variantRepository.SaveChangesAsync();

        var createdVariant = await _variantRepository.GetWithValuesAsync(variantId);
        return MapToVariantDto(createdVariant!);
    }

    public async Task<PaginatedVariantsDto> GetVariantsAsync(Guid productId, ProductQueryParameters queryParams)
    {
        var query = _variantRepository.GetVariantsQueryable(productId);

        if (!string.IsNullOrWhiteSpace(queryParams.Search))
        {
            var searchLower = queryParams.Search.ToLower();
            query = query.Where(v => v.SKU.ToLower().Contains(searchLower));
        }

        if (queryParams.Status.HasValue)
        {
            var activeState = queryParams.Status.Value == Domain.Enums.ProductStatus.Active;
            query = query.Where(v => v.IsActive == activeState);
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderBy(v => v.SKU)
            .Skip((queryParams.Page - 1) * queryParams.PageSize)
            .Take(queryParams.PageSize)
            .ToListAsync();

        return new PaginatedVariantsDto
        {
            Items = items.Select(MapToVariantDto).ToList(),
            TotalCount = totalCount,
            PageNumber = queryParams.Page,
            PageSize = queryParams.PageSize
        };
    }

    public async Task<VariantResponseDto> GetVariantByIdAsync(Guid productId, Guid variantId)
    {
        var variant = await _variantRepository.GetWithValuesAsync(variantId);
        if (variant == null || variant.ProductId != productId)
        {
            throw new KeyNotFoundException("Variant not found for this product.");
        }

        return MapToVariantDto(variant);
    }

    public async Task<VariantResponseDto> UpdateVariantAsync(Guid productId, Guid variantId, UpdateVariantRequest request, Guid merchantId)
    {
        await VerifyProductOwnershipAsync(productId, merchantId);

        var variant = await _variantRepository.GetWithValuesAsync(variantId);
        if (variant == null || variant.ProductId != productId)
        {
            throw new KeyNotFoundException("Variant not found for this product.");
        }

        if (request.Quantity.HasValue)
        {
            variant.Quantity = request.Quantity.Value;
        }

        if (request.PriceOverride.HasValue)
        {
            variant.PriceOverride = request.PriceOverride.Value;
        }

        if (request.IsActive.HasValue)
        {
            variant.IsActive = request.IsActive.Value;
        }

        variant.UpdatedAt = DateTime.UtcNow;

        _variantRepository.Update(variant);
        await _variantRepository.SaveChangesAsync();

        return MapToVariantDto(variant);
    }

    public async Task DeleteVariantAsync(Guid productId, Guid variantId, Guid merchantId)
    {
        await VerifyProductOwnershipAsync(productId, merchantId);

        var variant = await _variantRepository.GetByIdAsync(variantId);
        if (variant == null || variant.ProductId != productId)
        {
            throw new KeyNotFoundException("Variant not found for this product.");
        }

        _variantRepository.Delete(variant);
        await _variantRepository.SaveChangesAsync();
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

    private VariantResponseDto MapToVariantDto(ProductVariant variant)
    {
        return new VariantResponseDto
        {
            Id = variant.Id,
            ProductId = variant.ProductId,
            SKU = variant.SKU,
            Quantity = variant.Quantity,
            PriceOverride = variant.PriceOverride,
            IsActive = variant.IsActive,
            AttributeValues = variant.AttributeValues.Select(pvav => new AttributeValueResponseDto
            {
                Id = pvav.AttributeValue.Id,
                AttributeId = pvav.AttributeValue.AttributeId,
                Value = pvav.AttributeValue.Value,
                DisplayOrder = pvav.AttributeValue.DisplayOrder
            }).ToList()
        };
    }
}
