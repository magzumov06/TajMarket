using System.Net;
using Domain.DTOs.AuthDto;
using Domain.Entities.UserEntity;
using Domain.Responses;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

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
            logger.LogInformation(
                "Registration attempt for email {Email}",
                dto.Email);


            var existing = await userManager.FindByEmailAsync(dto.Email);


            if (existing != null)
            {
                logger.LogWarning(
                    "Registration failed. Email already exists {Email}",
                    dto.Email);

                return new Response<AuthResponseDto>(
                    HttpStatusCode.Conflict,
                    "Корбар бо ин почтаи электронӣ аллакай сабт шудааст");
            }


            var user = new User
            {
                UserName = dto.Email,
                Email = dto.Email,
                FullName = dto.FullName,
                PhoneNumber = dto.PhoneNumber,
                CreatedAt = DateTime.UtcNow
            };


            var createResult = await userManager.CreateAsync(
                user,
                dto.Password);


            if (!createResult.Succeeded)
            {
                var errors = string.Join(
                    "; ",
                    createResult.Errors.Select(e => e.Description));


                logger.LogWarning(
                    "User registration failed for {Email}: {Errors}",
                    dto.Email,
                    errors);


                return new Response<AuthResponseDto>(
                    HttpStatusCode.BadRequest,
                    errors);
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


            logger.LogInformation(
                "User registered successfully {UserId} {Email}",
                user.Id,
                dto.Email);


            return new Response<AuthResponseDto>(
                response,
                "Бо муваффақият сабт шудед");
        }
        catch (Exception e)
        {
            logger.LogError(
                e,
                "Error during registration for email {Email}",
                dto.Email);

            return new Response<AuthResponseDto>(
                HttpStatusCode.InternalServerError,
                "Internal server error");
        }
    }


    public async Task<Response<AuthResponseDto>> LoginAsync(LoginDto dto)
    {
        try
        {
            logger.LogInformation(
                "Login attempt for email {Email}",
                dto.Email);


            var user = await userManager.FindByEmailAsync(dto.Email);


            if (user == null || !user.IsActive)
            {
                logger.LogWarning(
                    "Login failed. User not found or inactive {Email}",
                    dto.Email);

                return new Response<AuthResponseDto>(
                    HttpStatusCode.BadRequest,
                    "Почтаи электронӣ ё парол нодуруст аст");
            }


            var passwordValid = await userManager.CheckPasswordAsync(
                user,
                dto.Password);


            if (!passwordValid)
            {
                logger.LogWarning(
                    "Login failed. Wrong password for user {UserId}",
                    user.Id);

                return new Response<AuthResponseDto>(
                    HttpStatusCode.BadRequest,
                    "Почтаи электронӣ ё парол нодуруст аст");
            }


            var roles = await userManager.GetRolesAsync(user);


            var (token, expiresAt) = tokenService.GenerateToken(
                user,
                roles);


            var response = new AuthResponseDto(
                token,
                expiresAt,
                user.Id,
                user.FullName,
                user.Email!,
                roles.ToList());


            logger.LogInformation(
                "Login successful for user {UserId}",
                user.Id);


            return new Response<AuthResponseDto>(
                response,
                "Login Success");
        }
        catch (Exception e)
        {
            logger.LogError(
                e,
                "Error during login for email {Email}",
                dto.Email);

            return new Response<AuthResponseDto>(
                HttpStatusCode.InternalServerError,
                "Internal server error");
        }
    }
}