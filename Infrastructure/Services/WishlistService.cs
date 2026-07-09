using System.Net;
using Domain.DTOs.WishlistDto;
using Domain.Entities;
using Domain.Responses;
using Infrastructure.Data;
using Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class WishlistService(DataContext context) : IWishlistService
{
    #region Add

    public async Task<Response<string>> AddAsync(int userId, int productId)
    {
        try
        {
            var productExists = await context.Products.AnyAsync(p => p.Id == productId && p.IsActive);
            if (!productExists)
                return new Response<string>(HttpStatusCode.NotFound,"Маҳсулот ёфт нашуд");

            var alreadyAdded = await context.WishlistItems.AnyAsync(w => w.UserId == userId && w.ProductId == productId);
            if (alreadyAdded)
                return new Response<string>(HttpStatusCode.BadRequest,"Ин маҳсулот аллакай дар рӯйхати дӯстдоштаҳо ҳаст");

            context.WishlistItems.Add(new WishlistItem
            {
                UserId = userId,
                ProductId = productId,
                AddedAt = DateTime.UtcNow
            });

            await context.SaveChangesAsync();
            return new Response<string>(HttpStatusCode.OK,"Ба рӯйхати дӯстдоштаҳо илова шуд");        }
        catch (Exception e)
        {
            throw;
        }
    }

    #endregion

    #region Remove

    public async Task<Response<string>> RemoveAsync(int userId, int productId)
    {
        try
        {
            var item = await context.WishlistItems
                .FirstOrDefaultAsync(w => w.UserId == userId && w.ProductId == productId);

            if (item == null)
                return new Response<string>(HttpStatusCode.NotFound,"Ин маҳсулот дар рӯйхати дӯстдоштаҳо нест");

            context.WishlistItems.Remove(item);
            await context.SaveChangesAsync();

            return new Response<string>(HttpStatusCode.OK,"Аз рӯйхати дӯстдоштаҳо бароварда шуд");        }
       
        catch (Exception e)
        {
            throw;
        }
    }

    #endregion

    #region GetAll

    public async Task<Response<List<WishlistItemDto>>> GetAllAsync(int userId)
    {
        try
        {
            var items = await context.WishlistItems
                .AsNoTracking()
                .Include(w => w.Product).ThenInclude(p => p.Images)
                .Where(w => w.UserId == userId)
                .OrderByDescending(w => w.AddedAt)
                .ToListAsync();

            var res = items.Select(w => new WishlistItemDto(
                w.Id,
                w.ProductId,
                w.Product.Name,
                w.Product.Images.FirstOrDefault(i => i.IsMain)?.Url ?? w.Product.Images.FirstOrDefault()?.Url,
                w.Product.DiscountPrice ?? w.Product.Price,
                w.AddedAt
            )).ToList();

            return new Response<List<WishlistItemDto>>(res);
        }
        catch (Exception e)
        {
            throw;
        }
    }

    #endregion
}