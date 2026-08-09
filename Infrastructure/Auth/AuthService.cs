using System.Net;
using Application.Common.Interfaces;
using Application.Features.Auth.DTOs;
using Domain.Entities.UserEntity;
using Domain.Responses;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Auth;

public class AuthService(
    UserManager<User> userManager,
    ITokenService tokenService,
    IOtpService otpService,
    ILogger<AuthService> logger) : IAuthService
{
    private const string DefaultRole = "Customer";


    public async Task<Response<AuthResponseDto>> RegisterAsync(RegisterDto dto)
    {
        try
        {
            dto.Email = dto.Email.Trim().ToLowerInvariant();


            logger.LogInformation(
                "Registration attempt for {Email}",
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
                    createResult.Errors.Select(x => x.Description));


                logger.LogWarning(
                    "User registration failed {Errors}",
                    errors);


                return new Response<AuthResponseDto>(
                    HttpStatusCode.BadRequest,
                    errors);
            }


            var roleResult = await userManager.AddToRoleAsync(
                user,
                DefaultRole);


            if (!roleResult.Succeeded)
            {
                var errors = string.Join(
                    "; ",
                    roleResult.Errors.Select(x => x.Description));


                logger.LogError(
                    "Role assignment failed {Errors}",
                    errors);


                await userManager.DeleteAsync(user);


                return new Response<AuthResponseDto>(
                    HttpStatusCode.BadRequest,
                    errors);
            }


            await otpService.ResendOtpAsync(user.Email!);


            logger.LogInformation(
                "User registered successfully {UserId}",
                user.Id);


            return new Response<AuthResponseDto>(
                HttpStatusCode.OK,
                "Рамзи тасдиқ ба почтаи электронӣ фиристода шуд.");
        }
        catch(Exception ex)
        {
            logger.LogError(
                ex,
                "Error during registration {Email}",
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
            dto.Email = dto.Email.Trim().ToLowerInvariant();


            logger.LogInformation(
                "Login attempt for {Email}",
                dto.Email);


            const string genericError =
                "Почтаи электронӣ ё парол нодуруст аст";


            var user = await userManager.FindByEmailAsync(dto.Email);


            if (user == null || !user.IsActive)
            {
                logger.LogWarning(
                    "Login failed. User not found or inactive");


                return new Response<AuthResponseDto>(
                    HttpStatusCode.Unauthorized,
                    genericError);
            }


            if (!await userManager.IsEmailConfirmedAsync(user))
            {
                return new Response<AuthResponseDto>(
                    HttpStatusCode.Unauthorized,
                    "Почтаи электронии шумо тасдиқ карда нашудааст.");
            }


            if (await userManager.IsLockedOutAsync(user))
            {
                return new Response<AuthResponseDto>(
                    HttpStatusCode.Unauthorized,
                    "Ҳисоби шумо қулф шудааст.");
            }


            var passwordValid =
                await userManager.CheckPasswordAsync(
                    user,
                    dto.Password);


            if (!passwordValid)
            {
                await userManager.AccessFailedAsync(user);


                logger.LogWarning(
                    "Wrong password for user {UserId}",
                    user.Id);


                return new Response<AuthResponseDto>(
                    HttpStatusCode.Unauthorized,
                    genericError);
            }


            await userManager.ResetAccessFailedCountAsync(user);


            var roles = await userManager.GetRolesAsync(user);


            var (token, expiresAt) =
                tokenService.GenerateToken(
                    user,
                    roles);


            var response = new AuthResponseDto(
                token,
                expiresAt,
                user.Id,
                user.FullName,
                user.Email ?? string.Empty,
                roles.ToList());


            logger.LogInformation(
                "Login successful {UserId}",
                user.Id);


            return new Response<AuthResponseDto>(
                response,
                "Вуруд бомуваффақият анҷом ёфт.");
        }
        catch(Exception ex)
        {
            logger.LogError(
                ex,
                "Login error");


            return new Response<AuthResponseDto>(
                HttpStatusCode.InternalServerError,
                "Internal server error");
        }
    }



    public async Task<Response<string>> ChangePassword(
        ChangePasswordDto changePasswordDto,
        int userId)
    {
        try
        {
            logger.LogInformation(
                "Changing password for {UserId}",
                userId);


            var user =
                await userManager.FindByIdAsync(
                    userId.ToString());


            if (user == null)
            {
                return new Response<string>(
                    HttpStatusCode.NotFound,
                    "User not found");
            }


            var result =
                await userManager.ChangePasswordAsync(
                    user,
                    changePasswordDto.OldPassword,
                    changePasswordDto.Password);


            if (!result.Succeeded)
            {
                return new Response<string>(
                    HttpStatusCode.BadRequest,
                    "Your password not changed");
            }


            return new Response<string>(
                HttpStatusCode.OK,
                "Your password has been changed");
        }
        catch(Exception ex)
        {
            logger.LogError(
                ex,
                "Password change error {UserId}",
                userId);


            return new Response<string>(
                HttpStatusCode.InternalServerError,
                "Internal server error");
        }
    }
}