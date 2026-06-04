namespace e_commerce_platform.DTOs.Auth;

public class VerifyEmailRequest
{
    public string Email { get; set; } = string.Empty;
    public string Otp { get; set; } = string.Empty;
}
