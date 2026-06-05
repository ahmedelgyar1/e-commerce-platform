using e_commerce_platform.Domain.Entities;
using e_commerce_platform.Application.DTOs.Product;
using e_commerce_platform.Application.Interfaces;
using e_commerce_platform.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace e_commerce_platform.Application.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly IImageService _imageService;
    private readonly IMemoryCache _cache;
    private readonly ILogger<ProductService> _logger;

    private static string ProductCacheKey(Guid id) => $"product:{id}";
    private static readonly MemoryCacheEntryOptions CacheOptions = new MemoryCacheEntryOptions()
        .SetSlidingExpiration(TimeSpan.FromMinutes(10));

    public ProductService(IProductRepository productRepository, IImageService imageService, IMemoryCache cache, ILogger<ProductService> logger)
    {
        _productRepository = productRepository;
        _imageService = imageService;
        _cache = cache;
        _logger = logger;
    }

    public async Task<ProductDto> CreateProductAsync(CreateProductRequest request, Guid merchantId)
    {
        _logger.LogInformation("Creating product '{Name}' for merchant {MerchantId}.", request.Name, merchantId);

        string? imageUrl = null;
        string? imagePublicId = null;

        if (request.Image != null)
        {
            ValidateImage(request.Image);
            var uploadResult = await _imageService.UploadImageAsync(request.Image, "products");
            imageUrl = uploadResult.Url;
            imagePublicId = uploadResult.PublicId;
        }

        var product = new Product
        {
            Id = Guid.NewGuid(),
            MerchantId = merchantId,
            Name = request.Name,
            Description = request.Description,
            BasePrice = request.BasePrice,
            Status = request.Status,
            ImageUrl = imageUrl,
            ImagePublicId = imagePublicId,
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

        if (_cache.TryGetValue(ProductCacheKey(id), out ProductDto? cached))
        {
            _logger.LogInformation("Cache hit for product {ProductId}.", id);
            return cached;
        }

        var product = await _productRepository.GetProductWithMerchantAsync(id);
        if (product == null)
        {
            return null;
        }

        var dto = MapToDto(product);
        _cache.Set(ProductCacheKey(id), dto, CacheOptions);

        return dto;
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

        _cache.Remove(ProductCacheKey(id));
        _logger.LogInformation("Product {ProductId} updated and cache invalidated.", id);

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

        _cache.Remove(ProductCacheKey(id));
        _logger.LogInformation("Product {ProductId} deleted and cache invalidated.", id);
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

    public async Task<ProductDto> UploadProductImageAsync(Guid productId, IFormFile image, Guid merchantId)
    {
        _logger.LogInformation("Uploading image for product {ProductId} by merchant {MerchantId}.", productId, merchantId);

        var product = await _productRepository.GetByIdAsync(productId);
        if (product == null)
        {
            throw new KeyNotFoundException("Product not found.");
        }

        if (product.MerchantId != merchantId)
        {
            _logger.LogWarning("Unauthorized attempt to upload image for product {ProductId} by merchant {MerchantId}.", productId, merchantId);
            throw new UnauthorizedAccessException("You are not authorized to upload an image for this product.");
        }

        ValidateImage(image);

        // Delete old image if exists
        if (!string.IsNullOrEmpty(product.ImagePublicId))
        {
            _logger.LogInformation("Deleting old image {PublicId} for product {ProductId}.", product.ImagePublicId, productId);
            await _imageService.DeleteImageAsync(product.ImagePublicId);
        }

        var uploadResult = await _imageService.UploadImageAsync(image, "products");

        product.ImageUrl = uploadResult.Url;
        product.ImagePublicId = uploadResult.PublicId;
        product.UpdatedAt = DateTime.UtcNow;

        _productRepository.Update(product);
        await _productRepository.SaveChangesAsync();

        _cache.Remove(ProductCacheKey(productId));
        _logger.LogInformation("Product {ProductId} image uploaded and cache invalidated.", productId);

        return MapToDto(product);
    }

    private void ValidateImage(IFormFile image)
    {
        var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp", "image/gif" };
        if (!allowedTypes.Contains(image.ContentType.ToLower()))
        {
            throw new ArgumentException("Invalid file type. Only JPEG, PNG, WebP, and GIF are allowed.");
        }

        const long maxFileSize = 5 * 1024 * 1024; // 5 MB
        if (image.Length > maxFileSize)
        {
            throw new ArgumentException("File size exceeds the maximum limit of 5MB.");
        }
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
            ImageUrl = product.ImageUrl,
            Status = product.Status,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt
        };
    }
}
