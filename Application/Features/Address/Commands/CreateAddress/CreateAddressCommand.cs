using Application.Features.Address.Dtos;
using Application.Features.Address.DTOs;
using Domain.Responses;
using MediatR;

namespace Application.Features.Address.Commands.CreateAddress;

public class CreateAddressCommand(int userId, CreateAddressDto dto) : IRequest<Response<AddressDto>>
{
    public int UserId { get; } = userId;
    public CreateAddressDto Dto { get; } = dto;
}