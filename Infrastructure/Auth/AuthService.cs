using System.Net;
using Domain.DTOs.AuthDto;
using Domain.Entities;
using Domain.Entities.UserEntity;
using Domain.Responses;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Auth;

public class AuthService(
    UserManager<User> userManager,
    ITokenService tokenService) : IAuthService
{
    private const string DefaultRole = "Customer";

    public async Task<Response<AuthResponseDto>> RegisterAsync(RegisterDto dto)
    {
        var existing = await userManager.FindByEmailAsync(dto.Email);
        if (existing != null)
            return new Response<AuthResponseDto>(HttpStatusCode.Conflict,"Корбар бо ин почтаи электронӣ аллакай сабт шудааст");

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
            return new Response<AuthResponseDto>(HttpStatusCode.BadRequest,errors);
        }

        await userManager.AddToRoleAsync(user, DefaultRole);

        var (token, expiresAt) = tokenService.GenerateToken(user, [DefaultRole]);

        var response = new AuthResponseDto(token, expiresAt, user.Id, user.FullName, user.Email!, [DefaultRole]);
        return new Response<AuthResponseDto>(response, "Бо муваффақият сабт шудед");
    }

    public async Task<Response<AuthResponseDto>> LoginAsync(LoginDto dto)
    {
        var user = await userManager.FindByEmailAsync(dto.Email);
        if (user == null || !user.IsActive)
            return new Response<AuthResponseDto>(HttpStatusCode.BadRequest,"Почтаи электронӣ ё парол нодуруст аст");

        var passwordValid = await userManager.CheckPasswordAsync(user, dto.Password);
        if (!passwordValid)
            return new Response<AuthResponseDto>(HttpStatusCode.BadRequest,"Почтаи электронӣ ё парол нодуруст аст");

        var roles = await userManager.GetRolesAsync(user);
        var (token, expiresAt) = tokenService.GenerateToken(user, roles);

        var response = new AuthResponseDto(token, expiresAt, user.Id, user.FullName, user.Email!, roles.ToList());
        return new Response<AuthResponseDto>(response,"Login Success");
    }  
}