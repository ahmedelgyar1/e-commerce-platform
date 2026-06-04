using e_commerce_platform.Domain.Entities;
using e_commerce_platform.Application.DTOs.Product;
using e_commerce_platform.Application.Services;
using e_commerce_platform.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace e_commerce_platform.UnitTests;

public class ProductVariantServiceTests
{
    private readonly Mock<IProductRepository> _productRepoMock;
    private readonly Mock<IProductVariantRepository> _variantRepoMock;
    private readonly Mock<IAttributeRepository> _attributeRepoMock;
    private readonly Mock<ILogger<ProductVariantService>> _loggerMock;
    private readonly ProductVariantService _service;

    public ProductVariantServiceTests()
    {
        _productRepoMock = new Mock<IProductRepository>();
        _variantRepoMock = new Mock<IProductVariantRepository>();
        _attributeRepoMock = new Mock<IAttributeRepository>();
        _loggerMock = new Mock<ILogger<ProductVariantService>>();

        _service = new ProductVariantService(
            _productRepoMock.Object,
            _variantRepoMock.Object,
            _attributeRepoMock.Object,
            _loggerMock.Object
        );
    }

    [Fact]
    public async Task CreateVariantAsync_ShouldThrowKeyNotFoundException_WhenProductDoesNotExist()
    {
        var productId = Guid.NewGuid();
        var merchantId = Guid.NewGuid();
        var request = new CreateVariantRequest { SKU = "SKU123" };

        _productRepoMock
            .Setup(m => m.GetByIdAsync(productId))
            .ReturnsAsync((Product?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _service.CreateVariantAsync(productId, request, merchantId));
    }

    [Fact]
    public async Task CreateVariantAsync_ShouldThrowUnauthorizedAccessException_WhenMerchantDoesNotOwnProduct()
    {
        var productId = Guid.NewGuid();
        var merchantId = Guid.NewGuid();
        var otherMerchantId = Guid.NewGuid();
        var request = new CreateVariantRequest { SKU = "SKU123" };

        var product = new Product { Id = productId, MerchantId = otherMerchantId };

        _productRepoMock
            .Setup(m => m.GetByIdAsync(productId))
            .ReturnsAsync(product);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _service.CreateVariantAsync(productId, request, merchantId));
    }

    [Fact]
    public async Task CreateVariantAsync_ShouldThrowInvalidOperationException_WhenSkuIsNotUnique()
    {
        var productId = Guid.NewGuid();
        var merchantId = Guid.NewGuid();
        var request = new CreateVariantRequest { SKU = "SKU-DUP" };
        var product = new Product { Id = productId, MerchantId = merchantId };

        _productRepoMock
            .Setup(m => m.GetByIdAsync(productId))
            .ReturnsAsync(product);

        _variantRepoMock
            .Setup(m => m.IsSkuUniqueAsync("SKU-DUP"))
            .ReturnsAsync(false);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.CreateVariantAsync(productId, request, merchantId));
    }

    [Fact]
    public async Task CreateVariantAsync_ShouldThrowInvalidOperationException_WhenProductHasNoAttributes()
    {
        var productId = Guid.NewGuid();
        var merchantId = Guid.NewGuid();
        var request = new CreateVariantRequest { SKU = "SKU123" };
        var product = new Product { Id = productId, MerchantId = merchantId };

        _productRepoMock
            .Setup(m => m.GetByIdAsync(productId))
            .ReturnsAsync(product);

        _variantRepoMock
            .Setup(m => m.IsSkuUniqueAsync(It.IsAny<string>()))
            .ReturnsAsync(true);

        _attributeRepoMock
            .Setup(m => m.GetByProductIdAsync(productId))
            .ReturnsAsync(new List<Domain.Entities.Attribute>());

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.CreateVariantAsync(productId, request, merchantId));
    }

    [Fact]
    public async Task CreateVariantAsync_ShouldThrowInvalidOperationException_WhenProvidedAttributeValueDoesNotBelongToProduct()
    {
        var productId = Guid.NewGuid();
        var merchantId = Guid.NewGuid();
        var invalidValueId = Guid.NewGuid();
        var request = new CreateVariantRequest { SKU = "SKU123", AttributeValueIds = [invalidValueId] };
        var product = new Product { Id = productId, MerchantId = merchantId };

        var attributes = new List<Domain.Entities.Attribute>
        {
            new Domain.Entities.Attribute
            {
                Id = Guid.NewGuid(),
                Values = [new AttributeValue { Id = Guid.NewGuid(), Value = "Red" }]
            }
        };

        _productRepoMock
            .Setup(m => m.GetByIdAsync(productId))
            .ReturnsAsync(product);

        _variantRepoMock
            .Setup(m => m.IsSkuUniqueAsync(It.IsAny<string>()))
            .ReturnsAsync(true);

        _attributeRepoMock
            .Setup(m => m.GetByProductIdAsync(productId))
            .ReturnsAsync(attributes);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.CreateVariantAsync(productId, request, merchantId));
    }

    [Fact]
    public async Task CreateVariantAsync_ShouldThrowInvalidOperationException_WhenNotAllAttributesAreSpecified()
    {
        var productId = Guid.NewGuid();
        var merchantId = Guid.NewGuid();
        var valueId = Guid.NewGuid();
        var request = new CreateVariantRequest { SKU = "SKU123", AttributeValueIds = [valueId] };
        var product = new Product { Id = productId, MerchantId = merchantId };

        var attributes = new List<Domain.Entities.Attribute>
        {
            new Domain.Entities.Attribute
            {
                Id = Guid.NewGuid(),
                Values = [new AttributeValue { Id = valueId, Value = "Red" }]
            },
            new Domain.Entities.Attribute
            {
                Id = Guid.NewGuid(),
                Values = [new AttributeValue { Id = Guid.NewGuid(), Value = "Large" }]
            }
        };

        _productRepoMock
            .Setup(m => m.GetByIdAsync(productId))
            .ReturnsAsync(product);

        _variantRepoMock
            .Setup(m => m.IsSkuUniqueAsync(It.IsAny<string>()))
            .ReturnsAsync(true);

        _attributeRepoMock
            .Setup(m => m.GetByProductIdAsync(productId))
            .ReturnsAsync(attributes);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.CreateVariantAsync(productId, request, merchantId));
    }
}
