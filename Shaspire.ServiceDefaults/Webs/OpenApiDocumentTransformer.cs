using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;

namespace Shaspire.ServiceDefaults.Webs;
public sealed class OpenApiDocumentTransformer : IOpenApiDocumentTransformer
{
    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        // Set default document info if not already set
        document.Info ??= new OpenApiInfo
        {
            Title = context.DocumentName ?? "API",
            Version = "v1",
            Description = "API documentation generated with .NET 9 OpenAPI"
        };

        // Initialize components
        document.Components ??= new OpenApiComponents();

        // Add common response schemas
        AddCommonSchemas(document);

        return Task.CompletedTask;
    }

    private static void AddCommonSchemas(OpenApiDocument document)
    {
        document.Components.Schemas ??= new Dictionary<string, OpenApiSchema>();

        // Add ProblemDetails schema
        if (!document.Components.Schemas.ContainsKey("ProblemDetails"))
        {
            document.Components.Schemas["ProblemDetails"] = new OpenApiSchema
            {
                Type = "object",
                Properties = new Dictionary<string, OpenApiSchema>
                {
                    ["type"] = new OpenApiSchema { Type = "string", Nullable = true },
                    ["title"] = new OpenApiSchema { Type = "string", Nullable = true },
                    ["status"] = new OpenApiSchema { Type = "integer", Format = "int32", Nullable = true },
                    ["detail"] = new OpenApiSchema { Type = "string", Nullable = true },
                    ["instance"] = new OpenApiSchema { Type = "string", Nullable = true }
                },
                AdditionalPropertiesAllowed = true
            };
        }

        // Add ValidationProblemDetails schema
        if (!document.Components.Schemas.ContainsKey("ValidationProblemDetails"))
        {
            document.Components.Schemas["ValidationProblemDetails"] = new OpenApiSchema
            {
                Type = "object",
                AllOf = new List<OpenApiSchema>
                {
                    new OpenApiSchema
                    {
                        Reference = new OpenApiReference
                        {
                            Id = "ProblemDetails",
                            Type = ReferenceType.Schema
                        }
                    }
                },
                Properties = new Dictionary<string, OpenApiSchema>
                {
                    ["errors"] = new OpenApiSchema
                    {
                        Type = "object",
                        AdditionalProperties = new OpenApiSchema
                        {
                            Type = "array",
                            Items = new OpenApiSchema { Type = "string" }
                        },
                        Nullable = true
                    }
                }
            };
        }
    }
}
