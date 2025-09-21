using System.Text.Json;
using Shaspire.ApiService.Core;
using Shaspire.ServiceDefaults.I18n;
using Shaspire.ServiceDefaults.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddSqlServerDbContext<ApplicationDbContext>("Shaspire");
builder.Services.AddI18n();
builder.AddRepositories();
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddProblemDetails();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.ConfigureOpenApi(
    title: "Shaspire API",
    version: "v1.0.0",
    description: "Shaspire application API",
    contactName: "Development Team",
    contactEmail: "dev@shaspire.com"
);


var app = builder.Build();


// Configure the HTTP request pipeline.
app.UseExceptionHandler();

// Configure pipeline
if (app.Environment.IsDevelopment())
{
    // Add debugging endpoint to check OpenAPI JSON
    app.MapGet("/debug/openapi", async (HttpContext context) =>
    {
        try
        {
            var httpClient = new HttpClient();
            var baseUrl = $"{context.Request.Scheme}://{context.Request.Host}";
            var openApiUrl = $"{baseUrl}/openapi/v1.json";
            
            var response = await httpClient.GetAsync(openApiUrl);
            var content = await response.Content.ReadAsStringAsync();
            
            // Try to parse to validate JSON
            JsonDocument.Parse(content);
            
            return Results.Json(new
            {
                Status = "Valid",
                Url = openApiUrl,
                ContentLength = content.Length,
                FirstLines = content.Split('\n').Take(5).ToArray()
            });
        }
        catch (JsonException ex)
        {
            return Results.Json(new
            {
                Status = "Invalid JSON",
                Error = ex.Message,
                Line = ex.LineNumber,
                Position = ex.BytePositionInLine
            });
        }
        catch (Exception ex)
        {
            return Results.Json(new
            {
                Status = "Error",
                Error = ex.Message
            });
        }
    });

    // Enable the built-in OpenAPI endpoint
}

app.MapOpenApi();


app.MapDefaultEndpoints();

app.MapI18nApi();
app.MapRedocApi();

app.Run();