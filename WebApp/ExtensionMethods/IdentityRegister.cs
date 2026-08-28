using Domain.Entities.UserEntity;
using Infrastructure.Data;
using Microsoft.AspNetCore.Identity;

namespace WebApp.ExtensionMethods;

public static class IdentityRegister
{
    public static void RegisterIdentity(this IServiceCollection services)
    {
        services
            .AddIdentityCore<User>(opt =>
            {
                opt.Password.RequiredLength = 8;
                opt.Password.RequireNonAlphanumeric = true;
                opt.Password.RequireUppercase = true;
                opt.Password.RequireLowercase = false;
                opt.Password.RequireDigit = false;
                opt.User.RequireUniqueEmail = true;
                opt.Lockout.MaxFailedAccessAttempts = 5;
                opt.Lockout.AllowedForNewUsers = true;
                opt.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            })
            .AddRoles<IdentityRole<int>>()
            .AddEntityFrameworkStores<DataContext>()
            .AddSignInManager();
    }
}