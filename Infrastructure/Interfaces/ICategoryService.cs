using Domain.DTOs.CategoryDtos;
using Domain.Responses;

namespace Infrastructure.Interfaces;

public interface ICategoryService
{
    Task<Response<string>> CreateCategoryAsync(CreateCategoryDto dto);
    Task<Response<string>> DeleteCategoryAsync(int id);
    Task<List<CategoryDto>> GetTreeAsync();
    Task<Response<CategoryDto>> GetCategoryAsync(int id);
}