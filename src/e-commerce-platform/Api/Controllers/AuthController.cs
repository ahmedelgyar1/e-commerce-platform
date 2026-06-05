using e_commerce_platform.Application.DTOs.Auth;
using e_commerce_platform.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace e_commerce_platform.Api.Controllers;

/// <summary>
/// Handles merchant authentication including registration, email verification, login, and token management.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting("auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Registers a new merchant account.
    /// </summary>
    /// <remarks>
    /// After successful registration, an OTP code will be sent to the provided email address for verification.
    /// </remarks>
    /// <param name="request">The registration details including email, password, and full name.</param>
    /// <returns>A success message prompting the user to verify their email.</returns>
    /// <response code="201">Registration successful. OTP sent to email.</response>
    /// <response code="400">Invalid registration details or email already in use.</response>
    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        _logger.LogInformation("Registration attempt for email: {Email}, role: {Role}", request.Email, request.Role);
        await _authService.RegisterAsync(request);
        _logger.LogInformation("Registration successful for email: {Email}", request.Email);
        return StatusCode(StatusCodes.Status201Created, new { message = "Registration successful. Please verify your email with the code sent to your inbox." });
    }

    /// <summary>
    /// Verifies a merchant's email address using the OTP code sent during registration.
    /// </summary>
    /// <param name="request">The email address and OTP code to verify.</param>
    /// <returns>A success message confirming email verification.</returns>
    /// <response code="200">Email verified successfully.</response>
    /// <response code="400">Invalid or expired OTP code.</response>
    [HttpPost("verify-email")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailRequest request)
    {
        await _authService.VerifyEmailAsync(request);
        return Ok(new { message = "Email verified successfully. You can now login." });
    }

    /// <summary>
    /// Authenticates a merchant and returns a JWT access token along with a refresh token.
    /// </summary>
    /// <param name="request">The login credentials (email and password).</param>
    /// <returns>A JWT access token and a refresh token.</returns>
    /// <response code="200">Login successful. Returns access and refresh tokens.</response>
    /// <response code="400">Email not verified.</response>
    /// <response code="403">Invalid email or password.</response>
    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        _logger.LogInformation("Login attempt for email: {Email}", request.Email);
        var response = await _authService.LoginAsync(request);
        _logger.LogInformation("Login successful for email: {Email}", request.Email);
        return Ok(new { message = "Login successful.", data = response });
    }

    /// <summary>
    /// Issues a new JWT access token using a valid refresh token.
    /// </summary>
    /// <param name="request">The expired or near-expiry refresh token.</param>
    /// <returns>A new JWT access token and a rotated refresh token.</returns>
    /// <response code="200">Token refreshed successfully.</response>
    /// <response code="403">Refresh token is invalid or has expired.</response>
    [HttpPost("refresh")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        _logger.LogInformation("Token refresh attempt");
        var response = await _authService.RefreshTokenAsync(request.RefreshToken);
        _logger.LogInformation("Token refresh successful");
        return Ok(new { message = "Token refreshed successfully.", data = response });
    }

    /// <summary>
    /// Revokes the provided refresh token, effectively logging the merchant out.
    /// </summary>
    /// <param name="request">The refresh token to revoke.</param>
    /// <returns>No content on success.</returns>
    /// <response code="200">Logged out successfully.</response>
    /// <response code="403">Refresh token is invalid or already revoked.</response>
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequest request)
    {
        _logger.LogInformation("Logout attempt");
        await _authService.LogoutAsync(request.RefreshToken);
        _logger.LogInformation("Logout successful");
        return Ok(new { message = "Logged out successfully." });
    }
}