using Domain.DTOs.ReviewDtos;
using Infrastructure.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WebApp.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReviewController(IReviewService reviewService) : ControllerBase
{
    private int UserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

    
    [HttpPost]
    public async Task<IActionResult> CreateReview([FromBody] CreateReviewDto dto)
    {
        var res = await reviewService.CreateAsync(UserId, dto);
        return StatusCode((int)res.StatusCode, res);
    }


    [HttpDelete("{reviewId}")]
    public async Task<IActionResult> DeleteReview(int reviewId)
    {
        var res = await reviewService.DeleteAsync(UserId, reviewId);
        return StatusCode((int)res.StatusCode, res);
    }


    [HttpGet("product/{productId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetProductReviews(int productId)
    {
        var res = await reviewService.GetByProductAsync(productId);
        return StatusCode((int)res.StatusCode, res);
    }
}
