using System.Net;
using Application.Common.Interfaces;
using Application.Features.Cart.DTOs;
using Domain.Responses;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Cart.Queries.GetCartItems;

public class GetCartItemsQueryHandler(
    IApplicationDbContext context,
    ILogger<GetCartItemsQueryHandler> logger)
    : IRequestHandler<GetCartItemsQuery, Response<CartDto>>
{
    public async Task<Response<CartDto>> Handle(GetCartItemsQuery request, CancellationToken cancellationToken)
    {
        var userId = request.UserId;

        try
        {
            logger.LogInformation("Retrieving cart items for user {UserId}", userId);

            var cart = await CartHelper.GetOrCreateCartAsync(context, userId, logger, cancellationToken);

            var cartDto = await CartMapper.MapAsync(context, cart.Id, cancellationToken);

            logger.LogInformation("Retrieved {ItemCount} cart items for user {UserId}", cartDto.Items.Count, userId);

            return new Response<CartDto>(cartDto);
        }
        catch (Exception e)
        {
            logger.LogError(e, "GetCartItemsAsync failed for user {UserId}", userId);
            return new Response<CartDto>(HttpStatusCode.InternalServerError, "Internal server error");
        }
    }
}