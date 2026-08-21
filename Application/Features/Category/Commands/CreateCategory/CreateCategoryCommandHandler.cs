using System.Net;
using Application.Common.Interfaces;
using Application.Common.Utils;
using Domain.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Features.Category.Commands.CreateCategory;

public class CreateCategoryCommandHandler(
    IApplicationDbContext context,
    IFileStorageService fileStorage,
    ILogger<CreateCategoryCommandHandler> logger)
    : IRequestHandler<CreateCategoryCommand, Response<string>>
{
    public async Task<Response<string>> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;

        try
        {
            logger.LogInformation("Creating category {CategoryName} with parent id {ParentCategoryId}", dto.Name, dto.ParentCategoryId);

            if (dto.ParentCategoryId.HasValue)
            {
                var parentExist = await context.Categories
                    .AnyAsync(c => c.Id == dto.ParentCategoryId, cancellationToken);

                if (!parentExist)
                {
                    logger.LogWarning("Parent category does not exist {ParentCategoryId}", dto.ParentCategoryId);
                    return new Response<string>(HttpStatusCode.NotFound, "Parent category does not exist");
                }
            }
            
            var baseSlug = SlugHelper.GenerateBase(dto.Name);
            var slug = baseSlug;
            var attempt = 0;

            while (await context.Categories.AnyAsync(c => c.Slug == slug, cancellationToken))
            {
                attempt++;
                slug = SlugHelper.WithSuffix(baseSlug, attempt);
            }

            var category = new Domain.Entities.CategoryEntity.Category
            {
                Name = dto.Name,
                Slug = slug,
                ParentCategoryId = dto.ParentCategoryId
            };

            string? uploadedPublicId = null;

            if (dto.Icon != null)
            {
                var uploaded = await fileStorage.UploadImageAsync(dto.Icon, "categories");
                category.IconUrl = uploaded.Url;
                category.IconPublicId = uploaded.PublicId;
                uploadedPublicId = uploaded.PublicId;
            }

            context.Categories.Add(category);

            try
            {
                await context.SaveChangesAsync(cancellationToken);
            }
            catch
            {
                if (uploadedPublicId != null)
                {
                    var deleted = await fileStorage.DeleteImageAsync(uploadedPublicId);
                    if (!deleted)
                        logger.LogWarning("Failed to clean up orphan image {PublicId}", uploadedPublicId);
                }
                throw;
            }

            logger.LogInformation("Category created successfully {CategoryId}", category.Id);
            return new Response<string>(HttpStatusCode.Created, "Category created");
        }
        catch (ArgumentException e)
        {
            logger.LogWarning(e, "Invalid file for category {CategoryName}", dto.Name);
            return new Response<string>(HttpStatusCode.BadRequest, e.Message);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error creating category {CategoryName}", dto.Name);
            return new Response<string>(HttpStatusCode.InternalServerError, "Interval Server Error");
        }
    }
}