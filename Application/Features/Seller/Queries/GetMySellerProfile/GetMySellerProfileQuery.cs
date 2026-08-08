using Application.Features.Seller.DTOs;
using Domain.Responses;
using MediatR;

namespace Application.Features.Seller.Queries.GetMySellerProfile;

public class GetMySellerProfileQuery(int userId) : IRequest<Response<SellerProfileDto>>
{
    public int UserId { get; } = userId;
}