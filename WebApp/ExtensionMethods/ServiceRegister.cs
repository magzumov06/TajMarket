using Application.Common.Interfaces;
using Infrastructure.Auth;
using Infrastructure.Background;
using Infrastructure.Data;
using Infrastructure.FileStorage;
using Infrastructure.Interfaces;
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

        
        services.AddScoped<ICartService, CartService>();
        services.AddScoped<ICouponService, CouponService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IReviewService, ReviewService>();
        services.AddScoped<IWishlistService, WishlistService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ISellerService, SellerService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IOtpService, OtpService>();
        services.AddScoped<IUnconfirmedUserCleanupService, UnconfirmedUserCleanupService>();
        services.AddScoped<ICourierService, CourierService>();  
    }
    
}
