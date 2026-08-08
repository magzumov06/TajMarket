using Application.Features.Seller.DTOs;
using Domain.DTOs.SellerDto;
using Domain.Responses;
using MediatR;

namespace Application.Features.Seller.Commands.BecomeSeller;

public class BecomeSellerCommand(int userId, SellerRegisterDto dto) : IRequest<Response<SellerProfileDto>>
{
    public int UserId { get; } = userId;
    public SellerRegisterDto Dto { get; } = dto;
}