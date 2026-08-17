using Application.Common.Interfaces;
using Application.Common.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Twilio;
using Twilio.Types;

namespace Infrastructure.Services;

public class TwilioSmsService : ISmsService
{
    private readonly SmsSettings _settings;
    private readonly ILogger<TwilioSmsService> _logger;

    public TwilioSmsService(IOptions<SmsSettings> options, ILogger<TwilioSmsService> logger)
    {
        _settings = options.Value;
        _logger = logger;
        TwilioClient.Init(_settings.AccountSid, _settings.AuthToken);
    }

    public async Task SendSmsAsync(string phoneNumber, string message)
    {
        try
        {
            _logger.LogInformation("Sending SMS to {PhoneNumber}", phoneNumber);

            var normalizedNumber = NormalizePhoneNumber(phoneNumber);

            await MessageResource.CreateAsync(
                body: message,
                from: new PhoneNumber(_settings.FromPhoneNumber),
                to: new PhoneNumber(normalizedNumber));

            _logger.LogInformation("SMS sent successfully to {PhoneNumber}", phoneNumber);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send SMS to {PhoneNumber}", phoneNumber);
        }
    }

    private static string NormalizePhoneNumber(string phoneNumber)
    {
        var digits = new string(phoneNumber.Where(char.IsDigit).ToArray());

        if (digits.StartsWith("992"))
            return "+" + digits;

        if (digits.StartsWith("0"))
            return "+992" + digits[1..];

        return "+992" + digits;
    }
}