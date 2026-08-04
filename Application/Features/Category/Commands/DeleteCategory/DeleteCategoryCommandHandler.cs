// Application/Features/Category/Commands/DeleteCategory/DeleteCategoryCommandHandler.cs
using System.Net;
using Application.Common.Interfaces;
using Domain.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Features.Category.Commands.DeleteCategory;

public class DeleteCategoryCommandHandler(
    IApplicationDbContext context,
    IFileStorageService fileStorage,
    ILogger<DeleteCategoryCommandHandler> logger)
    : IRequestHandler<DeleteCategoryCommand, Response<string>>
{
    public async Task<Response<string>> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        var id = request.Id;

        try
        {
            logger.LogInformation("Deleting category {CategoryId}", id);

            var category = await context.Categories
                .Include(c => c.SubCategories)
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

            if (category == null)
            {
                logger.LogWarning("Category not found {CategoryId}", id);
                return new Response<string>(HttpStatusCode.NotFound, "Категория ёфт нашуд");
            }

            if (category.SubCategories?.Count > 0)
            {
                logger.LogWarning("Category has sub categories {CategoryId}", id);
                return new Response<string>(HttpStatusCode.Conflict, "Аввал зеркатегорияҳоро нест кунед");
            }

            if (category.Products?.Count > 0)
            {
                logger.LogWarning("Category has products {CategoryId}", id);
                return new Response<string>(HttpStatusCode.Conflict, "Ин категория маҳсулот дорад, аввал маҳсулотро кӯчонед ё нест кунед");
            }

            if (!string.IsNullOrEmpty(category.IconPublicId))
                await fileStorage.DeleteImageAsync(category.IconPublicId);

            context.Categories.Remove(category);
            await context.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Category deleted successfully {CategoryId}", id);
            return new Response<string>(HttpStatusCode.OK, "Category deleted");
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error deleting category {CategoryId}", id);
            return new Response<string>(HttpStatusCode.InternalServerError, "Interval Server Error");
        }
    }
}