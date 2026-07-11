using Infrastructure.Interfaces;
using Infrastructure.Services;
using Infrastructure.Settings;

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
