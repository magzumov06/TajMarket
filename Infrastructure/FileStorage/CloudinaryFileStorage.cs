using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Domain.DTOs.FileUpload;
using Infrastructure.Settings;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Infrastructure.FileStorage;

public class CloudinaryFileStorageService : IFileStorageService
{
    private readonly Cloudinary _cloudinary;
    private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".webp"];
    private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5 MB

    public CloudinaryFileStorageService(IOptions<CloudinarySetting> options)
    {
        var settings = options.Value;
        var account = new Account(settings.CloudName, settings.ApiKey, settings.ApiSecret);
        _cloudinary = new Cloudinary(account);
    }

    public async Task<FileUploadResultDto> UploadImageAsync(IFormFile file, string folder)
    {
        ValidateFile(file);

        await using var stream = file.OpenReadStream();

        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(file.FileName, stream),
            Folder = $"tajmarket/{folder}",
            Transformation = new Transformation()
                .Width(1200).Height(1200).Crop("limit")
                .Quality("auto")
                .FetchFormat("auto"),
            Overwrite = true
        };

        var result = await _cloudinary.UploadAsync(uploadParams);

        if (result.Error != null)
            throw new InvalidOperationException($"Хатогии боркунии Cloudinary: {result.Error.Message}");

        return new FileUploadResultDto(result.SecureUrl.ToString(), result.PublicId);
    }

    public async Task<List<FileUploadResultDto>> UploadImagesAsync(IEnumerable<IFormFile> files, string folder)
    {
        var results = new List<FileUploadResultDto>();
        foreach (var file in files)
            results.Add(await UploadImageAsync(file, folder));

        return results;
    }

    public async Task<bool> DeleteImageAsync(string? publicId)
    {
        if (string.IsNullOrWhiteSpace(publicId))
            return false;

        var result = await _cloudinary.DestroyAsync(new DeletionParams(publicId));
        return result.Result == "ok";
    }

    private static void ValidateFile(IFormFile file)
    {
        if (file.Length == 0)
            throw new ArgumentException("Файл интихоб нашудааст");

        if (file.Length > MaxFileSizeBytes)
            throw new ArgumentException("Андозаи файл набояд аз 5MB зиёд бошад");

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(extension))
            throw new ArgumentException("Танҳо JPG, PNG ва WEBP қабул мешаванд");
    }
}
