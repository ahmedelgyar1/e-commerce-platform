using e_commerce_platform.DTOs.Product;
using e_commerce_platform.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace e_commerce_platform.Controllers;

[ApiController]
[Route("api/products/{productId:guid}/variants")]
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

    [HttpPost]
    [Authorize(Policy = "MerchantOnly")]
    public async Task<IActionResult> CreateVariant(Guid productId, [FromBody] CreateVariantRequest request)
    {
        var merchantId = _currentUserService.UserId;
        if (merchantId == null)
        {
            return Unauthorized(new { message = "User is not authenticated." });
        }

        try
        {
            var result = await _variantService.CreateVariantAsync(productId, request, merchantId.Value);
            return CreatedAtAction(nameof(GetVariantById), new { productId, variantId = result.Id }, new { message = "Variant created successfully.", data = result });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> ListVariants(Guid productId, [FromQuery] ProductQueryParameters queryParams)
    {
        var result = await _variantService.GetVariantsAsync(productId, queryParams);
        return Ok(new { message = "Variants retrieved successfully.", data = result });
    }

    [HttpGet("{variantId:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetVariantById(Guid productId, Guid variantId)
    {
        try
        {
            var result = await _variantService.GetVariantByIdAsync(productId, variantId);
            return Ok(new { message = "Variant retrieved successfully.", data = result });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPatch("{variantId:guid}")]
    [Authorize(Policy = "MerchantOnly")]
    public async Task<IActionResult> UpdateVariant(Guid productId, Guid variantId, [FromBody] UpdateVariantRequest request)
    {
        var merchantId = _currentUserService.UserId;
        if (merchantId == null)
        {
            return Unauthorized(new { message = "User is not authenticated." });
        }

        try
        {
            var result = await _variantService.UpdateVariantAsync(productId, variantId, request, merchantId.Value);
            return Ok(new { message = "Variant updated successfully.", data = result });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{variantId:guid}")]
    [Authorize(Policy = "MerchantOnly")]
    public async Task<IActionResult> DeleteVariant(Guid productId, Guid variantId)
    {
        var merchantId = _currentUserService.UserId;
        if (merchantId == null)
        {
            return Unauthorized(new { message = "User is not authenticated." });
        }

        try
        {
            await _variantService.DeleteVariantAsync(productId, variantId, merchantId.Value);
            return Ok(new { message = "Variant deleted successfully." });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }
}
