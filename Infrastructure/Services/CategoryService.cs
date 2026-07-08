using System.Net;
using Domain.DTOs.CategoryDtos;
using Domain.Entities.CategoryEntity;
using Domain.Responses;
using Infrastructure.Data;
using Infrastructure.FileStorage;
using Infrastructure.Helpers;
using Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Infrastructure.Services;

public class CategoryService(
    DataContext context,
    IFileStorageService  fileStorage) : ICategoryService
{
    public async Task<Response<string>> CreateCategoryAsync(CreateCategoryDto dto)
    {
        try
        {
            Log.Information("Creating category {CategoryName} with parent id {ParentCategoryId}", dto.Name, dto.ParentCategoryId);
            if (dto.ParentCategoryId.HasValue)
            {
                var prentExist = await context.Categories.AnyAsync(c=> c.Id == dto.ParentCategoryId);
                if (!prentExist)
                    return new Response<string>(HttpStatusCode.NotFound, "Parent category does not exist");
            }

            var baseSlug = SlugHelper.GenerateSlug(dto.Name);
            var slug = await context.Categories.AnyAsync(c => c.Slug == baseSlug)
                ? SlugHelper.WithUniqueSuffix(baseSlug)
                : baseSlug;

            var category = new Category
            {
                Name = dto.Name,
                Slug = slug,
                ParentCategoryId = dto.ParentCategoryId
            };

            if (dto.Icon != null)
            {
                var uploaded = await fileStorage.UploadImageAsync(dto.Icon, "categories");
                category.IconUrl = uploaded.Url;
                category.IconPublicId = uploaded.PublicId;
            }
            
            context.Categories.Add(category);
            await context.SaveChangesAsync();
            
            Log.Information("Category created successfully: {CategoryId}", category.Id);
            return new Response<string>(HttpStatusCode.OK, "Category created");
        }
        catch (Exception e)
        {
            Log.Error(e, "Error creating category {CategoryName}", dto.Name);
            return new Response<string>(HttpStatusCode.InternalServerError, "Interval Server Error");
        }
    }

    public async Task<Response<string>> DeleteCategoryAsync(int id)
    {
        try
        {
            Log.Information("Deleting category {CategoryId}", id);
            var category = await context.Categories
                .Include(c=> c.SubCategories)
                .Include(c=> c.Products)
                .FirstOrDefaultAsync(c => c.Id == id);
            
            if (category == null)
                return new Response<string>(HttpStatusCode.NotFound, "Категория ёфт нашуд");

            if (category.SubCategories?.Count > 0)
                return new Response<string>(HttpStatusCode.Conflict, "Аввал зеркатегорияҳоро нест кунед");

            if (category.Products?.Count > 0)
                return new Response<string>(HttpStatusCode.Conflict,
                    "Ин категория маҳсулот дорад, аввал маҳсулотро кӯчонед ё нест кунед");
            
            if(!string.IsNullOrEmpty(category.IconPublicId))
                await fileStorage.DeleteImageAsync(category.IconPublicId);
            
            context.Categories.Remove(category);
            await context.SaveChangesAsync();
            Log.Information("Category deleted successfully: {CategoryId}", id);
            return new Response<string>(HttpStatusCode.OK, "Category deleted");
        }
        catch (Exception e)
        {
            Log.Error(e, "Error deleting category {CategoryId}", id);
            return new Response<string>(HttpStatusCode.InternalServerError, "Interval Server Error");
        }
    }

    public async Task<List<CategoryDto>> GetTreeAsync()
    {
        try
        {
            Log.Information("Retrieving full category tree");
            var all = await context.Categories.AsNoTracking().ToListAsync();

            var roots = all.Where(c => c.ParentCategoryId == null);
            var tree = roots.Select(root => MapWithChildren(root, all)).ToList();
            Log.Information("Retrieved {CategoryCount} categories for tree", all.Count);
            return tree;
        }
        catch (Exception e)
        {
            Log.Error(e, "Error retrieving category tree");
            throw;
        }
    }

    public async Task<Response<CategoryDto>> GetCategoryAsync(int id)
    {
        try
        {
            Log.Information("Retrieving category {CategoryId}", id);
            var all = await context.Categories.AsNoTracking().ToListAsync();
            var category = all.FirstOrDefault(c => c.Id == id);
            if (category == null)
            {
                Log.Warning("Category not found: {CategoryId}", id);
                return new Response<CategoryDto>(HttpStatusCode.NotFound, "Category not found");
            }

            var result = new Response<CategoryDto>(MapWithChildren(category, all));
            Log.Information("Retrieved category successfully: {CategoryId}", id);
            return result;
        }
        catch (Exception e)
        {
            Log.Error(e, "Error retrieving category {CategoryId}", id);
            return new Response<CategoryDto>(HttpStatusCode.InternalServerError,"Interval Server Error");
        }
    }
    
    private static CategoryDto MapWithChildren(Category category, List<Category> all)
    {
        var children = all
            .Where(c => c.ParentCategoryId == category.Id)
            .Select(c => MapWithChildren(c, all))
            .ToList();

        return new CategoryDto(category.Id, category.Name, category.Slug, category.IconUrl, category.ParentCategoryId, children);
    }
}