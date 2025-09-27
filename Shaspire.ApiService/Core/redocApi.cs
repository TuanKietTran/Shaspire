using Microsoft.AspNetCore.Mvc;

namespace Shaspire.ApiService.Core;

public static class RedocApi
{
    /// <summary>
    /// Maps Redoc documentation endpoints
    /// </summary>
    /// <param name="routes">The endpoint route builder</param>
    /// <param name="routePrefix">Route prefix (default: "/doc")</param>
    /// <param name="openApiUrl">OpenAPI JSON endpoint (default: "/openapi/v1.json")</param>
    /// <returns>Route group builder for chaining</returns>
    public static RouteGroupBuilder MapRedocApi(
        this IEndpointRouteBuilder routes, 
        string routePrefix = "/doc",
        string openApiUrl = "/openapi/v1.json")
    {
        return routes.MapGroup(routePrefix)
            .MapApiEndpoints(openApiUrl)
            .WithTags("Documentation")
            .WithOpenApi();
    }

    private static RouteGroupBuilder MapApiEndpoints(this RouteGroupBuilder groups, string openApiUrl)
    {
        // Main Redoc documentation page
        groups.MapGet("/", ([FromServices] IWebHostEnvironment env, HttpContext context) =>
        {
            var baseUrl = $"{context.Request.Scheme}://{context.Request.Host}";
            var url = $"{baseUrl}{openApiUrl}";
            var title = env.ApplicationName ?? "API Documentation";
            var html = GenerateRedocHtml(title, openApiUrl);
            return Results.Content(html, "text/html");
        })
        .WithName("ReDocUI")
        .WithSummary("ReDoc API Documentation")
        .WithDescription("Interactive API documentation using ReDoc")
        .WithOpenApi()
        .ExcludeFromDescription(); // Don't show in OpenAPI spec

        // Alternative endpoint with custom title
        groups.MapGet("/custom", (string? title, HttpContext context) =>
        {
            var baseUrl = $"{context.Request.Scheme}://{context.Request.Host}";
            var url = $"{baseUrl}{openApiUrl}";
            var documentTitle = title ?? "Custom API Documentation";
            var html = GenerateRedocHtml(documentTitle, url);
            return Results.Content(html, "text/html");
        })
        .WithName("CustomReDocUI")
        .WithSummary("Custom ReDoc Documentation")
        .WithDescription("ReDoc documentation with custom title")
        .WithOpenApi()
        .ExcludeFromDescription();

        // Health check for documentation
        groups.MapGet("/health", () => new { Status = "OK", Documentation = "Available" })
            .WithName("DocHealth")
            .WithSummary("Documentation Health Check")
            .WithOpenApi()
            .ExcludeFromDescription();

        return groups;
    }

    private static string GenerateRedocHtml(string title, string openApiUrl)
    {
        // Create the theme object as a proper JSON string
        var themeJson = System.Text.Json.JsonSerializer.Serialize(new
        {
            colors = new
            {
                primary = new { main = "#3498db" },
                success = new { main = "#27ae60" },
                warning = new { main = "#f39c12" },
                error = new { main = "#e74c3c" }
            },
            typography = new
            {
                fontSize = "14px",
                lineHeight = "1.5em",
                code = new
                {
                    fontSize = "13px",
                    fontFamily = "Courier, monospace"
                },
                headings = new
                {
                    fontFamily = "Montserrat, sans-serif",
                    fontWeight = "600"
                }
            },
            sidebar = new
            {
                backgroundColor = "#f8f9fa",
                textColor = "#333"
            }
        }, new System.Text.Json.JsonSerializerOptions 
        { 
            PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase 
        });

        return $@"
<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""utf-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1"">
    <title>{title}</title>
    <style>
        body {{
            margin: 0;
            padding: 0;
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
        }}
        .loading {{
            display: flex;
            justify-content: center;
            align-items: center;
            height: 100vh;
            background: #fafafa;
        }}
        .spinner {{
            border: 4px solid #f3f3f3;
            border-top: 4px solid #3498db;
            border-radius: 50%;
            width: 40px;
            height: 40px;
            animation: spin 2s linear infinite;
        }}
        @keyframes spin {{
            0% {{ transform: rotate(0deg); }}
            100% {{ transform: rotate(360deg); }}
        }}
    </style>
</head>
<body>
    <div id=""loading"" class=""loading"">
        <div class=""spinner""></div>
    </div>
    
    <redoc 
        spec-url=""{openApiUrl}""
        scroll-y-offset=""60""
        hide-download-button=""false""
        native-scrollbars=""true""
        theme='{themeJson}'>
    </redoc>
    
    <script src=""https://cdn.redoc.ly/redoc/latest/bundles/redoc.standalone.js""></script>
    <script>
        // Hide loading spinner when Redoc is ready
        document.addEventListener('DOMContentLoaded', function() {{
            setTimeout(function() {{
                document.getElementById('loading').style.display = 'none';
            }}, 1000);
        }});
    </script>
</body>
</html>";
    }
}