using System.Globalization;
using System.Text.Json.Serialization;

namespace EssPortal.Web.Blazor.Dtos.ModelFilters;

public abstract record BaseFilter
{
    public virtual Dictionary<string, string?> CustomQueryParameters()
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

