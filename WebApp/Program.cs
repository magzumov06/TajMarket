using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Domain.Entities.UserEntity;
using Hangfire;
using Hangfire.PostgreSql;
using Infrastructure.Background;
using Infrastructure.Data;
using Infrastructure.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using WebApp.ExtensionMethods;

JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

var builder = WebApplication.CreateBuilder(args);

//Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteToServiceFiles("Logs")
    .CreateLogger();

builder.Services.Configure<CloudinarySetting>(
    builder.Configuration.GetSection("CloudinarySettings"));

builder.Services.Configure<ShippingSetting>(               
    builder.Configuration.GetSection("ShippingSettings"));

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));

builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection("EmailSettings"));

//DataContext
builder.Services.AddDataContext(builder.Configuration);


//Swagger
builder.Services.RegisterSwagger();

//Stripe Payment Service
builder.Services.AddStripeServices(builder.Configuration);

//Application Services
builder.Services.AddApplicationServices();

//Identity
builder.Services.RegisterIdentity();

//Hangfire
builder.Services.AddHangfire(config =>
{
    config.UsePostgreSqlStorage(
        builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddHangfireServer();

builder.Services.AddAuthentication(options => {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options => {
        options.TokenValidationParameters = new TokenValidationParameters {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
            ValidAudience = builder.Configuration["JwtSettings:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Key"]!))
        };
    });




builder.Host.UseSerilog();


builder.Services.AddHttpContextAccessor();


builder.Services.AddAuthorization(opt => 
{ 
    opt.AddPolicy("AdminOnly", p => p.RequireRole("Admin"));
    opt.AddPolicy("SellerOnly", p => p.RequireRole("Seller", "Admin"));
    opt.AddPolicy("CourierOnly", p => p.RequireRole("Courier" , "Admin"));  
});

builder.Services.AddSignalR();                     
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
    app.MapHub<Infrastructure.Realtime.CourierHub>("/hubs/couriers");  
    app.UseHangfireDashboard("/hangfire");
    
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        try
        {
            var userManager = services.GetRequiredService<UserManager<User>>();
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole<int>>>();
            var data =  services.GetRequiredService<DataContext>();
            await Seed.SeedRole(roleManager);
            await Seed.SeedAdmin(userManager, roleManager);
            await data.Database.MigrateAsync();
        }
        catch(Exception ex)
        {
            Log.Error(ex, "Database migration/seed failed");
            throw;
        }
    }
    RecurringJob.AddOrUpdate<IUnconfirmedUserCleanupService>(
        "delete-unconfirmed-users",
        service => service.DeleteOldUnconfirmedUsersAsync(),
        Cron.Daily);
    
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
}