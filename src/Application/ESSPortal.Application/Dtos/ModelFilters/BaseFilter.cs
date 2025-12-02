namespace ESSPortal.Application.Dtos.ModelFilters;

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

            string? stringValue = null;

            // Handle different value types properly
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
}