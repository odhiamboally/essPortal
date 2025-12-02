using ESSPortal.Application.Dtos.ModelFilters;

namespace ESSPortal.Application.Extensions;


public static class FilterExtensions
{
    public static string BuildQueryStringFromOrderedParams(this string[] parameters, string serviceName)
    {
        // Define parameter mappings for different services
        var serviceParamMappings = new Dictionary<string, string[]>
        {
            ["LeaveRelievers"] = ["ApplicationNo", "Staff_No"],
            ["AttachedDocumentDetails"] = ["Table_ID", "No", "File_Type"],
        };

        // Extract service name from full path if needed
        var serviceKey = serviceName.Split('/').Last().Split('?').First();

        if (serviceParamMappings.TryGetValue(serviceKey, out var paramNames))
        {
            if (parameters.Length > paramNames.Length)
            {
                throw new ArgumentException($"Too many parameters for service {serviceKey}. Expected {paramNames.Length}, got {parameters.Length}");
            }

            var pairs = new List<string>();
            for (int i = 0; i < Math.Min(parameters.Length, paramNames.Length); i++)
            {
                if (!string.IsNullOrEmpty(parameters[i]))
                {
                    pairs.Add($"{Uri.EscapeDataString(paramNames[i])}={Uri.EscapeDataString(parameters[i])}");
                }
            }

            return string.Join("&", pairs);
        }

        // Fallback: use generic parameter names
        var fallbackPairs = new List<string>();
        for (int i = 0; i < parameters.Length; i++)
        {
            if (!string.IsNullOrEmpty(parameters[i]))
            {
                fallbackPairs.Add($"param{i + 1}={Uri.EscapeDataString(parameters[i])}");
            }
        }
        return string.Join("&", fallbackPairs);
    }

    public static string BuildODataFilter<TFilter>(this TFilter filter) where TFilter : BaseFilter
    {
        var pairs = filter.CustomQueryParameters();  // name → raw string

        if (!pairs.Any()) return string.Empty;

        var clauses = new List<string>();

        foreach (var kv in pairs)
        {
            var (field, op) = ParseKeyOperator(kv.Key);
            var rawValue = kv.Value!;

            // Handle different data types for proper OData formatting
            if (decimal.TryParse(rawValue, out var dec))
            {
                clauses.Add($"{field} {op} {dec}"); // numeric literal—no quotes
            }
            else if (int.TryParse(rawValue, out var i))
            {
                clauses.Add($"{field} {op} {i}");
            }
            else if (bool.TryParse(rawValue, out var b))
            {
                clauses.Add($"{field} {op} {b.ToString().ToLowerInvariant()}"); // boolean—lowercase no quotes
            }
            else if (DateTime.TryParse(rawValue, out var dt))
            {
                // Format DateTime for OData
                clauses.Add($"{field} {op} datetime'{dt:yyyy-MM-ddTHH:mm:ss}'");
            }
            else if (DateTimeOffset.TryParse(rawValue, out var dto))
            {
                clauses.Add($"{field} {op} datetimeoffset'{dto:yyyy-MM-ddTHH:mm:sszzz}'");
            }
            else if (Guid.TryParse(rawValue, out var guid))
            {
                clauses.Add($"{field} {op} guid'{guid}'");
            }
            else
            {
                // String literal - escape single quotes and wrap in quotes
                var escapedValue = rawValue.Replace("'", "''");
                clauses.Add($"{field} {op} '{escapedValue}'");
            }
        }

        return clauses.Count > 0 ? "$filter=" + string.Join(" and ", clauses) : string.Empty;
    }

    /// <summary>
    /// Supports parsing keys like "StartDate__gt" into ("StartDate", "gt")
    /// If no operator suffix, defaults to "eq"
    /// </summary>
    private static (string fieldName, string op) ParseKeyOperator(string key)
    {
        if (key.Contains("__"))
        {
            var parts = key.Split("__", 2);
            var op = parts[1] switch
            {
                "gt" => "gt",
                "ge" => "ge",
                "lt" => "lt",
                "le" => "le",
                "ne" => "ne",
                "contains" => "contains",
                "startswith" => "startswith",
                "endswith" => "endswith",
                _ => "eq"
            };
            return (parts[0], op);
        }

        return (key, "eq");
    }


}
