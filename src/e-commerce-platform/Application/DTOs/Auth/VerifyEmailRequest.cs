namespace e_commerce_platform.Application.DTOs.Auth;

public class VerifyEmailRequest
{
    public string Email { get; set; } = string.Empty;
    public string Otp { get; set; } = string.Empty;
}
