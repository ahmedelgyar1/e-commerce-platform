using e_commerce_platform.Application.DTOs.Product;
using Microsoft.AspNetCore.Http;

namespace e_commerce_platform.Application.Interfaces;

public interface IImageService
{
    Task<ImageUploadResultDto> UploadImageAsync(IFormFile file, string folder);
    Task DeleteImageAsync(string publicId);
}
