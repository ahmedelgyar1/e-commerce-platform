using e_commerce_platform.Application.DTOs.Product;
using e_commerce_platform.Application.Interfaces;
using e_commerce_platform.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace e_commerce_platform.Api.Controllers;

[ApiController]
[Route("api/products/{productId:guid}/variants")]
[EnableRateLimiting("general")]
public class ProductVariantsController : ControllerBase
{
    private readonly IProductVariantService _variantService;
    private readonly ICurrentUserService _currentUserService;

    public ProductVariantsController(
        IProductVariantService variantService,
        ICurrentUserService currentUserService)
    {
        _variantService = variantService;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Creates a new variant of a product (e.g. Size: Medium, Color: Blue).
    /// </summary>
    /// <param name="productId">The unique identifier of the product.</param>
    /// <param name="request">The variant specifications (SKU, Qty, price overrides, attribute values).</param>
    /// <response code="201">Returns the created variant details.</response>
    /// <response code="400">If attributes are missing, or duplicate combination or SKU exists.</response>
    /// <response code="401">If the merchant is not authenticated or does not own the product.</response>
    /// <response code="429">If the rate limit is exceeded.</response>
    [HttpPost]
    [Authorize(Policy = "MerchantOnly")]
    [ProducesResponseType(typeof(ApiResponse<VariantResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> CreateVariant(Guid productId, [FromBody] CreateVariantRequest request)
    {
        var merchantId = _currentUserService.UserId;
        if (merchantId == null)
        {
            return Unauthorized(new { message = "User is not authenticated." });
        }

        var result = await _variantService.CreateVariantAsync(productId, request, merchantId.Value);
        return CreatedAtAction(nameof(GetVariantById), new { productId, variantId = result.Id }, new { message = "Variant created successfully.", data = result });
    }

    /// <summary>
    /// Lists all variants created under a specific product.
    /// </summary>
    /// <param name="productId">The unique identifier of the product.</param>
    /// <param name="queryParams">The query filters and pagination details.</param>
    /// <response code="200">Returns the matching variants.</response>
    /// <response code="429">If the rate limit is exceeded.</response>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<PaginatedVariantsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> ListVariants(Guid productId, [FromQuery] VariantQueryParameters queryParams)
    {
        var result = await _variantService.GetVariantsAsync(productId, queryParams);
        return Ok(new { message = "Variants retrieved successfully.", data = result });
    }

    /// <summary>
    /// Retrieves a specific product variant by its unique identifier.
    /// </summary>
    /// <param name="productId">The unique identifier of the parent product.</param>
    /// <param name="variantId">The unique identifier of the variant.</param>
    /// <response code="200">Returns the detailed variant information.</response>
    /// <response code="404">If the variant does not exist under the specified product.</response>
    /// <response code="429">If the rate limit is exceeded.</response>
    [HttpGet("{variantId:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<VariantResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> GetVariantById(Guid productId, Guid variantId)
    {
        var result = await _variantService.GetVariantByIdAsync(productId, variantId);
        return Ok(new { message = "Variant retrieved successfully.", data = result });
    }

    /// <summary>
    /// Updates live properties (quantity, price, active state) of an existing variant.
    /// </summary>
    /// <param name="productId">The unique identifier of the parent product.</param>
    /// <param name="variantId">The unique identifier of the variant.</param>
    /// <param name="request">The fields to update.</param>
    /// <response code="200">Returns the updated variant details.</response>
    /// <response code="400">If validation fails.</response>
    /// <response code="401">If the merchant is not authenticated or does not own the product.</response>
    /// <response code="404">If the variant does not exist under the specified product.</response>
    /// <response code="429">If the rate limit is exceeded.</response>
    [HttpPatch("{variantId:guid}")]
    [Authorize(Policy = "MerchantOnly")]
    [ProducesResponseType(typeof(ApiResponse<VariantResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> UpdateVariant(Guid productId, Guid variantId, [FromBody] UpdateVariantRequest request)
    {
        var merchantId = _currentUserService.UserId;
        if (merchantId == null)
        {
            return Unauthorized(new { message = "User is not authenticated." });
        }

        var result = await _variantService.UpdateVariantAsync(productId, variantId, request, merchantId.Value);
        return Ok(new { message = "Variant updated successfully.", data = result });
    }

    /// <summary>
    /// Soft deletes an existing variant.
    /// </summary>
    /// <param name="productId">The unique identifier of the parent product.</param>
    /// <param name="variantId">The unique identifier of the variant.</param>
    /// <response code="204">If the variant is successfully deleted.</response>
    /// <response code="403">If the merchant is not authenticated or does not own the product.</response>
    /// <response code="404">If the variant does not exist under the specified product.</response>
    /// <response code="429">If the rate limit is exceeded.</response>
    [HttpDelete("{variantId:guid}")]
    [Authorize(Policy = "MerchantOnly")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> DeleteVariant(Guid productId, Guid variantId)
    {
        var merchantId = _currentUserService.UserId;
        if (merchantId == null)
        {
            return Unauthorized(new { message = "User is not authenticated." });
        }

        await _variantService.DeleteVariantAsync(productId, variantId, merchantId.Value);
        return NoContent();
    }
}
