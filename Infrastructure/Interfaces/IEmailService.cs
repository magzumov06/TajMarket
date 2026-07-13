namespace Infrastructure.Interfaces;

public interface IEmailService
{
    Task SendOtpAsync(string email, string code);
}