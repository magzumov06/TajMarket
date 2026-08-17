namespace Application.Common.Settings;

public class SmsSettings
{
    public string Provider { get; set; } = "Twilio";  
    public string AccountSid { get; set; } = null!;
    public string AuthToken { get; set; } = null!;
    public string FromPhoneNumber { get; set; } = null!;
}