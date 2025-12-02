
namespace ESSPortal.Web.Mvc.Configurations;

public class FileSettings
{
    public string? Path { get; set; }
    public string? UploadPath { get; set; }
    public string? ProfilePicturesBaseUrl { get; set; }
    public string? ProfilePicturesPath { get; set; }
    public string? UploadUrl { get; set; }
    public string TempPath { get; set; } = "C:\\Temp\\";
    public string ImagesPath { get; set; } = "/temp/";
    public long MaxFileSize { get; set; } = 10 * 1024 * 1024; // 10MB
    public List<string> AllowedExtensions { get; set; } = [];
}
