using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;

namespace Shaspire.ServiceDefaults.Webs;

public sealed class OpenApiOperationTransformer : IOpenApiOperationTransformer
{
    public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
    {
        // Add tags based on controller name if not present
        if (operation.Tags?.Count == 0)
        {
            var controllerName = context.Description.ActionDescriptor.RouteValues["controller"];
            if (!string.IsNullOrEmpty(controllerName))
            {
                operation.Tags = new List<OpenApiTag> 
                { 
                    new OpenApiTag { Name = controllerName } 
                };
            }
        }

        // Add default responses if missing
        if (operation.Responses.Count == 0)
        {
            operation.Responses["200"] = new OpenApiResponse
            {
                Description = "Success"
            };
        }

        // Add common error responses
        AddCommonErrorResponses(operation);

        return Task.CompletedTask;
    }

    private static void AddCommonErrorResponses(OpenApiOperation operation)
    {
        // Add 400 Bad Request for POST/PUT operations
        if ((operation.RequestBody != null || operation.Parameters?.Any(p => p.In == ParameterLocation.Query) == true) 
            && !operation.Responses.ContainsKey("400"))
        {
            operation.Responses["400"] = new OpenApiResponse
            {
                Description = "Bad Request",
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    ["application/json"] = new OpenApiMediaType
                    {
                        Schema = new OpenApiSchema
                        {
                            Reference = new OpenApiReference
                            {
                                Id = "ValidationProblemDetails",
                                Type = ReferenceType.Schema
                            }
                        }
                    }
                }
            };
        }

        // Add 500 Internal Server Error
        if (!operation.Responses.ContainsKey("500"))
        {
            operation.Responses["500"] = new OpenApiResponse
            {
                Description = "Internal Server Error",
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    ["application/json"] = new OpenApiMediaType
                    {
                        Schema = new OpenApiSchema
                        {
                            Reference = new OpenApiReference
                            {
                                Id = "ProblemDetails",
                                Type = ReferenceType.Schema
                            }
                        }
                    }
                }
            };
        }
    }
}