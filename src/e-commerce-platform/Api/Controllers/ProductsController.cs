using e_commerce_platform.Domain.Enums;
using e_commerce_platform.Application.DTOs.Product;
using e_commerce_platform.Application.Interfaces;
using e_commerce_platform.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace e_commerce_platform.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting("general")]
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

    /// <summary>
    /// Creates a new product for the authenticated merchant.
    /// </summary>
    /// <param name="request">The product creation details.</param>
    /// <response code="201">Returns the created product details.</response>
    /// <response code="400">If the input details are invalid.</response>
    /// <response code="401">If the merchant is not authenticated.</response>
    /// <response code="429">If the rate limit is exceeded.</response>
    [HttpPost]
    [Authorize(Policy = "MerchantOnly")]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> CreateProduct([FromForm] CreateProductRequest request)
    {
        var merchantId = _currentUserService.UserId;
        if (merchantId == null)
        {
            return Unauthorized(new { message = "User is not authenticated." });
        }

        var result = await _productService.CreateProductAsync(request, merchantId.Value);
        return CreatedAtAction(nameof(GetProductById), new { id = result.Id }, new { message = "Product created successfully.", data = result });
    }

    /// <summary>
    /// Retrieves a paginated list of products based on query filters.
    /// </summary>
    /// <param name="queryParams">The query filters, pagination, and sorting parameters.</param>
    /// <response code="200">Returns the list of matching products.</response>
    /// <response code="429">If the rate limit is exceeded.</response>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<PaginatedProductsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
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

    /// <summary>
    /// Retrieves a specific product by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the product.</param>
    /// <response code="200">Returns the detailed product information.</response>
    /// <response code="404">If the product does not exist or is not active (for non-owners).</response>
    /// <response code="429">If the rate limit is exceeded.</response>
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
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

    /// <summary>
    /// Updates specific details of an existing product.
    /// </summary>
    /// <param name="id">The unique identifier of the product.</param>
    /// <param name="request">The fields to update.</param>
    /// <response code="200">Returns the updated product details.</response>
    /// <response code="400">If the input details are invalid.</response>
    /// <response code="401">If the merchant is not authenticated.</response>
    /// <response code="403">If the merchant does not own the product.</response>
    /// <response code="404">If the product does not exist.</response>
    /// <response code="429">If the rate limit is exceeded.</response>
    [HttpPatch("{id:guid}")]
    [Authorize(Policy = "MerchantOnly")]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> UpdateProduct(Guid id, [FromBody] UpdateProductRequest request)
    {
        var merchantId = _currentUserService.UserId;
        if (merchantId == null)
        {
            return Unauthorized(new { message = "User is not authenticated." });
        }

        var result = await _productService.UpdateProductAsync(id, request, merchantId.Value);
        return Ok(new { message = "Product updated successfully.", data = result });
    }

    /// <summary>
    /// Soft deletes an existing product and its cached data.
    /// </summary>
    /// <param name="id">The unique identifier of the product.</param>
    /// <response code="204">If the product is successfully deleted.</response>
    /// <response code="401">If the merchant is not authenticated.</response>
    /// <response code="403">If the merchant does not own the product.</response>
    /// <response code="404">If the product does not exist.</response>
    /// <response code="429">If the rate limit is exceeded.</response>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "MerchantOnly")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> DeleteProduct(Guid id)
    {
        var merchantId = _currentUserService.UserId;
        if (merchantId == null)
        {
            return Unauthorized(new { message = "User is not authenticated." });
        }

        await _productService.DeleteProductAsync(id, merchantId.Value);
        return NoContent();
    }

    /// <summary>
    /// Uploads an image for a specific product to Cloudinary.
    /// </summary>
    /// <param name="id">The unique identifier of the product.</param>
    /// <param name="image">The image file to upload (max 5MB, image types only).</param>
    /// <response code="200">Returns the updated product with the new image URL.</response>
    /// <response code="400">If the file is invalid or too large.</response>
    /// <response code="401">If the merchant is not authenticated.</response>
    /// <response code="403">If the merchant does not own the product.</response>
    /// <response code="404">If the product does not exist.</response>
    /// <response code="429">If the rate limit is exceeded.</response>
    [HttpPost("{id:guid}/image")]
    [Authorize(Policy = "MerchantOnly")]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> UploadProductImage(Guid id, IFormFile image)
    {
        var merchantId = _currentUserService.UserId;
        if (merchantId == null)
        {
            return Unauthorized(new { message = "User is not authenticated." });
        }

        var result = await _productService.UploadProductImageAsync(id, image, merchantId.Value);
        return Ok(new { message = "Product image uploaded successfully.", data = result });
    }
}
