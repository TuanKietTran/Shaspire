using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ServiceDiscovery;
using Microsoft.OpenApi.Models;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using Shaspire.ServiceDefaults.I18n;
using Shaspire.ServiceDefaults.Models;
using Shaspire.ServiceDefaults.Repositories;
using Shaspire.ServiceDefaults.Webs;

namespace Microsoft.Extensions.Hosting;

// Adds common .NET Aspire services: service discovery, resilience, health checks, and OpenTelemetry.
// This project should be referenced by each service project in your solution.
// To learn more about using this project, see https://aka.ms/dotnet/aspire/service-defaults
public static class Extensions
{
    public static TBuilder AddServiceDefaults<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(IStartup).Assembly));
        builder.ConfigureOpenTelemetry();

        builder.AddDefaultHealthChecks();
        
        builder.ConfigureOpenApi();

        builder.Services.AddServiceDiscovery();

        builder.Services.AddDateTimeProvider();
        
        builder.Services.ConfigureHttpClientDefaults(http =>
        {
            // Turn on resilience by default
            http.AddStandardResilienceHandler();

            // Turn on service discovery by default
            http.AddServiceDiscovery();
        });

        // Uncomment the following to restrict the allowed schemes for service discovery.
        // builder.Services.Configure<ServiceDiscoveryOptions>(options =>
        // {
        //     options.AllowedSchemes = ["https"];
        // });

        return builder;
    }
    public static TBuilder ConfigureOpenTelemetry<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        builder.Logging.AddOpenTelemetry(logging =>
        {
            logging.IncludeFormattedMessage = true;
            logging.IncludeScopes = true;
        });

        builder.Services.AddOpenTelemetry()
            .WithMetrics(metrics =>
            {
                metrics.AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation();
            })
            .WithTracing(tracing =>
            {
                tracing.AddSource(builder.Environment.ApplicationName)
                    .AddAspNetCoreInstrumentation()
                    // Uncomment the following line to enable gRPC instrumentation (requires the OpenTelemetry.Instrumentation.GrpcNetClient package)
                    //.AddGrpcClientInstrumentation()
                    .AddHttpClientInstrumentation();
            });

        builder.AddOpenTelemetryExporters();

        return builder;
    }

    private static TBuilder AddOpenTelemetryExporters<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        var useOtlpExporter = !string.IsNullOrWhiteSpace(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);

        if (useOtlpExporter)
        {
            builder.Services.AddOpenTelemetry().UseOtlpExporter();
        }

        // Uncomment the following lines to enable the Azure Monitor exporter (requires the Azure.Monitor.OpenTelemetry.AspNetCore package)
        //if (!string.IsNullOrEmpty(builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]))
        //{
        //    builder.Services.AddOpenTelemetry()
        //       .UseAzureMonitor();
        //}

        return builder;
    }

    public static TBuilder AddDefaultHealthChecks<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        builder.Services.AddHealthChecks()
            // Add a default liveness check to ensure app is responsive
            .AddCheck("self", () => HealthCheckResult.Healthy(), ["live"]);

        return builder;
    }

    public static WebApplication MapDefaultEndpoints(this WebApplication app)
    {
        // Adding health checks endpoints to applications in non-development environments has security implications.
        // See https://aka.ms/dotnet/aspire/healthchecks for details before enabling these endpoints in non-development environments.
        if (app.Environment.IsDevelopment())
        {
            // All health checks must pass for app to be considered ready to accept traffic after starting
            app.MapHealthChecks("/health");

            // Only health checks tagged with the "live" tag must pass for app to be considered alive
            app.MapHealthChecks("/alive", new HealthCheckOptions
            {
                Predicate = r => r.Tags.Contains("live")
            });
        }

        return app;
    }

    public static async Task<PagedResults<T>> ApplyPaging<T>(this IQueryable<T> query, int pageNumber, int pageSize)
    {
        var totalNumberOfRecords = await query.CountAsync();
        var mod = totalNumberOfRecords % pageSize;
        var totalPageCount = (totalNumberOfRecords / pageSize) + (mod == 0 ? 0 : 1);
        var results = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
        return new PagedResults<T>
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            Results = results,
            TotalNumberOfPages = totalPageCount,
            TotalNumberOfRecords = totalNumberOfRecords
        };
    }
}


public static class OpenApiExtensions
{
    /// <summary>
    /// Configures OpenAPI services for .NET 9+
    /// </summary>
    /// <typeparam name="TBuilder">The host application builder type</typeparam>
    /// <param name="builder">The host application builder</param>
    /// <param name="documentName">The OpenAPI document name (default: "v1")</param>
    /// <param name="configureOptions">Optional configuration action</param>
    /// <returns>The builder for chaining</returns>
    public static TBuilder ConfigureOpenApi<TBuilder>(
        this TBuilder builder, 
        string documentName = "v1",
        Action<OpenApiOptions>? configureOptions = null) 
        where TBuilder : IHostApplicationBuilder
    {
        // Add OpenAPI services
        builder.Services.AddOpenApi(documentName, options =>
        {
            // Add document transformer for basic info
            options.AddDocumentTransformer<OpenApiDocumentTransformer>();
            
            // Add operation transformer for enhanced metadata
            options.AddOperationTransformer<OpenApiOperationTransformer>();
            
            // Add schema transformer for clean schemas
            options.AddSchemaTransformer<OpenApiSchemaTransformer>();
            
            // Apply custom configuration if provided
            configureOptions?.Invoke(options);
        });

        return builder;
    }

