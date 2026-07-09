using Infrastructure.FileStorage;
using Infrastructure.Interfaces;
using Infrastructure.Services;
using Infrastructure.Settings;
using Microsoft.Extensions.Options;

namespace WebApp.ExtensionMethods;

public static class StripeRegister
{
    public static void AddStripeServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Register Stripe settings from appsettings.json
        services.Configure<StripeSetting>(configuration.GetSection("StripeSettings"));
        
        // Register Stripe payment service
        services.AddScoped<IStripePaymentService, StripePaymentService>();
    }
}
