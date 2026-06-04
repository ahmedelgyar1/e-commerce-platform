using e_commerce_platform.Application.DTOs.Auth;

namespace e_commerce_platform.Application.Interfaces;

public interface IAuthService
{
    Task RegisterAsync(RegisterRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task VerifyEmailAsync(VerifyEmailRequest request);
    Task<AuthResponse> RefreshTokenAsync(string refreshToken);
    Task LogoutAsync(string refreshToken);
}
