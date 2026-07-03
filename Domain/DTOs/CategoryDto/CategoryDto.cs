namespace Domain.DTOs.CategoryDto;

public record CategoryDto(
    int Id,
    string Name,
    string Slug,
    string? IconUrl,
    int? ParentCategoryId,
    List<CategoryDto>? SubCategories
);