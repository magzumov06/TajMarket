using System.Net;
using Domain.DTOs.AuthDto;
using Domain.Entities;
using Domain.Entities.UserEntity;
using Domain.Responses;
using Infrastructure.Data;
using Infrastructure.Helpers;
using Infrastructure.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Auth;

public class OtpService(
    ILogger<OtpService> logger,
    UserManager<User> userManager,
    DataContext context,
    IEmailService emailService
) : IOtpService
{

    public async Task<Response<string>> VerifyOtpAsync(VerifyOtpDto dto)
    {
        try
        {
            dto.Email = dto.Email.Trim().ToLowerInvariant();

            logger.LogInformation(
                "OTP verification started for {Email}",
                dto.Email);


            var user = await userManager.FindByEmailAsync(dto.Email);


            if (user == null)
            {
                logger.LogWarning(
                    "OTP verification failed. User not found {Email}",
                    dto.Email);

                return new Response<string>(
                    HttpStatusCode.NotFound,
                    "Корбар ёфт нашуд");
            }


            if (user.EmailConfirmed)
            {
                logger.LogWarning(
                    "OTP verification failed. Email already confirmed {UserId}",
                    user.Id);

                return new Response<string>(
                    HttpStatusCode.BadRequest,
                    "Почтаи электронӣ аллакай тасдиқ шудааст");
            }


            var otp = await context.OtpCodes
                .Where(x => x.UserId == user.Id)
                .OrderByDescending(x => x.Id)
                .FirstOrDefaultAsync();


            if (otp == null)
            {
                logger.LogWarning(
                    "OTP not found for user {UserId}",
                    user.Id);

                return new Response<string>(
                    HttpStatusCode.BadRequest,
                    "Рамзи тасдиқ ёфт нашуд");
            }


            if (otp.AttemptCount >= 5)
            {
                logger.LogWarning(
                    "OTP attempts exceeded for user {UserId}",
                    user.Id);

                return new Response<string>(
                    HttpStatusCode.BadRequest,
                    "Шумораи кӯшишҳо зиёд шуд");
            }


            if (otp.IsUsed)
            {
                logger.LogWarning(
                    "OTP already used for user {UserId}",
                    user.Id);

                return new Response<string>(
                    HttpStatusCode.BadRequest,
                    "Ин рамз аллакай истифода шудааст");
            }


            if (otp.ExpireAt < DateTime.UtcNow)
            {
                logger.LogWarning(
                    "OTP expired for user {UserId}",
                    user.Id);

                return new Response<string>(
                    HttpStatusCode.BadRequest,
                    "Муҳлати рамз ба анҷом расидааст");
            }


            if (otp.Code != dto.Code)
            {
                otp.AttemptCount++;

                await context.SaveChangesAsync();


                logger.LogWarning(
                    "Wrong OTP code for user {UserId}. Attempt {AttemptCount}",
                    user.Id,
                    otp.AttemptCount);


                return new Response<string>(
                    HttpStatusCode.BadRequest,
                    "Рамзи тасдиқ нодуруст аст");
            }


            otp.IsUsed = true;
            otp.AttemptCount = 0;


            user.EmailConfirmed = true;


            await userManager.UpdateAsync(user);

            await context.SaveChangesAsync();


            logger.LogInformation(
                "OTP verified successfully for user {UserId}",
                user.Id);


            return new Response<string>(
                HttpStatusCode.OK,
                "Почтаи электронӣ бомуваффақият тасдиқ шуд");
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Error while verifying OTP for {Email}",
                dto.Email);


            return new Response<string>(
                HttpStatusCode.InternalServerError,
                "Internal server error");
        }
    }



    public async Task<Response<string>> ResendOtpAsync(string email)
    {
        try
        {
            email = email.Trim().ToLowerInvariant();


            logger.LogInformation(
                "Resend OTP started for {Email}",
                email);


            var user = await userManager.FindByEmailAsync(email);


            if (user == null)
            {
                logger.LogWarning(
                    "Resend OTP failed. User not found {Email}",
                    email);

                return new Response<string>(
                    HttpStatusCode.NotFound,
                    "Корбар ёфт нашуд");
            }


            if (user.EmailConfirmed)
            {
                return new Response<string>(
                    HttpStatusCode.BadRequest,
                    "Почтаи электронӣ аллакай тасдиқ шудааст");
            }


            var oldOtps = context.OtpCodes
                .Where(x => x.UserId == user.Id);


            context.OtpCodes.RemoveRange(oldOtps);


            var code = OtpGenerator.Generate();


            var otp = new OtpCode
            {
                UserId = user.Id,
                Code = code,
                ExpireAt = DateTime.UtcNow.AddMinutes(5),
                IsUsed = false,
                AttemptCount = 0
            };


            context.OtpCodes.Add(otp);


            await context.SaveChangesAsync();


            try
            {
                await emailService.SendOtpAsync(
                    user.Email!,
                    code);
            }
            catch
            {
                context.OtpCodes.Remove(otp);

                await context.SaveChangesAsync();

                throw;
            }


            logger.LogInformation(
                "New OTP sent for user {UserId}",
                user.Id);


            return new Response<string>(
                HttpStatusCode.OK,
                "Рамзи нав фиристода шуд");
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Error while resending OTP");


            return new Response<string>(
                HttpStatusCode.InternalServerError,
                "Internal server error");
        }
    }
}