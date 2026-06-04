namespace e_commerce_platform.Application.Interfaces;

public interface IEmailService
{
    Task SendOtpEmailAsync(string email, string otp);
}
