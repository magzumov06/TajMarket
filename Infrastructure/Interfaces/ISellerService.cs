using Application.Features.Seller.DTOs;
using Domain.DTOs.SellerDto;
using Domain.Responses;
using Microsoft.AspNetCore.Http;

namespace Infrastructure.Interfaces;

public interface ISellerService
{
    Task<Response<SellerProfileDto>> BecomeSellerAsync(int userId, SellerRegisterDto dto);
    Task<Response<SellerProfileDto>> GetProfileAsync(int userId);
    Task<Response<SellerProfileDto>> GetByIdAsync(int sellerProfileId);
    Task<Response<string>> UploadLogoAsync(int userId, IFormFile file);
}