using Domain.DTOs.FileUpload;
using Microsoft.AspNetCore.Http;

namespace Infrastructure.FileStorage;

public interface IFileStorageService
{
    Task<FileUploadResultDto> UploadImageAsync(IFormFile file, string folder);
    Task<List<FileUploadResultDto>> UploadImagesAsync(IEnumerable<IFormFile> files, string folder);
    Task<bool> DeleteImageAsync(string? publicId);
}