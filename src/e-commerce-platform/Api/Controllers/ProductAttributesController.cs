using e_commerce_platform.Application.DTOs.Product;
using e_commerce_platform.Application.Interfaces;
using e_commerce_platform.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace e_commerce_platform.Api.Controllers;

[ApiController]
[Route("api/products/{productId:guid}/attributes")]
[EnableRateLimiting("general")]
public class ProductAttributesController : ControllerBase
{
    private readonly IProductAttributeService _attributeService;
    private readonly ICurrentUserService _currentUserService;

    public ProductAttributesController(
        IProductAttributeService attributeService,
        ICurrentUserService currentUserService)
    {
        _attributeService = attributeService;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Creates a new attribute definition (e.g. Size, Color) for a product.
    /// </summary>
    /// <param name="productId">The unique identifier of the product.</param>
    /// <param name="request">The attribute name, display order, and optional values.</param>
    /// <response code="201">Returns the created attribute details.</response>
    /// <response code="400">If validation fails or maximum attributes exceeded.</response>
    /// <response code="401">If the merchant is not authenticated or does not own the product.</response>
    /// <response code="429">If the rate limit is exceeded.</response>
    [HttpPost]
    [Authorize(Policy = "MerchantOnly")]
    [ProducesResponseType(typeof(ApiResponse<AttributeResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> AddAttribute(Guid productId, [FromBody] CreateAttributeRequest request)
    {
        var merchantId = _currentUserService.UserId;
        if (merchantId == null)
        {
            return Unauthorized(new { message = "User is not authenticated." });
        }

        var result = await _attributeService.AddAttributeAsync(productId, request, merchantId.Value);
        return CreatedAtAction(nameof(ListAttributes), new { productId }, new { message = "Attribute added successfully.", data = result });
    }

    /// <summary>
    /// Lists all defined attributes and their allowed values for a product.
    /// </summary>
    /// <param name="productId">The unique identifier of the product.</param>
    /// <response code="200">Returns the attributes and their values.</response>
    /// <response code="429">If the rate limit is exceeded.</response>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<List<AttributeResponseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> ListAttributes(Guid productId)
    {
        var result = await _attributeService.GetAttributesByProductIdAsync(productId);
        return Ok(new { message = "Attributes retrieved successfully.", data = result });
    }

    /// <summary>
    /// Updates specific details of an existing attribute configuration.
    /// </summary>
    /// <param name="productId">The unique identifier of the product.</param>
    /// <param name="attributeId">The unique identifier of the attribute.</param>
    /// <param name="request">The fields to update.</param>
    /// <response code="200">Returns the updated attribute details.</response>
    /// <response code="400">If validation fails.</response>
    /// <response code="401">If the merchant is not authenticated or does not own the product.</response>
    /// <response code="404">If the attribute does not exist.</response>
    /// <response code="429">If the rate limit is exceeded.</response>
    [HttpPatch("{attributeId:guid}")]
    [Authorize(Policy = "MerchantOnly")]
    [ProducesResponseType(typeof(ApiResponse<AttributeResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> UpdateAttribute(Guid productId, Guid attributeId, [FromBody] UpdateAttributeRequest request)
    {
        var merchantId = _currentUserService.UserId;
        if (merchantId == null)
        {
            return Unauthorized(new { message = "User is not authenticated." });
        }

        var result = await _attributeService.UpdateAttributeAsync(productId, attributeId, request, merchantId.Value);
        return Ok(new { message = "Attribute updated successfully.", data = result });
    }

    /// <summary>
    /// Deletes an attribute definition and all its child values.
    /// </summary>
    /// <param name="productId">The unique identifier of the product.</param>
    /// <param name="attributeId">The unique identifier of the attribute.</param>
    /// <response code="200">If the attribute is successfully deleted.</response>
    /// <response code="401">If the merchant is not authenticated or does not own the product.</response>
    /// <response code="404">If the attribute does not exist.</response>
    /// <response code="429">If the rate limit is exceeded.</response>
    [HttpDelete("{attributeId:guid}")]
    [Authorize(Policy = "MerchantOnly")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> DeleteAttribute(Guid productId, Guid attributeId)
    {
        var merchantId = _currentUserService.UserId;
        if (merchantId == null)
        {
            return Unauthorized(new { message = "User is not authenticated." });
        }

        await _attributeService.DeleteAttributeAsync(productId, attributeId, merchantId.Value);
        return Ok(new { message = "Attribute deleted successfully." });
    }

    /// <summary>
    /// Appends a new value option to an existing attribute.
    /// </summary>
    /// <param name="productId">The unique identifier of the product.</param>
    /// <param name="attributeId">The unique identifier of the attribute.</param>
    /// <param name="request">The value and display order details.</param>
    /// <response code="201">Returns the updated parent attribute containing the new value.</response>
    /// <response code="400">If validation fails.</response>
    /// <response code="401">If the merchant is not authenticated or does not own the product.</response>
    /// <response code="404">If the attribute does not exist.</response>
    /// <response code="429">If the rate limit is exceeded.</response>
    [HttpPost("{attributeId:guid}/values")]
    [Authorize(Policy = "MerchantOnly")]
    [ProducesResponseType(typeof(ApiResponse<AttributeResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> AddAttributeValue(Guid productId, Guid attributeId, [FromBody] CreateAttributeValueRequest request)
    {
        var merchantId = _currentUserService.UserId;
        if (merchantId == null)
        {
            return Unauthorized(new { message = "User is not authenticated." });
        }

        var result = await _attributeService.AddAttributeValueAsync(productId, attributeId, request, merchantId.Value);
        return CreatedAtAction(nameof(ListAttributes), new { productId }, new { message = "Attribute value added successfully.", data = result });
    }

    /// <summary>
    /// Updates specific details of an existing attribute value option.
    /// </summary>
    /// <param name="productId">The unique identifier of the product.</param>
    /// <param name="attributeId">The unique identifier of the attribute.</param>
    /// <param name="valueId">The unique identifier of the attribute value option.</param>
    /// <param name="request">The fields to update.</param>
    /// <response code="200">Returns the updated value details.</response>
    /// <response code="400">If validation fails.</response>
    /// <response code="401">If the merchant is not authenticated or does not own the product.</response>
    /// <response code="404">If the attribute value option does not exist.</response>
    /// <response code="429">If the rate limit is exceeded.</response>
    [HttpPatch("{attributeId:guid}/values/{valueId:guid}")]
    [Authorize(Policy = "MerchantOnly")]
    [ProducesResponseType(typeof(ApiResponse<AttributeValueResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> UpdateAttributeValue(Guid productId, Guid attributeId, Guid valueId, [FromBody] UpdateAttributeValueRequest request)
    {
        var merchantId = _currentUserService.UserId;
        if (merchantId == null)
        {
            return Unauthorized(new { message = "User is not authenticated." });
        }

        var result = await _attributeService.UpdateAttributeValueAsync(productId, attributeId, valueId, request, merchantId.Value);
        return Ok(new { message = "Attribute value updated successfully.", data = result });
    }

    /// <summary>
    /// Deletes a specific value option from an attribute.
    /// </summary>
    /// <param name="productId">The unique identifier of the product.</param>
    /// <param name="attributeId">The unique identifier of the attribute.</param>
    /// <param name="valueId">The unique identifier of the attribute value option.</param>
    /// <response code="200">If the value option is successfully deleted.</response>
    /// <response code="401">If the merchant is not authenticated or does not own the product.</response>
    /// <response code="404">If the attribute value option does not exist.</response>
    /// <response code="429">If the rate limit is exceeded.</response>
    [HttpDelete("{attributeId:guid}/values/{valueId:guid}")]
    [Authorize(Policy = "MerchantOnly")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> DeleteAttributeValue(Guid productId, Guid attributeId, Guid valueId)
    {
        var merchantId = _currentUserService.UserId;
        if (merchantId == null)
        {
            return Unauthorized(new { message = "User is not authenticated." });
        }

        await _attributeService.DeleteAttributeValueAsync(productId, attributeId, valueId, merchantId.Value);
        return Ok(new { message = "Attribute value deleted successfully." });
    }
}
