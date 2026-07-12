using System.Net;
using Domain.DTOs.AuthDto;
using Domain.Entities.UserEntity;
using Domain.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Serilog;

namespace Infrastructure.Auth;

public class AuthService(
    UserManager<User> userManager,
    ITokenService tokenService,
    ILogger<AuthService> logger) : IAuthService
{
    private const string DefaultRole = "Customer";


    public async Task<Response<AuthResponseDto>> RegisterAsync(RegisterDto dto)
    {
        try
        {
            logger.LogInformation("Registration attempt for email {Email}", dto.Email);
            
            var existing = await userManager.FindByEmailAsync(dto.Email);
            
            if (existing != null)
            {
                logger.LogWarning("Registration failed. Email already exists {Email}", dto.Email);

                return new Response<AuthResponseDto>(HttpStatusCode.Conflict, "Корбар бо ин почтаи электронӣ аллакай сабт шудааст");
            }
            
            var user = new User
            {
                UserName = dto.Email,
                Email = dto.Email,
                FullName = dto.FullName,
                PhoneNumber = dto.PhoneNumber,
                CreatedAt = DateTime.UtcNow
            };
            
            var createResult = await userManager.CreateAsync(user, dto.Password);
            
            if (!createResult.Succeeded)
            {
                var errors = string.Join("; ", createResult.Errors.Select(e => e.Description));
                
                logger.LogWarning("User registration failed for {Email}: {Errors}", dto.Email, errors);
                
                return new Response<AuthResponseDto>(HttpStatusCode.BadRequest, errors);
            }


            await userManager.AddToRoleAsync(
                user,
                DefaultRole);


            var (token, expiresAt) = tokenService.GenerateToken(
                user,
                [DefaultRole]);


            var response = new AuthResponseDto(
                token,
                expiresAt,
                user.Id,
                user.FullName,
                user.Email!,
                [DefaultRole]);


            logger.LogInformation("User registered successfully {UserId} {Email}", user.Id, dto.Email);
            
            return new Response<AuthResponseDto>(response, "Бо муваффақият сабт шудед");
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error during registration for email {Email}", dto.Email);

            return new Response<AuthResponseDto>(HttpStatusCode.InternalServerError, "Internal server error");
        }
    }


   public async Task<Response<AuthResponseDto>> LoginAsync(LoginDto dto) 
   { 
       try 
       { 
           dto.Email = dto.Email?.Trim() ?? string.Empty;
           
           const string genericError = "Почтаи электронӣ ё парол нодуруст аст"; 
           logger.LogInformation("Login attempt started.");
           
           var user = await userManager.FindByEmailAsync(dto.Email);
           
           if (user == null || !user.IsActive) 
           {
               logger.LogWarning("Login failed. User not found or inactive."); 
               return new Response<AuthResponseDto>(HttpStatusCode.Unauthorized, genericError);
               
           }
           
           if (userManager.Options.SignIn.RequireConfirmedEmail && !await userManager.IsEmailConfirmedAsync(user)) 
           {
               logger.LogWarning("Login failed. Email not confirmed for user {UserId}", user.Id); 
               return new Response<AuthResponseDto>(HttpStatusCode.Unauthorized, "Почтаи электронии шумо тасдиқ карда нашудааст.");
               
           }
           
           if (await userManager.IsLockedOutAsync(user)) 
           {
               var lockEnd = await userManager.GetLockoutEndDateAsync(user); 
               var remainingMinutes = lockEnd.HasValue 
                   ? Math.Max(0, Math.Ceiling((lockEnd.Value.UtcDateTime - DateTime.UtcNow).TotalMinutes)) 
                   : 0;
               
               logger.LogWarning("Login blocked. Account locked for user {UserId}", user.Id); 
               return new Response<AuthResponseDto>(HttpStatusCode.Unauthorized, 
                   $"Ҳисоби шумо қулф шудааст. Лутфан пас аз {remainingMinutes} дақиқа дубора кӯшиш кунед.");
               
           }
           
           var passwordValid = await userManager.CheckPasswordAsync(user, dto.Password);
           
           if (!passwordValid) 
           {
               await userManager.AccessFailedAsync(user); 
               var failedCount = await userManager.GetAccessFailedCountAsync(user); 
               var attemptsLeft = Math.Max(0, userManager.Options.Lockout.MaxFailedAccessAttempts - failedCount);
               
               logger.LogWarning("Login failed. Wrong password for user {UserId}. Attempts left: {AttemptsLeft}", user.Id, attemptsLeft);
               
               return new Response<AuthResponseDto>(HttpStatusCode.Unauthorized, genericError);
               
           }
           
           await userManager.ResetAccessFailedCountAsync(user); 
           var roles = await userManager.GetRolesAsync(user);
           
           var (token, expiresAt) = tokenService.GenerateToken(user, roles);
           
           var response = new AuthResponseDto( 
               token, 
               expiresAt, 
               user.Id, 
               user.FullName, 
               user.Email ?? string.Empty, 
               roles.ToList());
           
           logger.LogInformation("Login successful for user {UserId}", user.Id); 
           return new Response<AuthResponseDto>(response, "Вуруд бомуваффақият анҷом ёфт."); 
       }
       catch (Exception ex) 
       { 
           logger.LogError(ex, "Unexpected error occurred during login."); 
           return new Response<AuthResponseDto>(HttpStatusCode.InternalServerError, "Internal server error"); 
       }
}

    
    public async Task<Response<string>> ChangePassword(ChangePassword changePassword, int  userId)
    {
        try
        {
            logger.LogInformation("Changing password for userId {UserId}", userId);
            
            var user = userManager.Users.FirstOrDefault(x => x.Id == userId);
            
            if (user == null)
                return new Response<string>(HttpStatusCode.NotFound, "User not found");
            
            var res = await userManager.ChangePasswordAsync(user, changePassword.OldPassword,
                changePassword.Password);
            
            if (!res.Succeeded) 
                return new Response<string>(HttpStatusCode.BadRequest, "Your password not changed");
            
            return new Response<string>(HttpStatusCode.OK, "Your password has been changed");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during password change userId {UserId}", userId);
            return new Response<string>(HttpStatusCode.InternalServerError, $"Хатогӣ ҳангоми ивазкунии рамз: {ex.Message}");
        }
    }
}
