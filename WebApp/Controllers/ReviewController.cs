using Application.Features.Review.Commands.CreateReview;
using Application.Features.Review.Commands.DeleteReview;
using Application.Features.Review.DTOs;
using Application.Features.Review.Queries.GetReviewsByProduct;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers;

[Authorize]   
public class ReviewController(IMediator mediator) : BaseApiController
{
    [HttpPost]
    public async Task<IActionResult> CreateReview([FromBody] CreateReviewDto dto)
    {
        var res = await mediator.Send(new CreateReviewCommand(UserId, dto));
        return StatusCode((int)res.StatusCode, res);
    }

    [HttpDelete("{reviewId}")]
    public async Task<IActionResult> DeleteReview(int reviewId)
    {
        var res = await mediator.Send(new DeleteReviewCommand(UserId, reviewId));
        return StatusCode((int)res.StatusCode, res);
    }

    [HttpGet("product/{productId}")]
    [AllowAnonymous] 
    public async Task<IActionResult> GetProductReviews(int productId)
    {
        var res = await mediator.Send(new GetReviewsByProductQuery(productId));
        return StatusCode((int)res.StatusCode, res);
    }
}