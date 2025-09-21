
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Shaspire.ServiceDefaults.I18n;
using Shaspire.ServiceDefaults.Models;

namespace Shaspire.ApiService.Core;
public static class I18nApi
{
  public static void MapI18nApi(this IEndpointRouteBuilder routes)
  {
    routes.MapGroup("/i18n")
      .MapApiEndpoints()
      .WithTags("I18n")
      .RequireAuthorization()
      .WithOpenApi()
      .WithMetadata();
  }
  private static RouteGroupBuilder MapApiEndpoints(this RouteGroupBuilder groups)
  {
    // Map endpoints with OpenAPI metadata
    groups.MapGet("/", GetByCulture)
        .WithName("GetTranslationsByCulture")
        .WithOpenApi(operation =>
        {
            operation.Summary = "Get translations by culture";
            operation.Description = "Retrieves all translations for a specified culture.";
            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "culture",
                In = ParameterLocation.Query,
                Required = true,
                Description = "The culture code (e.g., en-US, fr-FR)",
                Schema = new OpenApiSchema { Type = "string" }
            });
            return operation;
        })
        .Produces<EntityTranslationDto[]>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest);

    groups.MapGet("/{propertyName}", GetByProperty)
        .WithName("GetTranslationByProperty")
        .WithOpenApi(operation =>
        {
            operation.Summary = "Get translation by property name";
            operation.Description = "Retrieves the translation for a specific property name across cultures.";
            operation.Parameters[0].Description = "The property name to retrieve translations for";
            return operation;
        })
        .Produces<EntityTranslationDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

    groups.MapPost("/", AddTranslation)
        .WithName("AddTranslation")
        .WithOpenApi(operation =>
        {
            operation.Summary = "Add a new translation";
            operation.Description = "Creates a new translation entry for a culture and property.";
            operation.RequestBody = new OpenApiRequestBody
            {
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    ["application/json"] = new OpenApiMediaType
                    {
                        Schema = new OpenApiSchema
                        {
                            Reference = new OpenApiReference { Id = nameof(EntityTranslationDto), Type = ReferenceType.Schema }
                        }
                    }
                }
            };
            return operation;
        })
        .Accepts<EntityTranslationDto>("application/json")
        .Produces<EntityTranslationDto>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest);

    groups.MapPut("/", UpdateTranslation)
        .WithName("UpdateTranslation")
        .WithOpenApi(operation =>
        {
            operation.Summary = "Update an existing translation";
            operation.Description = "Updates the translation value for a specific culture and property.";
            operation.RequestBody = new OpenApiRequestBody
            {
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    ["application/json"] = new OpenApiMediaType
                    {
                        Schema = new OpenApiSchema
                        {
                            Reference = new OpenApiReference { Id = nameof(EntityTranslationDto), Type = ReferenceType.Schema }
                        }
                    }
                }
            };
            return operation;
        })
        .Accepts<EntityTranslationDto>("application/json")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);

    groups.MapDelete("/", DeleteTranslation)
        .WithName("DeleteTranslation")
        .WithOpenApi(operation =>
        {
            operation.Summary = "Delete a translation";
            operation.Description = "Deletes a translation for a specific culture and property.";
            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "culture",
                In = ParameterLocation.Query,
                Required = true,
                Description = "The culture code of the translation to delete",
                Schema = new OpenApiSchema { Type = "string" }
            });
            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "propertyName",
                In = ParameterLocation.Query,
                Required = true,
                Description = "The property name of the translation to delete",
                Schema = new OpenApiSchema { Type = "string" }
            });
            return operation;
        })
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound);


    return groups;
  }

  private static async Task<IResult> GetByCulture(IMediator mediator, [FromQuery] string language)
  {
    var query = new GetLocalizationQuery { Language = language };
    var result = await mediator.Send(query);
    return TypedResults.Ok(result);
  }

  private static async Task<IResult> GetByProperty(IMediator mediator, [FromQuery] string propertyName, [FromQuery] string entityType)
  {
    var query = new GetLocalizationByPropertyQuery { PropertyName = propertyName, EntityType = entityType };
    var result = await mediator.Send(query);
    return TypedResults.Ok(result);
  }
  private static async Task<IResult> AddTranslation(IMediator mediator, [FromBody] AddTranslationCommand command)
  {
    var result = await mediator.Send(command);
    return TypedResults.Ok(result);
  }
  private static async Task<IResult> UpdateTranslation(IMediator mediator, [FromBody] UpdateTranslationCommand command)
  {
    var result = await mediator.Send(command);
    return TypedResults.Ok(result);
  }
  private static async Task<IResult> DeleteTranslation(IMediator mediator, [FromBody] DeleteTranslationCommand command)
  {
    await mediator.Send(command);
    return TypedResults.NoContent();
  }
}