    /// <summary>
    /// Configures OpenAPI with custom document information
    /// </summary>
    /// <typeparam name="TBuilder">The host application builder type</typeparam>
    /// <param name="builder">The host application builder</param>
    /// <param name="title">API title</param>
    /// <param name="version">API version</param>
    /// <param name="description">API description</param>
    /// <param name="contactName">Contact name</param>
    /// <param name="contactEmail">Contact email</param>
    /// <param name="enableAuth">Enable JWT Bearer authentication</param>
    /// <param name="baseUrl">Base URL for the API server</param>
    /// <returns>The builder for chaining</returns>
    public static TBuilder ConfigureOpenApi<TBuilder>(
        this TBuilder builder,
        string title,
        string version,
        string? description = null,
        string? contactName = null,
        string? contactEmail = null,
        bool enableAuth = false,
        string? baseUrl = null) 
        where TBuilder : IHostApplicationBuilder
    {
        return builder.ConfigureOpenApi("v1", options =>
        {
            options.AddDocumentTransformer((document, context, cancellationToken) =>
            {
                document.Info = new OpenApiInfo
                {
                    Title = title,
                    Version = version,
                    Description = description,
                    Contact = !string.IsNullOrEmpty(contactName) || !string.IsNullOrEmpty(contactEmail) 
                        ? new OpenApiContact 
                        { 
                            Name = contactName, 
                            Email = contactEmail 
                        } 
                        : null
                };
                
                // Configure server URLs
                ConfigureServers(document, context, baseUrl);

                // Initialize components
                document.Components ??= new OpenApiComponents();
                document.Components.Schemas ??= new Dictionary<string, OpenApiSchema>();

                // Add authentication if enabled
                if (enableAuth)
                {
                    AddAuthentication(document);
                }

                // Add common response schemas
                AddCommonSchemas(document);
                
                return Task.CompletedTask;
            });
        });
    }

    /// <summary>
    /// Configures OpenAPI with security schemes
    /// </summary>
    /// <typeparam name="TBuilder">The host application builder type</typeparam>
    /// <param name="builder">The host application builder</param>
    /// <param name="enableJwtAuth">Enable JWT Bearer authentication</param>
    /// <param name="enableApiKey">Enable API Key authentication</param>
    /// <returns>The builder for chaining</returns>
    public static TBuilder ConfigureOpenApiWithAuth<TBuilder>(
        this TBuilder builder,
        bool enableJwtAuth = true,
        bool enableApiKey = false) 
        where TBuilder : IHostApplicationBuilder
    {
        return builder.ConfigureOpenApi("v1", options =>
        {
            options.AddDocumentTransformer((document, context, cancellationToken) =>
            {
                document.Components ??= new OpenApiComponents();
                document.Components.SecuritySchemes ??= new Dictionary<string, OpenApiSecurityScheme>();

                if (enableJwtAuth)
                {
                    document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
                    {
                        Type = SecuritySchemeType.Http,
                        Scheme = "bearer",
                        BearerFormat = "JWT",
                        Description = "Enter your JWT token in the format: Bearer {your_token}"
                    };
                }

                if (enableApiKey)
                {
                    document.Components.SecuritySchemes["ApiKey"] = new OpenApiSecurityScheme
                    {
                        Type = SecuritySchemeType.ApiKey,
                        In = ParameterLocation.Header,
                        Name = "X-API-Key",
                        Description = "Enter your API key"
                    };
                }

                return Task.CompletedTask;
            });
        });
    }
    
    private static void ConfigureServers(OpenApiDocument document, OpenApiDocumentTransformerContext context, string? baseUrl)
    {
        document.Servers ??= new List<OpenApiServer>();

        if (!string.IsNullOrEmpty(baseUrl))
        {
            // Use provided base URL
            document.Servers.Clear();
            document.Servers.Add(new OpenApiServer 
            { 
                Url = baseUrl.TrimEnd('/'),
                Description = "API Server"
            });
        }
        else
        {
            // Auto-detect from context or use environment-specific URLs
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
            
            switch (environment.ToLower())
            {
                case "production":
                    document.Servers.Add(new OpenApiServer 
                    { 
                        Url = "https://api.yourdomain.com",
                        Description = "Production Server"
                    });
                    break;
                case "staging":
                    document.Servers.Add(new OpenApiServer 
                    { 
                        Url = "https://staging-api.yourdomain.com",
                        Description = "Staging Server"
                    });
                    break;
                case "development":
                default:
                    // For development, try to auto-detect from configuration
                    document.Servers.Add(new OpenApiServer 
                    { 
                        Url = "https://localhost:5001",
                        Description = "Development Server (HTTPS)"
                    });
                    document.Servers.Add(new OpenApiServer 
                    { 
                        Url = "http://localhost:5000",
                        Description = "Development Server (HTTP)"
                    });
                    break;
            }
        }
    }

    private static void AddAuthentication(OpenApiDocument document)
    {
        document.Components.SecuritySchemes ??= new Dictionary<string, OpenApiSecurityScheme>();
        document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description = "Enter your JWT token in the format: Bearer {your_token}"
        };

        document.SecurityRequirements ??= new List<OpenApiSecurityRequirement>();
        document.SecurityRequirements.Add(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
    }

    private static void AddCommonSchemas(OpenApiDocument document)
    {
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
    }
}