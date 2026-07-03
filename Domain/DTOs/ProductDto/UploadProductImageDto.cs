using Microsoft.AspNetCore.Http;

namespace Domain.DTOs.ProductDto;

public class UploadProductImagesDto
{
    public int ProductId { get; set; }
    public List<IFormFile> Files { get; set; }
}