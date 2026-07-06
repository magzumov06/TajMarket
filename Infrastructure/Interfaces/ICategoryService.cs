using Domain.DTOs.CategoryDtos;
using Domain.Resposes;

namespace Infrastructure.Interfaces;

public interface ICategoryService
{
    Task<Response<string>> CreateCategoryAsync(CreateCategoryDto dto);
    Task<Response<string>> DeleteCategoryAsync(int id);
    Task<Response<List<CategoryDto>>> GetAllCategoriesAsync();
    Task<Response<CategoryDto>> GetCategoryAsync(int id);
}