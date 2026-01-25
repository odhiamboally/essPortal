using System.Globalization;
using System.Text.Json.Serialization;

namespace EssPortal.Shared.Dtos.ModelFilters;

public abstract record BaseFilter
{
    /// <summary> 
    /// Collects non-null properties as eq filters with proper enum handling
    /// </summary>
    public virtual Dictionary<string, string?> CustomQueryParameters()
    {
        var dict = new Dictionary<string, string?>();

        foreach (var prop in GetType().GetProperties())
        {
            var value = prop.GetValue(this);

            if (value == null) continue;

            string? stringValue;
            if (prop.PropertyType.IsEnum)
            {
                // Handle direct enum values
                stringValue = value.ToString();
            }
            else if (prop.PropertyType.IsGenericType &&
                     prop.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>) &&
                     Nullable.GetUnderlyingType(prop.PropertyType)?.IsEnum == true)
            {
                // Handle nullable enum values (Gender?, Employee_Type?, etc.)
                stringValue = value.ToString();
            }
            else
            {
                // Handle regular string and other types
                stringValue = value.ToString();
            }

            if (!string.IsNullOrWhiteSpace(stringValue))
            {
                dict[prop.Name] = stringValue;
            }
        }

        return dict;
    }

    public virtual Dictionary<string, string?> CustomQueryParameters_()
    {
        var dict = new Dictionary<string, string?>();

        foreach (var prop in GetType().GetProperties())
        {
            if (!prop.CanRead) // Ensure property is readable
            {
                continue;
            }

            if (prop.GetCustomAttributes(typeof(JsonIgnoreAttribute), false).Length > 0) // Skip properties with JsonIgnore
            {
                continue;
            }

            if (prop.GetIndexParameters().Length > 0) // Skip indexed properties
            {
                continue;
            }

            //var propValue = prop.GetValue(this).ToString();
            var propValue = prop.GetValue(this);

            if (propValue == null)
            {
                continue; // Skip null properties
            }

            string? stringValue;

            // Type-specific formatting
            switch (propValue)
            {
                case DateTime dateTimeValue:
                    // Use a standard, culture-invariant format. ISO 8601 date-only is common.
                    // Or "s" for a sortable format with time: "yyyy-MM-ddTHH:mm:ss"
                    stringValue = dateTimeValue.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                    break;
                case bool boolValue:
                    // Consistent lowercase boolean representation
                    stringValue = boolValue.ToString().ToLowerInvariant();
                    break;
                // Add other type-specific conversions if needed (e.g., for enums to string/int)
                default:
                    stringValue = propValue.ToString();
                    break;
            }

            if (!string.IsNullOrWhiteSpace(stringValue))
            {
                // For more advanced scenarios, you could use an attribute to define the key
                // e.g., var keyName = prop.GetCustomAttribute<QueryParameterNameAttribute>()?.Name ?? prop.Name;
                dict[prop.Name] = stringValue;
            }
        }

        return dict;
    }
}

