using Domain.DTOs.CategoryDtos;
using Domain.Respoces;

namespace Infrastructure.Interfaces;

public interface ICategoryService
{
    Task<Responce<string>> CreateCategoryAsync(CreateCategoryDto dto);
    Task<Responce<string>> DeleteCategoryAsync(int id);
    Task<Responce<List<CategoryDto>>> GetAllCategoriesAsync();
    Task<Responce<CategoryDto>> GetCategoryAsync(int id);
}