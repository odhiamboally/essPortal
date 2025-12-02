using System.Text.Json.Serialization;
using System.Text.Json;

namespace ESSPortal.Application.Configuration;
public class JsonSettings
{
    public JsonSerializerOptions Options { get; set; } = new JsonSerializerOptions
    {
        ReadCommentHandling = JsonCommentHandling.Skip,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        AllowTrailingCommas = false,
        WriteIndented = true
        
    };
}
