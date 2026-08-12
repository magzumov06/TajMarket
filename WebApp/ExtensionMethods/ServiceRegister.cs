using Application.Common.Interfaces;
using Infrastructure.Auth;
using Infrastructure.Background;
using Infrastructure.Data;
using Infrastructure.FileStorage;
using Infrastructure.Identity;
using Infrastructure.Interfaces;
using Infrastructure.Realtime;
using Infrastructure.Services;

namespace WebApp.ExtensionMethods;

public static class ServiceRegister
{
    public static void AddApplicationServices(this IServiceCollection services)
    {

        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(IApplicationDbContext).Assembly));

        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<DataContext>());
        services.AddScoped<IFileStorageService, CloudinaryFileStorageService>();

        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IOtpService, OtpService>();
        services.AddScoped<IUnconfirmedUserCleanupService, UnconfirmedUserCleanupService>();
        services.AddScoped<IStripePaymentService, StripePaymentService>();
        services.AddScoped<IRevokedTokenCleanupService, RevokedTokenCleanupService>();
        
        services.AddScoped<IRealtimeNotifier, SignalRRealtimeNotifier>();
    }
}
