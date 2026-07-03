using Microsoft.AspNetCore.Http;

namespace Domain.DTOs.CategoryDto;

public class CreateCategoryDto
{
    public string Name { get; set; }
    public int? ParentCategoryId { get; set; }
    public IFormFile? Icon { get; set; }
}