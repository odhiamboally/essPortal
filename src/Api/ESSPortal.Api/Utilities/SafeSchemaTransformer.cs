using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

using Serilog;

namespace ESSPortal.Api.Utilities;

/// <summary>
/// Schema transformer to handle problematic types
/// </summary>
public class SafeSchemaTransformer : IOpenApiSchemaTransformer
{
    public Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context, CancellationToken cancellationToken)
    {
        try
        {
            if (schema == null) return Task.CompletedTask;

            // In OpenApi 3.0+, Properties is never null - it's pre-initialized
            //schema.Properties ??= new Dictionary<string, OpenApiSchema>();

            // Check if Properties exists before processing
            if (schema.Properties != null)
            {
                var problematicKeys = schema.Properties
                    .Where(kvp => string.IsNullOrWhiteSpace(kvp.Key) || kvp.Value == null)
                    .Select(kvp => kvp.Key)
                    .ToList();

                foreach (var key in problematicKeys)
                {
                    schema.Properties.Remove(key);
                }
            }

            if (schema.AdditionalPropertiesAllowed && schema.AdditionalProperties != null)
            {
                // In OpenApi 3.0+, Type is now JsonSchemaType enum, not string
                if (schema.AdditionalProperties.Type == JsonSchemaType.Null || schema.AdditionalProperties.Type == default)
                {
                    schema.AdditionalProperties = null;
                }

                //if (string.IsNullOrWhiteSpace(schema.AdditionalProperties.Type))
                //{
                //    schema.AdditionalProperties = null;
                //}
            }
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Error in schema transformation for type {TypeName}", context?.JsonTypeInfo?.Type?.Name);

            throw;
        }

        return Task.CompletedTask;
    }
}
