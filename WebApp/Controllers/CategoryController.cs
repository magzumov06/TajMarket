using Domain.DTOs.CategoryDtos;
using Infrastructure.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoryController(ICategoryService categoryService) : ControllerBase
{

    [HttpPost]
    public async Task<IActionResult> CreateCategory([FromForm] CreateCategoryDto dto)
    {
        var res = await categoryService.CreateCategoryAsync(dto);
        return StatusCode((int)res.StatusCode, res);
    }

    
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var res = await categoryService.DeleteCategoryAsync(id);
        return StatusCode((int)res.StatusCode, res);
    }


    [HttpGet("tree")]
    public async Task<IActionResult> GetCategoryTree()
    {
        var res = await categoryService.GetTreeAsync();
        return Ok(res);
    }


    [HttpGet("{id}")]
    public async Task<IActionResult> GetCategory(int id)
    {
        var res = await categoryService.GetCategoryAsync(id);
        return StatusCode((int)res.StatusCode, res);
    }
}
