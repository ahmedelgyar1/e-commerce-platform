using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using e_commerce_platform.Application.DTOs.Product;
using e_commerce_platform.Application.Interfaces;
using e_commerce_platform.Settings;
using Microsoft.Extensions.Options;

namespace e_commerce_platform.Infrastructure.Services;

public class CloudinaryImageService : IImageService
{
    private readonly Cloudinary _cloudinary;
    private readonly ILogger<CloudinaryImageService> _logger;

    public CloudinaryImageService(IOptions<CloudinarySettings> settings, ILogger<CloudinaryImageService> logger)
    {
        var account = new Account(
            settings.Value.CloudName,
            settings.Value.ApiKey,
            settings.Value.ApiSecret);

        _cloudinary = new Cloudinary(account);
        _logger = logger;
    }

    public async Task<ImageUploadResultDto> UploadImageAsync(IFormFile file, string folder)
    {
        _logger.LogInformation("Uploading image to Cloudinary folder '{Folder}'.", folder);

        await using var stream = file.OpenReadStream();

        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(file.FileName, stream),
            Folder = folder,
            Transformation = new Transformation()
                .Quality("auto")
                .FetchFormat("auto")
        };

        var uploadResult = await _cloudinary.UploadAsync(uploadParams);

        if (uploadResult.Error != null)
        {
            _logger.LogError("Cloudinary upload failed: {Error}", uploadResult.Error.Message);
            throw new InvalidOperationException($"Image upload failed: {uploadResult.Error.Message}");
        }

        _logger.LogInformation("Image uploaded successfully. PublicId: {PublicId}", uploadResult.PublicId);

        return new ImageUploadResultDto
        {
            Url = uploadResult.SecureUrl.ToString(),
            PublicId = uploadResult.PublicId
        };
    }

    public async Task DeleteImageAsync(string publicId)
    {
        _logger.LogInformation("Deleting image from Cloudinary. PublicId: {PublicId}", publicId);

        var deleteParams = new DeletionParams(publicId);
        var result = await _cloudinary.DestroyAsync(deleteParams);

        if (result.Error != null)
        {
            _logger.LogError("Cloudinary deletion failed: {Error}", result.Error.Message);
            throw new InvalidOperationException($"Image deletion failed: {result.Error.Message}");
        }

        _logger.LogInformation("Image deleted successfully. PublicId: {PublicId}", publicId);
    }
}
