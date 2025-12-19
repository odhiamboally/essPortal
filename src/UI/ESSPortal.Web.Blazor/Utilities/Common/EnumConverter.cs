using System.Text.Json;
using System.Text.Json.Serialization;

namespace ESSPortal.Web.Blazor.Utilities.Common;

public class EnumConverter<T> : JsonConverter<T> where T : struct, Enum
{
    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var stringValue = reader.GetString();

            if (string.IsNullOrWhiteSpace(stringValue))
            {
                return default(T);
            }

            // Try exact match first
            if (Enum.TryParse<T>(stringValue, true, out var exactResult))
            {
                return exactResult;
            }

            // Try with underscores replaced with spaces (common BC pattern)
            var normalizedValue = stringValue.Replace("_", " ");
            if (Enum.TryParse<T>(normalizedValue, true, out var normalizedResult))
            {
                return normalizedResult;
            }

            // Try with spaces replaced with underscores
            var underscoreValue = stringValue.Replace(" ", "_");
            if (Enum.TryParse<T>(underscoreValue, true, out var underscoreResult))
            {
                return underscoreResult;
            }

            // Log the issue but don't throw - return default value
            Console.WriteLine($"Warning: Could not parse enum value '{stringValue}' for type {typeof(T).Name}. Using default value.");
            return default(T);
        }

        if (reader.TokenType == JsonTokenType.Number)
        {
            var numValue = reader.GetInt32();
            if (Enum.IsDefined(typeof(T), numValue))
            {
                return (T)Enum.ToObject(typeof(T), numValue);
            }
            return default(T);
        }

        // For null or other token types, return default
        return default(T);
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}

