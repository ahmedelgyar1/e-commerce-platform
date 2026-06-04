using e_commerce_platform.DTOs.Product;
using e_commerce_platform.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace e_commerce_platform.Controllers;

[ApiController]
[Route("api/products/{productId:guid}/attributes")]
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

    [HttpPost]
    [Authorize(Policy = "MerchantOnly")]
    public async Task<IActionResult> AddAttribute(Guid productId, [FromBody] CreateAttributeRequest request)
    {
        var merchantId = _currentUserService.UserId;
        if (merchantId == null)
        {
            return Unauthorized(new { message = "User is not authenticated." });
        }

        try
        {
            var result = await _attributeService.AddAttributeAsync(productId, request, merchantId.Value);
            return CreatedAtAction(nameof(ListAttributes), new { productId }, new { message = "Attribute added successfully.", data = result });
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
    public async Task<IActionResult> ListAttributes(Guid productId)
    {
        var result = await _attributeService.GetAttributesByProductIdAsync(productId);
        return Ok(new { message = "Attributes retrieved successfully.", data = result });
    }

    [HttpPatch("{attributeId:guid}")]
    [Authorize(Policy = "MerchantOnly")]
    public async Task<IActionResult> UpdateAttribute(Guid productId, Guid attributeId, [FromBody] UpdateAttributeRequest request)
    {
        var merchantId = _currentUserService.UserId;
        if (merchantId == null)
        {
            return Unauthorized(new { message = "User is not authenticated." });
        }

        try
        {
            var result = await _attributeService.UpdateAttributeAsync(productId, attributeId, request, merchantId.Value);
            return Ok(new { message = "Attribute updated successfully.", data = result });
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

    [HttpDelete("{attributeId:guid}")]
    [Authorize(Policy = "MerchantOnly")]
    public async Task<IActionResult> DeleteAttribute(Guid productId, Guid attributeId)
    {
        var merchantId = _currentUserService.UserId;
        if (merchantId == null)
        {
            return Unauthorized(new { message = "User is not authenticated." });
        }

        try
        {
            await _attributeService.DeleteAttributeAsync(productId, attributeId, merchantId.Value);
            return Ok(new { message = "Attribute deleted successfully." });
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

    [HttpPost("{attributeId:guid}/values")]
    [Authorize(Policy = "MerchantOnly")]
    public async Task<IActionResult> AddAttributeValue(Guid productId, Guid attributeId, [FromBody] CreateAttributeValueRequest request)
    {
        var merchantId = _currentUserService.UserId;
        if (merchantId == null)
        {
            return Unauthorized(new { message = "User is not authenticated." });
        }

        try
        {
            var result = await _attributeService.AddAttributeValueAsync(productId, attributeId, request, merchantId.Value);
            return Ok(new { message = "Attribute value added successfully.", data = result });
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

    [HttpPatch("{attributeId:guid}/values/{valueId:guid}")]
    [Authorize(Policy = "MerchantOnly")]
    public async Task<IActionResult> UpdateAttributeValue(Guid productId, Guid attributeId, Guid valueId, [FromBody] UpdateAttributeValueRequest request)
    {
        var merchantId = _currentUserService.UserId;
        if (merchantId == null)
        {
            return Unauthorized(new { message = "User is not authenticated." });
        }

        try
        {
            var result = await _attributeService.UpdateAttributeValueAsync(productId, attributeId, valueId, request, merchantId.Value);
            return Ok(new { message = "Attribute value updated successfully.", data = result });
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

    [HttpDelete("{attributeId:guid}/values/{valueId:guid}")]
    [Authorize(Policy = "MerchantOnly")]
    public async Task<IActionResult> DeleteAttributeValue(Guid productId, Guid attributeId, Guid valueId)
    {
        var merchantId = _currentUserService.UserId;
        if (merchantId == null)
        {
            return Unauthorized(new { message = "User is not authenticated." });
        }

        try
        {
            await _attributeService.DeleteAttributeValueAsync(productId, attributeId, valueId, merchantId.Value);
            return Ok(new { message = "Attribute value deleted successfully." });
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
