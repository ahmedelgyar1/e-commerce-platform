using e_commerce_platform.DTOs.Auth;
using e_commerce_platform.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace e_commerce_platform.Controllers;

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

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        try
        {
            _logger.LogInformation("Registration attempt for email: {Email}, role: {Role}", request.Email, request.Role);
            await _authService.RegisterAsync(request);
            _logger.LogInformation("Registration successful for email: {Email}", request.Email);
            return Ok(new { message = "Registration successful. Please verify your email with the code sent to your inbox." });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Registration failed for email: {Email} - {Error}", request.Email, ex.Message);
            return BadRequest(new { error = "Registration failed. Please check your details and try again." });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Registration failed - invalid argument: {Error}", ex.Message);
            return BadRequest(new { error = "Invalid registration details." });
        }
        catch (UnauthorizedAccessException)
        {
            _logger.LogWarning("Registration blocked - attempted Admin registration for email: {Email}", request.Email);
            return BadRequest(new { error = "Invalid registration details." });
        }
    }

    [HttpPost("verify-email")]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailRequest request)
    {
        try
        {
            await _authService.VerifyEmailAsync(request);
            return Ok(new { message = "Email verified successfully. You can now login." });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Email verification failed for {Email} - {Error}", request.Email, ex.Message);
            return BadRequest(new { error = "Invalid code or email." });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError("Email verification process error: {Error}", ex.Message);
            return StatusCode(500, new { error = "An error occurred during verification." });
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            _logger.LogInformation("Login attempt for email: {Email}", request.Email);
            var response = await _authService.LoginAsync(request);
            _logger.LogInformation("Login successful for email: {Email}", request.Email);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Login failed for email: {Email} - {Error}", request.Email, ex.Message);
            
            // If the user's email is not confirmed, let them know explicitly so they can verify it.
            if (ex.Message.Contains("verify your email"))
            {
                return BadRequest(new { error = "Please verify your email before logging in." });
            }

            return Unauthorized(new { error = "Invalid credentials." });
        }
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        try
        {
            _logger.LogInformation("Token refresh attempt");
            var response = await _authService.RefreshTokenAsync(request.RefreshToken);
            _logger.LogInformation("Token refresh successful");
            return Ok(response);
        }
        catch (UnauthorizedAccessException)
        {
            _logger.LogWarning("Token refresh failed - invalid or expired refresh token");
            return Unauthorized(new { error = "Session expired. Please login again." });
        }
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequest request)
    {
        try
        {
            _logger.LogInformation("Logout attempt");
            await _authService.LogoutAsync(request.RefreshToken);
            _logger.LogInformation("Logout successful");
            return Ok(new { message = "Logged out successfully." });
        }
        catch (UnauthorizedAccessException)
        {
            _logger.LogWarning("Logout failed - invalid refresh token");
            return Unauthorized(new { error = "Session expired. Please login again." });
        }
    }
}
