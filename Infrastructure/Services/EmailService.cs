using System.Net;
using System.Net.Mail;
using Infrastructure.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public class EmailService(
    IConfiguration configuration,
    ILogger<EmailService> logger) : IEmailService
{
    public async Task SendOtpAsync(string email, string code)
    {
        try
        {
            logger.LogInformation("Sending OTP email to {Email}", email);

            var smtpHost = configuration["EmailSettings:SmtpHost"];
            var smtpPortStr = configuration["EmailSettings:SmtpPort"];
            var smtpEmail = configuration["EmailSettings:Email"];
            var smtpPassword = configuration["EmailSettings:Password"];
            var displayName = configuration["EmailSettings:DisplayName"];

            if (string.IsNullOrEmpty(smtpHost) ||
                string.IsNullOrEmpty(smtpPortStr) ||
                string.IsNullOrEmpty(smtpEmail) ||
                string.IsNullOrEmpty(smtpPassword))
            {
                logger.LogError("Email settings are missing");
                throw new InvalidOperationException("Email settings are not configured");
            }

            if (!int.TryParse(smtpPortStr, out var smtpPort))
            {
                logger.LogError("Invalid SMTP port configuration: {SmtpPort}", smtpPortStr);
                throw new InvalidOperationException("Invalid SMTP port configuration");
            }

            using var client = new SmtpClient(smtpHost, smtpPort)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(smtpEmail, smtpPassword)
            };

            using var mail = new MailMessage
            {
                From = new MailAddress(smtpEmail, displayName),
                Subject = "TajMarket OTP Verification Code",
                Body = $"""
                        Hello,

                        Your TajMarket verification code is:

                        {code}

                        This code will expire soon.

                        If you did not request this code, please ignore this email.

                        Thank you,
                        TajMarket Team
                        """,
                IsBodyHtml = false
            };

            mail.To.Add(email);

            await client.SendMailAsync(mail);

            logger.LogInformation("OTP email successfully sent to {Email}", email);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send OTP email to {Email}", email);
            throw;
        }
    }
}