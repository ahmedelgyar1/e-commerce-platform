using e_commerce_platform.Domain.Entities;
using e_commerce_platform.Domain.Enums;
using e_commerce_platform.Application.DTOs.Product;
using e_commerce_platform.Application.Services;
using e_commerce_platform.Domain.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace e_commerce_platform.UnitTests.Products;

public class ProductServiceTests
{
    private readonly Mock<IProductRepository> _productRepoMock;
    private readonly Mock<IMemoryCache> _cacheMock;
    private readonly Mock<ILogger<ProductService>> _loggerMock;
    private readonly ProductService _service;

    public ProductServiceTests()
    {
        _productRepoMock = new Mock<IProductRepository>();
        _cacheMock = new Mock<IMemoryCache>();
        _loggerMock = new Mock<ILogger<ProductService>>();
        
        var cacheEntryMock = new Mock<ICacheEntry>();
        _cacheMock
            .Setup(m => m.CreateEntry(It.IsAny<object>()))
            .Returns(cacheEntryMock.Object);

        _service = new ProductService(
            _productRepoMock.Object,
            _cacheMock.Object,
            _loggerMock.Object
        );
    }

    [Fact]
    public async Task CreateProductAsync_ShouldCreateProductSuccessfully()
    {
        var merchantId = Guid.NewGuid();
        var request = new CreateProductRequest
        {
            Name = "Nike Air Max",
            Description = "Premium Sneakers",
            BasePrice = 120.00m,
            Status = ProductStatus.Active
        };

        var result = await _service.CreateProductAsync(request, merchantId);

        Assert.Equal(request.Name, result.Name);
        Assert.Equal(request.BasePrice, result.BasePrice);
        Assert.Equal(merchantId, result.MerchantId);
        _productRepoMock.Verify(m => m.AddAsync(It.IsAny<Product>()), Times.Once);
        _productRepoMock.Verify(m => m.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateProductAsync_ShouldThrowKeyNotFoundException_WhenProductDoesNotExist()
    {
        var productId = Guid.NewGuid();
        var merchantId = Guid.NewGuid();
        var request = new UpdateProductRequest { Name = "New Name" };

        _productRepoMock
            .Setup(m => m.GetByIdAsync(productId))
            .ReturnsAsync((Product?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _service.UpdateProductAsync(productId, request, merchantId));
    }

    [Fact]
    public async Task UpdateProductAsync_ShouldThrowUnauthorizedAccessException_WhenMerchantDoesNotOwnProduct()
    {
        var productId = Guid.NewGuid();
        var merchantId = Guid.NewGuid();
        var otherMerchantId = Guid.NewGuid();
        var request = new UpdateProductRequest { Name = "New Name" };

        var existingProduct = new Product
        {
            Id = productId,
            MerchantId = otherMerchantId,
            Name = "Old Name"
        };

        _productRepoMock
            .Setup(m => m.GetByIdAsync(productId))
            .ReturnsAsync(existingProduct);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _service.UpdateProductAsync(productId, request, merchantId));
    }

    [Fact]
    public async Task UpdateProductAsync_ShouldUpdateSuccessfully_WhenValidationPasses()
    {
        var productId = Guid.NewGuid();
        var merchantId = Guid.NewGuid();
        var request = new UpdateProductRequest { Name = "Updated Name", BasePrice = 150.00m };

        var existingProduct = new Product
        {
            Id = productId,
            MerchantId = merchantId,
            Name = "Old Name",
            BasePrice = 120.00m
        };

        _productRepoMock
            .Setup(m => m.GetByIdAsync(productId))
            .ReturnsAsync(existingProduct);

        var result = await _service.UpdateProductAsync(productId, request, merchantId);

        Assert.Equal("Updated Name", result.Name);
        Assert.Equal(150.00m, result.BasePrice);
        _productRepoMock.Verify(m => m.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteProductAsync_ShouldThrowUnauthorizedAccessException_WhenMerchantDoesNotOwnProduct()
    {
        var productId = Guid.NewGuid();
        var merchantId = Guid.NewGuid();
        var otherMerchantId = Guid.NewGuid();

        var existingProduct = new Product
        {
            Id = productId,
            MerchantId = otherMerchantId,
            Name = "Product"
        };

        _productRepoMock
            .Setup(m => m.GetByIdAsync(productId))
            .ReturnsAsync(existingProduct);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _service.DeleteProductAsync(productId, merchantId));
    }

    [Fact]
    public async Task DeleteProductAsync_ShouldDeleteSuccessfully_WhenValidationPasses()
    {
        var productId = Guid.NewGuid();
        var merchantId = Guid.NewGuid();

        var existingProduct = new Product
        {
            Id = productId,
            MerchantId = merchantId,
            Name = "Product"
        };

        _productRepoMock
            .Setup(m => m.GetByIdAsync(productId))
            .ReturnsAsync(existingProduct);

        await _service.DeleteProductAsync(productId, merchantId);

        _productRepoMock.Verify(m => m.Delete(existingProduct), Times.Once);
        _productRepoMock.Verify(m => m.SaveChangesAsync(), Times.Once);
    }
}
