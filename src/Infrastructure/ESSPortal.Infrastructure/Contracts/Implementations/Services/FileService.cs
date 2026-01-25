using ESSPortal.Application.Configuration;
using ESSPortal.Application.Contracts.Interfaces.Services;
using ESSPortal.Infrastructure.Configuration;
using ESSPortal.Shared.Dtos.Common;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ESSPortal.Infrastructure.Contracts.Implementations.Services;
internal sealed class FileService : IFileService
{
    private readonly ILogger<FileService> _logger;
    private readonly FileSettings _fileSettings;
    private readonly string _fileStoragePath;

    public FileService(
        IOptions<EmailSettings> emailSettings, 
        IOptions<FileSettings> fileSettings, 
        ILogger<FileService> logger)
    {
        _fileSettings = fileSettings.Value;
        _fileStoragePath = _fileSettings.ImagesPath;

        if (!Directory.Exists(_fileStoragePath))
        {
            Directory.CreateDirectory(_fileStoragePath);
        }

        _logger = logger;
    }

    public async Task<AppResponse<string>> SaveLogoAsync(string base64Image)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(base64Image) || !base64Image.Contains(","))
                return AppResponse<string>.Failure("Invalid image data");

            // Extract image data
            var data = Convert.FromBase64String(base64Image.Split(',')[1]);
            var fileName = "UNLogo.png";
            var filePath = Path.Combine(_fileStoragePath, fileName);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            // Save the new image
            await File.WriteAllBytesAsync(filePath, data);

            return AppResponse<string>.Success("Logo saved successfully", $"{_fileStoragePath}{fileName}");

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving logo");
            throw;
        }
    }
}
