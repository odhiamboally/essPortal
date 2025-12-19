namespace ESSPortal.Web.Blazor.Extensions;

public static class FormFileExtensions
{
    public static async Task<string> ToBase64StringAsync(this IFormFile file)
    {
        if (file == null || file.Length == 0)
            return string.Empty;

        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream);
        var bytes = memoryStream.ToArray();
        return Convert.ToBase64String(bytes);
    }

    public static async Task<byte[]> ToBytesAsync(this IFormFile file)
    {
        if (file == null || file.Length == 0)
            return [];

        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream);
        return memoryStream.ToArray();
    }
}
