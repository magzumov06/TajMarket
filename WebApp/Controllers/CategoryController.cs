// WebApp/Controllers/CategoryController.cs
using Application.Features.Category.Commands.CreateCategory;
using Application.Features.Category.Commands.DeleteCategory;
using Application.Features.Category.Dtos;
using Application.Features.Category.Queries.GetCategoryById;
using Application.Features.Category.Queries.GetCategoryTree;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers;

public class CategoryController(IMediator mediator) : BaseApiController
{
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateCategory([FromForm] CreateCategoryDto dto)
    {
        var res = await mediator.Send(new CreateCategoryCommand(dto));
        return StatusCode((int)res.StatusCode, res);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var res = await mediator.Send(new DeleteCategoryCommand(id));
        return StatusCode((int)res.StatusCode, res);
    }

    [HttpGet("tree")]
    [Authorize]
    public async Task<IActionResult> GetCategoryTree()
    {
        var res = await mediator.Send(new GetCategoryTreeQuery());
        return Ok(res);
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetCategory(int id)
    {
        var res = await mediator.Send(new GetCategoryByIdQuery(id));
        return StatusCode((int)res.StatusCode, res);
    }
}