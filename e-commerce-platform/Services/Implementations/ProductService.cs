using e_commerce_platform.Domain.Entities;
using e_commerce_platform.Domain.Enums;
using e_commerce_platform.DTOs.Product;
using e_commerce_platform.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace e_commerce_platform.Services.Implementations;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly ILogger<ProductService> _logger;

    public ProductService(IProductRepository productRepository, ILogger<ProductService> logger)
    {
        _productRepository = productRepository;
        _logger = logger;
    }

    public async Task<ProductDto> CreateProductAsync(CreateProductRequest request, Guid merchantId)
    {
        _logger.LogInformation("Creating product '{Name}' for merchant {MerchantId}.", request.Name, merchantId);

        var product = new Product
        {
            Id = Guid.NewGuid(),
            MerchantId = merchantId,
            Name = request.Name,
            Description = request.Description,
            BasePrice = request.BasePrice,
            Status = request.Status,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _productRepository.AddAsync(product);
        await _productRepository.SaveChangesAsync();

        _logger.LogInformation("Product {ProductId} created successfully.", product.Id);
        return MapToDto(product);
    }

    public async Task<ProductDto?> GetProductByIdAsync(Guid id)
    {
        _logger.LogInformation("Retrieving product by id: {ProductId}", id);
        var product = await _productRepository.GetProductWithMerchantAsync(id);
        if (product == null)
        {
            return null;
        }
        return MapToDto(product);
    }

    public async Task<ProductDto> UpdateProductAsync(Guid id, UpdateProductRequest request, Guid merchantId)
    {
        _logger.LogInformation("Updating product {ProductId} for merchant {MerchantId}.", id, merchantId);

        var product = await _productRepository.GetByIdAsync(id);
        if (product == null)
        {
            throw new KeyNotFoundException("Product not found.");
        }

        if (product.MerchantId != merchantId)
        {
            _logger.LogWarning("Unauthorized attempt to update product {ProductId} by merchant {MerchantId}.", id, merchantId);
            throw new UnauthorizedAccessException("You are not authorized to update this product.");
        }

        if (request.Name != null) product.Name = request.Name;
        if (request.Description != null) product.Description = request.Description;
        if (request.BasePrice.HasValue) product.BasePrice = request.BasePrice.Value;
        if (request.Status.HasValue) product.Status = request.Status.Value;
        product.UpdatedAt = DateTime.UtcNow;

        _productRepository.Update(product);
        await _productRepository.SaveChangesAsync();

        _logger.LogInformation("Product {ProductId} updated successfully.", id);
        return MapToDto(product);
    }

    public async Task DeleteProductAsync(Guid id, Guid merchantId)
    {
        _logger.LogInformation("Deleting product {ProductId} for merchant {MerchantId}.", id, merchantId);

        var product = await _productRepository.GetByIdAsync(id);
        if (product == null)
        {
            throw new KeyNotFoundException("Product not found.");
        }

        if (product.MerchantId != merchantId)
        {
            _logger.LogWarning("Unauthorized attempt to delete product {ProductId} by merchant {MerchantId}.", id, merchantId);
            throw new UnauthorizedAccessException("You are not authorized to delete this product.");
        }

        _productRepository.Delete(product);
        await _productRepository.SaveChangesAsync();

        _logger.LogInformation("Product {ProductId} deleted successfully.", id);
    }

    public async Task<PaginatedProductsDto> GetProductsAsync(ProductQueryParameters queryParams)
    {
        _logger.LogInformation("Retrieving paginated products.");

        var query = _productRepository.GetProductsQueryable();

        if (!string.IsNullOrWhiteSpace(queryParams.Search))
        {
            var searchLower = queryParams.Search.ToLower();
            query = query.Where(p => p.Name.ToLower().Contains(searchLower) || 
                                     (p.Description != null && p.Description.ToLower().Contains(searchLower)));
        }

        if (queryParams.MinPrice.HasValue)
        {
            query = query.Where(p => p.BasePrice >= queryParams.MinPrice.Value);
        }

        if (queryParams.MaxPrice.HasValue)
        {
            query = query.Where(p => p.BasePrice <= queryParams.MaxPrice.Value);
        }

        if (queryParams.Status.HasValue)
        {
            query = query.Where(p => p.Status == queryParams.Status.Value);
        }

        if (queryParams.MerchantId.HasValue)
        {
            query = query.Where(p => p.MerchantId == queryParams.MerchantId.Value);
        }

        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)queryParams.PageSize);

        var items = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((queryParams.Page - 1) * queryParams.PageSize)
            .Take(queryParams.PageSize)
            .ToListAsync();

        return new PaginatedProductsDto
        {
            Items = items.Select(MapToDto).ToList(),
            TotalCount = totalCount,
            Page = queryParams.Page,
            PageSize = queryParams.PageSize,
            TotalPages = totalPages
        };
    }

    private ProductDto MapToDto(Product product)
    {
        return new ProductDto
        {
            Id = product.Id,
            MerchantId = product.MerchantId,
            Name = product.Name,
            Description = product.Description ?? string.Empty,
            BasePrice = product.BasePrice,
            Status = product.Status,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt
        };
    }
}
