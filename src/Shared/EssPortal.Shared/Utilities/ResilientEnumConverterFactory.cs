using ESSPortal.Shared.Utilities.Common;

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EssPortal.Shared.Utilities;

public class ResilientEnumConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        var type = GetEnumType(typeToConvert);
        return type != null && type.IsEnum;
    }

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var enumType = GetEnumType(typeToConvert)!;

        // Logic to decide between Nullable or Standard converter
        if (typeToConvert.IsGenericType && typeToConvert.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            return (JsonConverter)Activator.CreateInstance(
                typeof(NullableEnumConverter<>).MakeGenericType(enumType))!;
        }

        return (JsonConverter)Activator.CreateInstance(
            typeof(EnumConverter<>).MakeGenericType(enumType))!;
    }

    private static Type? GetEnumType(Type typeToConvert)
    {
        if (typeToConvert.IsEnum) return typeToConvert;
        return Nullable.GetUnderlyingType(typeToConvert);
    }
}
