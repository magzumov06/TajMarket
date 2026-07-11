using System.Net;
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

namespace Infrastructure.Services;

public class SellerService(
    DataContext context,
    UserManager<User> userManager,
    IFileStorageService fileStorage) : ISellerService
{
    private const string SellerRole = "Seller";

    public async Task<Response<SellerProfileDto>> BecomeSellerAsync(int userId, SellerRegisterDto dto)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user == null)
            return new Response<SellerProfileDto>(HttpStatusCode.NotFound,"Корбар ёфт нашуд");

        var alreadySeller = await context.SellerProfiles.AnyAsync(sp => sp.UserId == userId);
        if (alreadySeller)
            return new Response<SellerProfileDto>(HttpStatusCode.Conflict,"Шумо аллакай Seller ҳастед");

        var storeNameTaken = await context.SellerProfiles.AnyAsync(sp => sp.StoreName == dto.StoreName);
        if (storeNameTaken)
            return new Response<SellerProfileDto>(HttpStatusCode.Conflict,"Ин номи мағоза аллакай истифода шудааст");

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
            await userManager.AddToRoleAsync(user, SellerRole);

        return new Response<SellerProfileDto>(ToDto(profile, 0), "Шумо ҳоло Seller ҳастед. Пас аз тасдиқи админ, мағозаи шумо verified мешавад");
    }

    public async Task<Response<SellerProfileDto>> GetProfileAsync(int userId)
    {
        var profile = await context.SellerProfiles
            .Include(sp => sp.Products)
            .FirstOrDefaultAsync(sp => sp.UserId == userId);

        if (profile == null)
            return new Response<SellerProfileDto>(HttpStatusCode.NotFound,"Профили Seller ёфт нашуд");

        return new Response<SellerProfileDto>(ToDto(profile, profile.Products?.Count(p => p.IsActive) ?? 0));
    }

    public async Task<Response<SellerProfileDto>> GetByIdAsync(int sellerProfileId)
    {
        var profile = await context.SellerProfiles
            .Include(sp => sp.Products)
            .FirstOrDefaultAsync(sp => sp.Id == sellerProfileId);

        if (profile == null)
            return new Response<SellerProfileDto>(HttpStatusCode.NotFound,"Мағоза ёфт нашуд");

        return new Response<SellerProfileDto>(ToDto(profile, profile.Products?.Count(p => p.IsActive) ?? 0));
    }

    public async Task<Response<string>> UploadLogoAsync(int userId, IFormFile file)
    {
        var profile = await context.SellerProfiles.FirstOrDefaultAsync(sp => sp.UserId == userId);
        if (profile == null)
            return new Response<string>(HttpStatusCode.BadRequest,"Аввал бояд Seller шавед");

        if (!string.IsNullOrEmpty(profile.StoreLogoPublicId))
            await fileStorage.DeleteImageAsync(profile.StoreLogoPublicId);

        var uploaded = await fileStorage.UploadImageAsync(file, "store-logos");

        profile.StoreLogoUrl = uploaded.Url;
        profile.StoreLogoPublicId = uploaded.PublicId;
        await context.SaveChangesAsync();

        return new Response<string>(uploaded.Url, "Логои мағоза нав шуд");
    }

    private static SellerProfileDto ToDto(SellerProfile profile, int totalProducts) => new(
        profile.Id,
        profile.StoreName,
        profile.StoreDescription,
        profile.StoreLogoUrl,
        profile.IsVerified,
        profile.Rating,
        totalProducts
    );
}
