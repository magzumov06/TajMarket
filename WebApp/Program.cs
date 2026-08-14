using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Application.Common.Settings;
using Domain.Entities.UserEntity;
using Hangfire;
using Hangfire.PostgreSql;
using Infrastructure.Background;
using Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using WebApp.Auth;
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

//Settings
builder.Services.Configure<CloudinarySetting>(
    builder.Configuration.GetSection("CloudinarySettings"));

builder.Services.Configure<ShippingSetting>(
    builder.Configuration.GetSection("ShippingSettings"));

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));

builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection("EmailSettings"));

builder.Services.Configure<AdminSeedSettings>(builder.Configuration.GetSection("AdminSeed"));

builder.Services.Configure<HangfireDashboardSettings>(
    builder.Configuration.GetSection("HangfireDashboard"));

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

//JWT Authentication
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

        options.Events = new JwtBearerEvents
        {
            //SignalR: гирифтани JWT аз query string барои дархостҳои /hubs/*
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;

                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                    context.Token = accessToken;

                return Task.CompletedTask;
            },

            //Token Revocation: рад кардани token-ҳои бекоршуда (logout) ё блокшуда (Admin)
            OnTokenValidated = async context =>
            {
                var jti = context.Principal?.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
                var iatStr = context.Principal?.FindFirst(JwtRegisteredClaimNames.Iat)?.Value;
                var userIdStr = context.Principal?.FindFirst("sub")?.Value
                                ?? context.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                                ?? context.Principal?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

                if (jti == null || iatStr == null || userIdStr == null)
                {
                    context.Fail("Invalid token");
                    return;
                }

                var dbContext = context.HttpContext.RequestServices
                    .GetRequiredService<Application.Common.Interfaces.IApplicationDbContext>();

                var isRevoked = await dbContext.RevokedTokens.AnyAsync(rt => rt.Jti == jti);

                if (isRevoked)
                {
                    context.Fail("Token has been revoked");
                    return;
                }

                var iat = DateTimeOffset.FromUnixTimeSeconds(long.Parse(iatStr)).UtcDateTime;
                var userId = int.Parse(userIdStr);

                var user = await dbContext.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null || !user.IsActive || iat < user.TokenValidFrom)
                {
                    context.Fail("Token is no longer valid");
                }
            }
        };
    });

builder.Host.UseSerilog();

builder.Services.AddHttpContextAccessor();

//Authorization Policies
builder.Services.AddAuthorization(opt =>
{
    opt.AddPolicy("AdminOnly", p => p.RequireRole("Admin"));
    opt.AddPolicy("SellerOnly", p => p.RequireRole("Seller", "Admin"));
    opt.AddPolicy("CourierOnly", p => p.RequireRole("Courier", "Admin"));
});

//SignalR
builder.Services.AddSignalR();
builder.Services.AddSingleton<IUserIdProvider, Infrastructure.Realtime.CustomUserIdProvider>();

//CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("SignalRPolicy", policy =>
    {
        policy.WithOrigins("https://frontend.com", "http://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();

try
{
    Log.Information("Starting web host");
    var app = builder.Build();

    //Swagger
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    //CORS
    app.UseCors("SignalRPolicy");

    //Authentication/Authorization
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseHttpsRedirection();

    //Controllers
    app.MapControllers();

    //SignalR Hubs
    app.MapHub<Infrastructure.Realtime.CourierHub>("/hubs/couriers");
    app.MapHub<Infrastructure.Realtime.NotificationHub>("/hubs/notifications");

    //Hangfire
    var hangfireSettings = app.Services.GetRequiredService<IOptions<HangfireDashboardSettings>>().Value;

    app.UseHangfireDashboard("/hangfire", new DashboardOptions
    {
        Authorization = new[]
        {
            new HangfireBasicAuthAuthorizationFilter(hangfireSettings.Username, hangfireSettings.Password)
        }
    });
    
    //DB Migration + Seed
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        try
        {
            var userManager = services.GetRequiredService<UserManager<User>>();
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole<int>>>();
            var data = services.GetRequiredService<DataContext>();
            await Seed.SeedRole(roleManager);
            var adminSettings = services.GetRequiredService<IOptions<AdminSeedSettings>>().Value;
            await Seed.SeedAdmin(userManager, roleManager, adminSettings);
            await data.Database.MigrateAsync();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Database migration/seed failed");
            throw;
        }
    }

    //Background Jobs
    RecurringJob.AddOrUpdate<IUnconfirmedUserCleanupService>(
        "delete-unconfirmed-users",
        service => service.DeleteOldUnconfirmedUsersAsync(),
        Cron.Daily);
    
    RecurringJob.AddOrUpdate<IRevokedTokenCleanupService>(
        "cleanup-revoked-tokens",
        service => service.DeleteExpiredTokensAsync(),
        Cron.Daily);

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
}