using Microsoft.AspNetCore.Http;

namespace Application.Features.Seller.DTOs;

public class UploadStoreLogoDto
{
    public IFormFile? File { get; set; }
}