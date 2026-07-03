using Microsoft.AspNetCore.Http;

namespace Domain.DTOs.SellerDto;

public class UploadStoreLogoDto
{
    public IFormFile? File { get; set; }
}