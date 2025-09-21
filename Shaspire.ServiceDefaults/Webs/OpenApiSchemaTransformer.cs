using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;

namespace Shaspire.ServiceDefaults.Webs;

public sealed class OpenApiSchemaTransformer : IOpenApiSchemaTransformer
{
    public Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context, CancellationToken cancellationToken)
    {
        // Clean up schema references - remove any invalid characters
        if (schema.Reference?.Id != null)
        {
            schema.Reference.Id = schema.Reference.Id.Replace("#", "").Replace("/", "");
        }

        // Add examples for common types
        AddSchemaExamples(schema, context.JsonTypeInfo.Type);

        return Task.CompletedTask;
    }

    private static void AddSchemaExamples(OpenApiSchema schema, Type type)
    {
        if (schema.Example != null) return; // Don't override existing examples

        if (type == typeof(string))
        {
            schema.Example = new Microsoft.OpenApi.Any.OpenApiString("string");
        }
        else if (type == typeof(int) || type == typeof(int?))
        {
            schema.Example = new Microsoft.OpenApi.Any.OpenApiInteger(0);
        }
        else if (type == typeof(bool) || type == typeof(bool?))
        {
            schema.Example = new Microsoft.OpenApi.Any.OpenApiBoolean(true);
        }
        else if (type == typeof(DateTime) || type == typeof(DateTime?))
        {
            schema.Example = new Microsoft.OpenApi.Any.OpenApiDateTime(DateTime.UtcNow);
        }
        else if (type == typeof(decimal) || type == typeof(decimal?) || 
                 type == typeof(double) || type == typeof(double?) ||
                 type == typeof(float) || type == typeof(float?))
        {
            schema.Example = new Microsoft.OpenApi.Any.OpenApiDouble(0.0);
        }
    }
}