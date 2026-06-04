using System.Security.Cryptography;
using e_commerce_platform.Domain.Entities;
using e_commerce_platform.Domain.Enums;
using e_commerce_platform.Application.DTOs.Auth;
using e_commerce_platform.helpers;
using e_commerce_platform.Infrastructure.Data;
using e_commerce_platform.Application.Interfaces;
using e_commerce_platform.Domain.Interfaces;
using e_commerce_platform.Settings;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace e_commerce_platform.Application.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITokenService _tokenService;
    private readonly IEmailService _emailService;
    private readonly AppDbContext _context;
    private readonly JwtSettings _jwtSettings;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        ITokenService tokenService,
        IEmailService emailService,
        AppDbContext context,
        IOptions<JwtSettings> jwtSettings,
        ILogger<AuthService> logger)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _emailService = emailService;
        _context = context;
        _jwtSettings = jwtSettings.Value;
        _logger = logger;
    }

    public async Task RegisterAsync(RegisterRequest request)
    {
        _logger.LogInformation("Processing registration for email: {Email}", request.Email);

        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
        {
            _logger.LogWarning("Registration rejected - email already exists: {Email}", request.Email);
            throw new InvalidOperationException("Email is already registered.");
        }

        if (!Enum.TryParse<UserRole>(request.Role, true, out var role))
        {
            _logger.LogWarning("Registration rejected - invalid role: {Role}", request.Role);
            throw new ArgumentException("Invalid role. Must be 'Merchant'.");
        }

        if (role == UserRole.Admin)
        {
            _logger.LogWarning("Registration rejected - attempted Admin registration: {Email}", request.Email);
            throw new UnauthorizedAccessException("Cannot register as Admin.");
        }

        if (role != UserRole.Merchant)
        {
            throw new ArgumentException("Invalid role. Must be 'Merchant'.");
        }

        ApplicationUser user = new Merchant { FullName = request.FullName, Email = request.Email, UserName = request.Email };

        
        var otpCode = CodeGenerator.Generate6DigitOtp();
        user.OtpCode = otpCode;
        user.OtpExpiry = DateTime.UtcNow.AddMinutes(15);
        user.EmailConfirmed = false; 

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            _logger.LogError("User creation failed for {Email}: {Errors}", request.Email, errors);
            throw new InvalidOperationException(errors);
        }

        _logger.LogInformation("User created successfully: {UserId}, sending OTP to {Email}", user.Id, request.Email);

        await _emailService.SendOtpEmailAsync(request.Email, otpCode);
    }

    public async Task VerifyEmailAsync(VerifyEmailRequest request)
    {
        _logger.LogInformation("Processing email verification for: {Email}", request.Email);

        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            _logger.LogWarning("Verification failed - user not found: {Email}", request.Email);
            throw new UnauthorizedAccessException("Invalid code or email.");
        }

        if (user.EmailConfirmed)
        {
            _logger.LogInformation("Verification redundant - email is already confirmed for {Email}", request.Email);
            return;
        }

        if (user.OtpCode != request.Otp || user.OtpExpiry == null || user.OtpExpiry < DateTime.UtcNow)
        {
            _logger.LogWarning("Verification failed - code mismatch or expired for {Email}", request.Email);
            throw new UnauthorizedAccessException("Invalid code or email.");
        }

        user.EmailConfirmed = true;
        user.OtpCode = null;
        user.OtpExpiry = null;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            _logger.LogError("User confirmation update failed for {Email}: {Errors}", request.Email, errors);
            throw new InvalidOperationException("Failed to confirm email.");
        }

        _logger.LogInformation("Email verified successfully for: {Email}", request.Email);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        _logger.LogInformation("Processing login for email: {Email}", request.Email);

        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            _logger.LogWarning("Login failed - user not found: {Email}", request.Email);
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        var isPasswordValid = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!isPasswordValid)
        {
            _logger.LogWarning("Login failed - invalid password for user: {UserId}", user.Id);
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        if (!user.EmailConfirmed)
        {
            _logger.LogWarning("Login failed - email not verified for user: {UserId}", user.Id);
            throw new UnauthorizedAccessException("Please verify your email before logging in.");
        }

        _logger.LogInformation("Login successful for user: {UserId}", user.Id);
        return await GenerateAuthResponseAsync(user);
    }

    public async Task<AuthResponse> RefreshTokenAsync(string refreshToken)
    {
        _logger.LogInformation("Processing token refresh");

        var storedToken = await _context.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

        if (storedToken == null || !storedToken.IsActive)
        {
            _logger.LogWarning("Token refresh failed - token invalid or expired");
            throw new UnauthorizedAccessException("Invalid or expired refresh token.");
        }

        storedToken.RevokedAt = DateTime.UtcNow;

        _logger.LogInformation("Token refresh successful for user: {UserId}", storedToken.User.Id);
        return await GenerateAuthResponseAsync(storedToken.User);
    }

    public async Task LogoutAsync(string refreshToken)
    {
        _logger.LogInformation("Processing logout");

        var storedToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

        if (storedToken == null)
            throw new UnauthorizedAccessException("Invalid refresh token.");

        storedToken.RevokedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Logout successful - token revoked for user: {UserId}", storedToken.UserId);
    }

    private async Task<AuthResponse> GenerateAuthResponseAsync(ApplicationUser user)
    {
        var accessToken = _tokenService.GenerateAccessToken(user);
        var refreshToken = _tokenService.GenerateRefreshToken();

        var refreshTokenEntity = new RefreshToken
        {
            UserId = user.Id,
            Token = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays),
            CreatedAt = DateTime.UtcNow
        };

        await _context.RefreshTokens.AddAsync(refreshTokenEntity);
        await _context.SaveChangesAsync();

        _logger.LogDebug("Auth tokens generated for user: {UserId}", user.Id);

        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            AccessTokenExpiration = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
            User = new UserDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email ?? string.Empty,
                Role = user.Role.ToString()
            }
        };
    }

   
}
