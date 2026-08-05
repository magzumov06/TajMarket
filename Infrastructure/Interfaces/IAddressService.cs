using Application.Features.Address.DTOs;
using Domain.DTOs.AddressDtos;
using Domain.Responses;

namespace Infrastructure.Interfaces;

public interface IAddressService
{
    Task<List<AddressDto>> GetAllAsync(int userId);
    Task<Response<AddressDto>> GetByIdAsync(int userId, int addressId);
    Task<Response<AddressDto>> CreateAsync(int userId, CreateAddressDto dto);
    Task<Response<AddressDto>> UpdateAsync(int userId, int addressId, CreateAddressDto dto);
    Task<Response<string>> DeleteAsync(int userId, int addressId);
    Task<Response<string>> SetDefaultAsync(int userId, int addressId);
}