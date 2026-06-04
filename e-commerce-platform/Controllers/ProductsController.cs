using e_commerce_platform.Domain.Enums;
using e_commerce_platform.DTOs.Product;
using e_commerce_platform.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace e_commerce_platform.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(
        IProductService productService,
        ICurrentUserService currentUserService,
        ILogger<ProductsController> logger)
    {
        _productService = productService;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    [HttpPost]
    [Authorize(Policy = "MerchantOnly")]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductRequest request)
    {
        var merchantId = _currentUserService.UserId;
        if (merchantId == null)
        {
            return Unauthorized(new { message = "User is not authenticated." });
        }

        var result = await _productService.CreateProductAsync(request, merchantId.Value);
        return CreatedAtAction(nameof(GetProductById), new { id = result.Id }, new { message = "Product created successfully.", data = result });
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> ListProducts([FromQuery] ProductQueryParameters queryParams)
    {
        if (queryParams.Page < 1) queryParams.Page = 1;
        if (queryParams.PageSize < 1 || queryParams.PageSize > 100) queryParams.PageSize = 10;

        var finalStatus = queryParams.Status;

        if (!_currentUserService.IsAuthenticated)
        {
            finalStatus = ProductStatus.Active;
        }
        else if (!queryParams.MerchantId.HasValue || queryParams.MerchantId.Value != _currentUserService.UserId)
        {
            finalStatus = ProductStatus.Active;
        }

        queryParams.Status = finalStatus;

        var result = await _productService.GetProductsAsync(queryParams);
        return Ok(new { message = "Products retrieved successfully.", data = result });
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetProductById(Guid id)
    {
        var product = await _productService.GetProductByIdAsync(id);
        if (product == null)
        {
            return NotFound(new { message = "Product not found." });
        }

        if (product.Status != ProductStatus.Active)
        {
            var currentUserId = _currentUserService.UserId;
            if (product.MerchantId != currentUserId)
            {
                return NotFound(new { message = "Product not found." });
            }
        }

        return Ok(new { message = "Product retrieved successfully.", data = product });
    }

    [HttpPatch("{id:guid}")]
    [Authorize(Policy = "MerchantOnly")]
    public async Task<IActionResult> UpdateProduct(Guid id, [FromBody] UpdateProductRequest request)
    {
        var merchantId = _currentUserService.UserId;
        if (merchantId == null)
        {
            return Unauthorized(new { message = "User is not authenticated." });
        }

        try
        {
            var result = await _productService.UpdateProductAsync(id, request, merchantId.Value);
            return Ok(new { message = "Product updated successfully.", data = result });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "Product not found." });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "MerchantOnly")]
    public async Task<IActionResult> DeleteProduct(Guid id)
    {
        var merchantId = _currentUserService.UserId;
        if (merchantId == null)
        {
            return Unauthorized(new { message = "User is not authenticated." });
        }

        try
        {
            await _productService.DeleteProductAsync(id, merchantId.Value);
            return Ok(new { message = "Product deleted successfully." });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "Product not found." });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }
}
