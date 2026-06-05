using System.Net;
using System.Net.Mail;
using e_commerce_platform.Application.Interfaces;
using e_commerce_platform.Domain.Interfaces;
using e_commerce_platform.Settings;
using Microsoft.Extensions.Options;

namespace e_commerce_platform.Application.Services;

public class EmailService : IEmailService
{
    private readonly MailSettings _mailSettings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<MailSettings> mailSettings, ILogger<EmailService> _logger)
    {
        _mailSettings = mailSettings.Value;
        this._logger = _logger;
    }

    public async Task SendOtpEmailAsync(string email, string otp)
    {
        var mailMessage = new MailMessage
        {
            From = new MailAddress(_mailSettings.FromEmail, _mailSettings.FromName),
            Subject = "Verify Your Email Address",
            Body = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #eee; border-radius: 8px;'>
                    <h2 style='color: #333; text-align: center;'>Welcome to E-Commerce Platform!</h2>
                    <p style='font-size: 16px; color: #555;'>Thank you for registering. Please verify your email using the 6-digit One-Time Password (OTP) below:</p>
                    <div style='background-color: #f9f9f9; border: 1px dashed #ccc; padding: 15px; text-align: center; margin: 20px 0; border-radius: 4px;'>
                        <span style='font-size: 32px; font-weight: bold; letter-spacing: 5px; color: #007bff;'>{otp}</span>
                    </div>
                    <p style='font-size: 14px; color: #777; text-align: center;'>This code is valid for 15 minutes. If you did not request this code, please ignore this email.</p>
                </div>",
            IsBodyHtml = true
        };
        mailMessage.To.Add(email);

        try
        {
            using var smtpClient = new SmtpClient(_mailSettings.Host, _mailSettings.Port)
            {
                Credentials = new NetworkCredential(_mailSettings.Username, _mailSettings.Password),
                EnableSsl = true
            };

            await smtpClient.SendMailAsync(mailMessage);
            _logger.LogInformation("OTP email successfully sent to {Email} via SMTP.", email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send OTP email to {Email} via SMTP. Falling back to local logging.", email);
            
            _logger.LogInformation("\n==================================================" +
                                   "\n[FALLBACK LOGGING] EMAILING OTP TO: {Email}" +
                                   "\nYOUR ONE-TIME PASSWORD IS: {Otp}" +
                                   "\nVALID FOR: 15 MINUTES" +
                                   "\n==================================================\n", email, otp);
        }
    }
}
