namespace e_commerce_platform.Services.Interfaces;

public interface IEmailService
{
    Task SendOtpEmailAsync(string email, string otp);
}
