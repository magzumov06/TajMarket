using Application.Features.Address.Dtos;
using Application.Features.Address.DTOs;
using Domain.Responses;
using MediatR;

namespace Application.Features.Address.Commands.UpdateAddress;

public class UpdateAddressCommand(int userId, int addressId, CreateAddressDto dto) : IRequest<Response<AddressDto>>
{
    public int UserId { get; } = userId;
    public int AddressId { get; } = addressId;
    public CreateAddressDto Dto { get; } = dto;
}