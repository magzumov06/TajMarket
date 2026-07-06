using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace WebApp.ExtensionMethods;

public static class DataContextRegister
{
    public static void AddDataContext(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<DataContext>(opt=>
            opt.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));
    }
}