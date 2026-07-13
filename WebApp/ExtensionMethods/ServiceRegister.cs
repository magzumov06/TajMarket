using Infrastructure.Auth;
using Infrastructure.FileStorage;
using Infrastructure.Interfaces;
using Infrastructure.Services;

namespace WebApp.ExtensionMethods;

public static class ServiceRegister
{
    public static void AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<ICartService, CartService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<ICouponService, CouponService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IReviewService, ReviewService>();
        services.AddScoped<IWishlistService, WishlistService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IFileStorageService, CloudinaryFileStorageService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ISellerService, SellerService>();
        services.AddScoped<IAddressService, AddressService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IOtpService, OtpService>();

    }
    
}
