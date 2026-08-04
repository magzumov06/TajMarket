// Application/Common/Interfaces/IFileStorageService.cs

using Microsoft.AspNetCore.Http;

namespace Application.Common.Interfaces;

public record UploadedFileResult(string Url, string PublicId);

public interface IFileStorageService
{
    Task<UploadedFileResult> UploadImageAsync(IFormFile file, string folder);
    Task<List<UploadedFileResult>> UploadImagesAsync(IEnumerable<IFormFile> files, string folder);
    Task<bool> DeleteImageAsync(string publicId);   // ← навъи бозгашт иваз шуд: Task → Task<bool>
}