using EssPortal.Web.Blazor.Dtos.Common;

using ESSPortal.Web.Blazor.Configurations;
using ESSPortal.Web.Blazor.Contracts.Interfaces.Services;
using ESSPortal.Web.Blazor.Dtos.Profile;

using Microsoft.Extensions.Options;

namespace ESSPortal.Web.Blazor.Contracts.Implementations.AppServices;

internal sealed class FileService : IFileService
{
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly ILogger<FileService> _logger;
    private readonly FileSettings _fileSettings;
    private readonly string _profilePicturesPath;
    private readonly string _baseUrl;

    public FileService(
        IWebHostEnvironment webHostEnvironment,
        ILogger<FileService> logger,
        IOptions<FileSettings> fileSettings
       )
    {
        _webHostEnvironment = webHostEnvironment;
        _logger = logger;
        _fileSettings = fileSettings.Value;

        _profilePicturesPath = _fileSettings.ProfilePicturesPath ?? Path.Combine("C:", "inetpub", "wwwroot", "EssPortal", "Images", "ProfilePictures");

        _baseUrl = _fileSettings.ProfilePicturesBaseUrl ?? "/Images/ProfilePictures";

        EnsureDirectoryExists(_profilePicturesPath);
    }

    public async Task<(string base64String, string imageDataUrl, byte[] imageBytes)> ReadLogoAsync()
    {
        try
        {
            var imagePath = Path.Combine(_webHostEnvironment.WebRootPath, "custom", "auth", "images", "UnLogo.png");

            if (!File.Exists(imagePath))
            {
                _logger.LogWarning("Logo file not found at: {ImagePath}", imagePath);
                return (string.Empty, string.Empty, []);
            }

            var imageBytes = await File.ReadAllBytesAsync(imagePath);
            var base64String = Convert.ToBase64String(imageBytes);
            var imageDataUrl = $"data:image/png;base64,{base64String}";

            return (base64String, imageDataUrl, imageBytes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading logo image");
            return (string.Empty, string.Empty, []);
        }
    }

    public async Task<AppResponse<ProfilePictureResponse>> SaveProfilePictureAsync(
        string userId, 
        byte[] imageBytes, 
        string fileName
        )
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return AppResponse<ProfilePictureResponse>.Failure("User ID is required");
            }

            if (imageBytes == null || imageBytes.Length == 0)
            {
                return AppResponse<ProfilePictureResponse>.Failure("Image data is required");
            }

            if (string.IsNullOrWhiteSpace(fileName))
            {
                return AppResponse<ProfilePictureResponse>.Failure("File name is required");
            }

            // Use the exact filename provided by backend (no modification)
            var filePath = Path.Combine(_profilePicturesPath, fileName);

            DeleteOldProfilePicture(userId);

            await File.WriteAllBytesAsync(filePath, imageBytes);

            var publicUrl = $"{_baseUrl}/{fileName}".Replace("\\", "/");

            _logger.LogInformation("Profile picture saved successfully for user {UserId}: {FileName}", userId, fileName);

            var response = new ProfilePictureResponse
            {
                UserId = userId,
                PictureUrl = publicUrl,
                FileName = fileName,
                Base64 = Convert.ToBase64String(imageBytes),
                ImageDataUrl = $"data:{GetContentType(Path.GetExtension(fileName))};base64,{Convert.ToBase64String(imageBytes)}"
            };

            return AppResponse<ProfilePictureResponse>.Success("Profile picture saved successfully", response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving profile picture for user {UserId}", userId);
            return AppResponse<ProfilePictureResponse>.Failure("An error occurred while saving the profile picture");
        }
    }

    public async Task<(byte[] imageBytes, string contentType)> ReadProfilePictureAsync(string fileName)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                _logger.LogWarning("Filename is required for reading profile picture");
                return ([], string.Empty);
            }

            var filePath = Path.Combine(_profilePicturesPath, fileName);

            if (!File.Exists(filePath))
            {
                _logger.LogWarning("Profile picture not found: {FilePath}", filePath);
                return ([], string.Empty);
            }

            var imageBytes = await File.ReadAllBytesAsync(filePath);
            var fileExtension = Path.GetExtension(fileName).ToLowerInvariant();
            var contentType = GetContentType(fileExtension);

            return (imageBytes, contentType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading profile picture: {FileName}", fileName);
            return ([], string.Empty);
        }
    }

    public async Task<(string base64String, string imageDataUrl)> ReadProfilePictureAsBase64Async(string fileName)
    {
        try
        {
            var (imageBytes, contentType) = await ReadProfilePictureAsync(fileName);

            if (imageBytes.Length == 0)
            {
                return (string.Empty, string.Empty);
            }

            var base64String = Convert.ToBase64String(imageBytes);
            var imageDataUrl = $"data:{contentType};base64,{base64String}";

            return (base64String, imageDataUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading profile picture as base64: {FileName}", fileName);
            return (string.Empty, string.Empty);
        }
    }

    public bool DeleteProfilePicture(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return false;

        var filePath = Path.Combine(_profilePicturesPath, fileName);

        if (!File.Exists(filePath))
        {
            _logger.LogInformation("Profile picture not found for deletion: {FilePath}", filePath);
            return true;
        }

        try
        {
            File.Delete(filePath);
            _logger.LogInformation("Profile picture deleted successfully: {FileName}", fileName);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting profile picture: {FileName}", fileName);
            return false;
        }
    }

    private void DeleteOldProfilePicture(string userId)
    {
        try
        {
            if (!Directory.Exists(_profilePicturesPath))
                return;

            var existingFiles = Directory.GetFiles(_profilePicturesPath, $"{userId}_*")
                .Where(f => IsImageFile(f))
                .ToArray();

            foreach (var file in existingFiles)
            {
                try
                {
                    File.Delete(file);
                    _logger.LogInformation("Deleted old profile picture: {FilePath}", file);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to delete old profile picture: {FilePath}", file);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting old profile pictures for user {UserId}", userId);
        }
    }

    private bool IsImageFile(string filePath)
    {
        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        string[] imageExtensions = [".jpg", ".jpeg", ".png", ".gif", ".webp"];
        return imageExtensions.Contains(extension);
    }

    private string GetContentType(string fileExtension)
    {
        return fileExtension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            _ => "application/octet-stream"
        };
    }

    private void EnsureDirectoryExists(string directoryPath)
    {
        try
        {
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
                _logger.LogInformation("Created profile pictures directory: {DirectoryPath}", directoryPath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating profile pictures directory: {DirectoryPath}", directoryPath);
            throw;
        }
    }

    public string ExtractFileNameFromUrl(string profilePictureUrl)
    {
        if (string.IsNullOrWhiteSpace(profilePictureUrl))
            return string.Empty;

        try
        {
            // Extract filename from URL like "/images/profilePictures/user123_20240804_143022_abc123.jpg"
            return Path.GetFileName(profilePictureUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting filename from URL: {Url}", profilePictureUrl);
            return string.Empty;
        }
    }




}
