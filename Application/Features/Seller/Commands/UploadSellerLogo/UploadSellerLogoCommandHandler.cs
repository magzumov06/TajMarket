using System.Net;
using Application.Common.Interfaces;
using Domain.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Features.Seller.Commands.UploadSellerLogo;

public class UploadSellerLogoCommandHandler(
    IApplicationDbContext context,
    IFileStorageService fileStorage,
    ILogger<UploadSellerLogoCommandHandler> logger)
    : IRequestHandler<UploadSellerLogoCommand, Response<string>>
{
    public async Task<Response<string>> Handle(UploadSellerLogoCommand request, CancellationToken cancellationToken)
    {
        var (userId, file) = (request.UserId, request.File);

        try
        {
            logger.LogInformation("Uploading seller logo for user {UserId}", userId);

            var profile = await context.SellerProfiles
                .FirstOrDefaultAsync(sp => sp.UserId == userId, cancellationToken);

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

            await context.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Seller logo uploaded successfully for user {UserId}", userId);

            return new Response<string>(uploaded.Url, "Логои мағоза нав шуд");
        }
        catch (ArgumentException e)
        {
            logger.LogWarning(e, "Invalid file for seller logo, user {UserId}", userId);
            return new Response<string>(HttpStatusCode.BadRequest, e.Message);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error uploading seller logo for user {UserId}", userId);
            return new Response<string>(HttpStatusCode.InternalServerError, "Internal server error");
        }
    }
}