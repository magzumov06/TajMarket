using System.Net;
using Application.Features.Seller.DTOs;
using Domain.DTOs.SellerDto;
using Domain.Entities;
using Domain.Entities.UserEntity;
using Domain.Responses;
using Infrastructure.Data;
using Infrastructure.FileStorage;
using Infrastructure.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public class SellerService(
    DataContext context,
    UserManager<User> userManager,
    IFileStorageService fileStorage,
    ILogger<SellerService> logger) : ISellerService
{
    private const string SellerRole = "Seller";


    public async Task<Response<SellerProfileDto>> BecomeSellerAsync(int userId, SellerRegisterDto dto)
    {
        try
        {
            logger.LogInformation("User {UserId} trying to become seller with store {StoreName}", userId, dto.StoreName);

            var user = await userManager.FindByIdAsync(userId.ToString());

            if (user == null)
            {
                logger.LogWarning("User not found {UserId}", userId);

                return new Response<SellerProfileDto>(HttpStatusCode.NotFound, "Корбар ёфт нашуд");
            }


            var alreadySeller = await context.SellerProfiles
                .AnyAsync(sp => sp.UserId == userId);

            if (alreadySeller)
            {
                logger.LogWarning("User {UserId} already has seller profile", userId);

                return new Response<SellerProfileDto>(HttpStatusCode.Conflict, "Шумо аллакай Seller ҳастед");
            }


            var storeNameTaken = await context.SellerProfiles
                .AnyAsync(sp => sp.StoreName == dto.StoreName);


            if (storeNameTaken)
            {
                logger.LogWarning("Store name already exists {StoreName}", dto.StoreName);

                return new Response<SellerProfileDto>(HttpStatusCode.Conflict, "Ин номи мағоза аллакай истифода шудааст");
            }


            var profile = new SellerProfile
            {
                UserId = userId,
                StoreName = dto.StoreName,
                StoreDescription = dto.StoreDescription,
                IsVerified = false,
                Rating = 0,
                RegisteredAt = DateTime.UtcNow
            };


            context.SellerProfiles.Add(profile);

            await context.SaveChangesAsync();


            if (!await userManager.IsInRoleAsync(user, SellerRole))
            {
                await userManager.AddToRoleAsync(user, SellerRole);

                logger.LogInformation("Seller role added to user {UserId}", userId);
            }
            
            logger.LogInformation("User {UserId} successfully became seller with profile {SellerProfileId}", userId, profile.Id);

            return new Response<SellerProfileDto>(ToDto(profile, 0), "Шумо ҳоло Seller ҳастед. Пас аз тасдиқи админ, мағозаи шумо verified мешавад");
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error while user {UserId} becoming seller", userId);

            return new Response<SellerProfileDto>(HttpStatusCode.InternalServerError, "Internal server error");
        }
    }


    public async Task<Response<SellerProfileDto>> GetProfileAsync(int userId)
    {
        try
        {
            logger.LogInformation("Getting seller profile for user {UserId}", userId);

            var profile = await context.SellerProfiles
                .Include(sp => sp.Products)
                .FirstOrDefaultAsync(sp => sp.UserId == userId);
            
            if (profile == null)
            {
                logger.LogWarning("Seller profile not found for user {UserId}", userId);

                return new Response<SellerProfileDto>(HttpStatusCode.NotFound, "Профили Seller ёфт нашуд");
            }
            
            return new Response<SellerProfileDto>(ToDto(profile, profile.Products?.Count(p => p.IsActive) ?? 0));
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error getting seller profile for user {UserId}", userId);

            return new Response<SellerProfileDto>(HttpStatusCode.InternalServerError, "Internal server error");
        }
    }


    public async Task<Response<SellerProfileDto>> GetByIdAsync(int sellerProfileId)
    {
        try
        {
            logger.LogInformation("Getting seller profile {SellerProfileId}", sellerProfileId);

            var profile = await context.SellerProfiles
                .Include(sp => sp.Products)
                .FirstOrDefaultAsync(sp => sp.Id == sellerProfileId);


            if (profile == null)
            {
                logger.LogWarning("Seller profile not found {SellerProfileId}", sellerProfileId);

                return new Response<SellerProfileDto>(HttpStatusCode.NotFound, "Мағоза ёфт нашуд");
            }


            return new Response<SellerProfileDto>(ToDto(profile, profile.Products?.Count(p => p.IsActive) ?? 0));
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error getting seller profile {SellerProfileId}", sellerProfileId);

            return new Response<SellerProfileDto>(HttpStatusCode.InternalServerError, "Internal server error");
        }
    }


    public async Task<Response<string>> UploadLogoAsync(int userId, IFormFile file)
    {
        try
        {
            logger.LogInformation("Uploading seller logo for user {UserId}", userId);
            
            var profile = await context.SellerProfiles
                .FirstOrDefaultAsync(sp => sp.UserId == userId);


            if (profile == null)
            {
                logger.LogWarning("Seller profile not found for user {UserId}", userId);

                return new Response<string>(HttpStatusCode.BadRequest, "Аввал бояд Seller шавед");
            }


            if (!string.IsNullOrEmpty(profile.StoreLogoPublicId))
            {
                logger.LogInformation("Deleting old seller logo {PublicId}", profile.StoreLogoPublicId);

                await fileStorage.DeleteImageAsync(profile.StoreLogoPublicId);
            }


            var uploaded = await fileStorage.UploadImageAsync(file, "store-logos");
            
            profile.StoreLogoUrl = uploaded.Url;
            profile.StoreLogoPublicId = uploaded.PublicId;

            await context.SaveChangesAsync();


            logger.LogInformation("Seller logo uploaded successfully for user {UserId}", userId);


            return new Response<string>(uploaded.Url, "Логои мағоза нав шуд");
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error uploading seller logo for user {UserId}", userId);

            return new Response<string>(HttpStatusCode.InternalServerError, "Internal server error");
        }
    }


    private static SellerProfileDto ToDto(
        SellerProfile profile,
        int totalProducts) => new(
            profile.Id,
            profile.StoreName,
            profile.StoreDescription,
            profile.StoreLogoUrl,
            profile.IsVerified,
            profile.Rating,
            totalProducts
        );
}