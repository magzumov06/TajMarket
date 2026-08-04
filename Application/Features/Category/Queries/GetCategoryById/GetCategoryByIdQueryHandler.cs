// Application/Features/Category/Queries/GetCategoryById/GetCategoryByIdQueryHandler.cs
using System.Net;
using Application.Common.Interfaces;
using Application.Features.Category.DTOs;
using Domain.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Features.Category.Queries.GetCategoryById;

public class GetCategoryByIdQueryHandler(
    IApplicationDbContext context,
    ILogger<GetCategoryByIdQueryHandler> logger)
    : IRequestHandler<GetCategoryByIdQuery, Response<CategoryDto>>
{
    public async Task<Response<CategoryDto>> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
    {
        var id = request.Id;

        try
        {
            logger.LogInformation("Retrieving category {CategoryId}", id);

            var all = await context.Categories
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            var category = all.FirstOrDefault(c => c.Id == id);

            if (category == null)
            {
                logger.LogWarning("Category not found {CategoryId}", id);
                return new Response<CategoryDto>(HttpStatusCode.NotFound, "Category not found");
            }

            var result = new Response<CategoryDto>(CategoryMapper.MapWithChildren(category, all));

            logger.LogInformation("Retrieved category successfully {CategoryId}", id);
            return result;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error retrieving category {CategoryId}", id);
            return new Response<CategoryDto>(HttpStatusCode.InternalServerError, "Interval Server Error");
        }
    }
}