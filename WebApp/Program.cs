using Domain.Entities.UserEntity;
using Infrastructure.Data;
using Infrastructure.Settings;
using Microsoft.AspNetCore.Identity;
using Serilog;
using WebApp.ExtensionMethods;

var builder = WebApplication.CreateBuilder(args);

//Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteToServiceFiles("Logs")
    .CreateLogger();

builder.Host.UseSerilog();


builder.Services.Configure<CloudinarySetting>(
    builder.Configuration.GetSection("CloudinarySettings"));

//DataContext
builder.Services.AddDataContext(builder.Configuration);

builder.Services.AddIdentity<User, IdentityRole<int>>()
    .AddEntityFrameworkStores<DataContext>()
    .AddDefaultTokenProviders();

//Swagger
builder.Services.RegisterSwagger();

//Stripe Payment Service
builder.Services.AddStripeServices(builder.Configuration);

//Application Services
builder.Services.AddApplicationServices();

builder.Host.UseSerilog();


builder.Services.AddHttpContextAccessor();







builder.Services.AddAuthorization(opt => 
{ 
    opt.AddPolicy("AdminOnly", p => p.RequireRole("Admin"));
    opt.AddPolicy("SellerOnly", p => p.RequireRole("Seller", "Admin"));
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

try
{
    Log.Information("Starting web host");
    var app = builder.Build();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseAuthentication();
    app.UseAuthorization();
    app.UseHttpsRedirection();
    app.MapControllers();
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
