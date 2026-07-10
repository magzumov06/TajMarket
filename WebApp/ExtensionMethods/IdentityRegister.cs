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
                opt.Password.RequireNonAlphanumeric = false;
                opt.Password.RequireUppercase = false;
                opt.Password.RequireLowercase = false;
                opt.Password.RequireDigit = false;
                opt.User.RequireUniqueEmail = true;
            })
            .AddRoles<IdentityRole<int>>()
            .AddEntityFrameworkStores<DataContext>()
            .AddSignInManager();
    }
}