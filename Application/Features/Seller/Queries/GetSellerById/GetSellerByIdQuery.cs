using Application.Features.Seller.DTOs;
using Domain.Responses;
using MediatR;

namespace Application.Features.Seller.Queries.GetSellerById;

public class GetSellerByIdQuery(int sellerProfileId) : IRequest<Response<SellerProfileDto>>
{
    public int SellerProfileId { get; } = sellerProfileId;
}