using Application.Features.Category.DTOs;

namespace Application.Features.Category;

internal static class CategoryMapper
{
    public static CategoryDto MapWithChildren(Domain.Entities.CategoryEntity.Category category, List<Domain.Entities.CategoryEntity.Category> all)
    {
        var children = all
            .Where(c => c.ParentCategoryId == category.Id)
            .Select(child => MapWithChildren(child, all))
            .ToList();

        return new CategoryDto(
            category.Id,
            category.Name,
            category.Slug,
            category.IconUrl,
            category.ParentCategoryId,
            children
        );
    }
}