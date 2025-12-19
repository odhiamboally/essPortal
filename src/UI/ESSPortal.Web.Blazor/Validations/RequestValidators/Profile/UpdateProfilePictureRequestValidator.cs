using ESSPortal.Application.Dtos.Profile;
using ESSPortal.Web.Blazor.Dtos.Profile;
using ESSPortal.Web.Blazor.ViewModels.Profile;

using FluentValidation;

namespace ESSPortal.Web.Blazor.Validations.RequestValidators.Profile;

public class UpdateProfilePictureRequestValidator : AbstractValidator<UpdateProfilePictureViewModel>
{
    public UpdateProfilePictureRequestValidator()
    {
        RuleFor(x => x.ProfilePicture)
            .NotNull()
            .WithMessage("Please select a file to upload.")
            .Must(BeAValidImageFile)
            .WithMessage("Invalid image file.")
            .Must(BeWithinSizeLimit)
            .WithMessage("File size cannot exceed 5MB.")
            .Must(HaveValidImageExtension)
            .WithMessage("Only JPG, PNG, GIF, and WebP images are allowed.")
            .MustAsync(HaveValidImageSignatureAsync)
            .WithMessage("Invalid image file format - file may be corrupted or not a valid image.");
    }

    private bool BeAValidImageFile(IFormFile? file)
    {
        return file is { Length: > 0 };
    }

    private bool BeWithinSizeLimit(IFormFile? file)
    {
        if (file == null) return false;

        const int maxSizeBytes = 5 * 1024 * 1024; // 5MB
        const int minSizeBytes = 1024; // 1KB

        return file.Length is <= maxSizeBytes and >= minSizeBytes;
    }

    private bool HaveValidImageExtension(IFormFile? file)
    {
        if (file == null) return false;

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        string[] allowedExtensions = [".jpg", ".jpeg", ".png", ".gif", ".webp"];

        return allowedExtensions.Contains(extension);
    }

    private async Task<bool> HaveValidImageSignatureAsync(IFormFile? file, CancellationToken cancellationToken)
    {
        if (file == null) return false;

        try
        {
            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream, cancellationToken);
            var bytes = memoryStream.ToArray();

            return IsValidImageFile(bytes);
        }
        catch
        {
            return false;
        }
    }

    private static bool IsValidImageFile(byte[] bytes)
    {
        if (bytes.Length < 4) return false;

        // Check for common image file signatures
        var signatures = new Dictionary<byte[], string>
        {
            { [0xFF, 0xD8, 0xFF], "JPEG" },
            { [0x89, 0x50, 0x4E, 0x47], "PNG" },
            { [0x47, 0x49, 0x46, 0x38], "GIF" },
            { [0x52, 0x49, 0x46, 0x46], "WEBP" }
        };

        return signatures.Any(signature =>
            bytes.Take(signature.Key.Length).SequenceEqual(signature.Key));
    }
}