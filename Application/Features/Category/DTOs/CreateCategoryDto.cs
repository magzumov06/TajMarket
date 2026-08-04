
using Microsoft.AspNetCore.Http;

namespace Application.Features.Category.DTOs;

public class CreateCategoryDto
{
    public string Name { get; set; }
    public int? ParentCategoryId { get; set; }
    public IFormFile? Icon { get; set; }
}