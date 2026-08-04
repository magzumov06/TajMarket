using Application.Common.Interfaces;
using Application.Features.Category.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Features.Category.Queries.GetCategoryTree;

public class GetCategoryTreeQueryHandler(
    IApplicationDbContext context,
    ILogger<GetCategoryTreeQueryHandler> logger)
    : IRequestHandler<GetCategoryTreeQuery, List<CategoryDto>>
{
    public async Task<List<CategoryDto>> Handle(GetCategoryTreeQuery request, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Retrieving full category tree");

            var all = await context.Categories
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            var roots = all.Where(c => c.ParentCategoryId == null);

            var tree = roots
                .Select(root => CategoryMapper.MapWithChildren(root, all))
                .ToList();

            logger.LogInformation("Retrieved {CategoryCount} categories for tree", all.Count);
            return tree;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error retrieving category tree");
            throw;
        }
    }
}