using Domain.DTOs.CategoryDtos;
using Infrastructure.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers;


public class CategoryController(ICategoryService categoryService) : BaseApiController
{

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateCategory([FromForm] CreateCategoryDto dto)
    {
        var res = await categoryService.CreateCategoryAsync(dto);
        return StatusCode((int)res.StatusCode, res);
    }

    
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var res = await categoryService.DeleteCategoryAsync(id);
        return StatusCode((int)res.StatusCode, res);
    }


    [HttpGet("tree")]
    [Authorize]
    public async Task<IActionResult> GetCategoryTree()
    {
        var res = await categoryService.GetTreeAsync();
        return Ok(res);
    }


    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetCategory(int id)
    {
        var res = await categoryService.GetCategoryAsync(id);
        return StatusCode((int)res.StatusCode, res);
    }
}
