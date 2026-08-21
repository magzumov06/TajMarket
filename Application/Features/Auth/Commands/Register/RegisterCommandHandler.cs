using System.Net;
using Application.Common.Interfaces;
using Application.Features.Auth.DTOs;
using Domain.Responses;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Auth.Commands.Register;

public class RegisterCommandHandler(
    IIdentityService identityService,
    IOtpService otpService,
    ILogger<RegisterCommandHandler> logger)
    : IRequestHandler<RegisterCommand, Response<AuthResponseDto>>
{
    private const string DefaultRole = "Customer";

    public async Task<Response<AuthResponseDto>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;
        dto.Email = dto.Email.Trim().ToLowerInvariant();

        try
        {
            logger.LogInformation("Registration attempt for {Email}", dto.Email);

            var existing = await identityService.FindUserByEmailAsync(dto.Email, cancellationToken);

            if (existing != null)
            {
                logger.LogWarning("Registration failed. Email already exists {Email}", dto.Email);
                return new Response<AuthResponseDto>(HttpStatusCode.Conflict,
                    "Корбар бо ин почтаи электронӣ аллакай сабт шудааст");
            }

            var user = new Domain.Entities.UserEntity.User
            {
                UserName = dto.Email,
                Email = dto.Email,
                FullName = dto.FullName,
                PhoneNumber = dto.PhoneNumber,
                CreatedAt = DateTime.UtcNow
            };

            var (createSucceeded, createErrors) =
                await identityService.CreateUserAsync(user, dto.Password, cancellationToken);

            if (!createSucceeded)
            {
                var errors = string.Join("; ", createErrors);
                logger.LogWarning("User registration failed {Errors}", errors);
                return new Response<AuthResponseDto>(HttpStatusCode.BadRequest, errors);
            }

            var (roleSucceeded, roleErrors) =
                await identityService.AddToRoleAsync(user.Id, DefaultRole, cancellationToken);

            if (!roleSucceeded)
            {
                var errors = string.Join("; ", roleErrors);
                logger.LogError("Role assignment failed {Errors}", errors);
                await identityService.DeleteUserAsync(user, cancellationToken);
                return new Response<AuthResponseDto>(HttpStatusCode.BadRequest, errors);
            }

            try
            {
                await otpService.ResendOtpAsync(user.Email!);
            }
            catch (Exception emailEx)
            {
                logger.LogError(emailEx,
                    "Failed to send OTP email during registration for {Email} — account was created successfully",
                    user.Email);

                return new Response<AuthResponseDto>(HttpStatusCode.OK,
                    "Ҳисоб сохта шуд, вале фиристодани рамзи тасдиқ ба почта ноком шуд. Лутфан тугмаи 'Дубора фиристодан'-ро пахш кунед.");
            }

            logger.LogInformation("User registered successfully {UserId}", user.Id);

            return new Response<AuthResponseDto>(HttpStatusCode.Created, "Рамзи тасдиқ ба почтаи электронӣ фиристода шуд.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during registration {Email}", dto.Email);
            return new Response<AuthResponseDto>(HttpStatusCode.InternalServerError, "Internal server error");
        }
    }
}