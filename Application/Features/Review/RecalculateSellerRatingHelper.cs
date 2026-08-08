using Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Features.Review;

internal static class RecalculateSellerRatingHelper
{
    public static async Task RecalculateAsync(
        IApplicationDbContext context, int sellerProfileId, ILogger logger, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Recalculating seller rating {SellerProfileId}", sellerProfileId);

            var sellerProfile = await context.SellerProfiles
                .FirstOrDefaultAsync(sp => sp.Id == sellerProfileId, cancellationToken);

            if (sellerProfile == null)
            {
                logger.LogWarning("Seller profile not found {SellerProfileId}", sellerProfileId);
                return;
            }

            var ratings = await context.Reviews
                .Where(r => r.Product.SellerProfileId == sellerProfileId)
                .Select(r => r.Rating)
                .ToListAsync(cancellationToken);

            sellerProfile.Rating = ratings.Count != 0
                ? decimal.Round((decimal)ratings.Average(), 1)
                : 0;

            await context.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Seller rating updated {SellerProfileId} Rating {Rating}", sellerProfileId, sellerProfile.Rating);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error recalculating seller rating {SellerProfileId}", sellerProfileId);
        }
    }
}