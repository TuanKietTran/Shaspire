using System.ComponentModel.DataAnnotations;

namespace Shaspire.ServiceDefaults.I18n;

/// <summary>
/// Culture information without circular references
/// </summary>
/// <param name="Id">Culture unique identifier</param>
/// <param name="Code">Culture code (e.g., "en-US", "fr-FR")</param>
/// <param name="Name">Culture display name</param>
public record CultureDto(int Id, string Code, string Name);

/// <summary>
/// Culture with translation count
/// </summary>
/// <param name="Id">Culture unique identifier</param>
/// <param name="Code">Culture code</param>
/// <param name="Name">Culture display name</param>
/// <param name="TranslationCount">Number of translations for this culture</param>
public record CultureWithStatsDto(int Id, string Code, string Name, int TranslationCount);

/// <summary>
/// Entity translation information
/// </summary>
/// <param name="Id">Translation unique identifier</param>
/// <param name="EntityType">Type of the translated entity</param>
/// <param name="EntityId">ID of the translated entity</param>
/// <param name="CultureId">Culture identifier</param>
/// <param name="CultureCode">Culture code for easy reference</param>
/// <param name="PropertyName">Name of the translated property</param>
/// <param name="Value">Translated value</param>
public record EntityTranslationDto(
    int Id, 
    string EntityType, 
    int EntityId, 
    int CultureId,
    string CultureCode,
    string PropertyName, 
    string Value
);

/// <summary>
/// Request to create a new culture
/// </summary>
/// <param name="Code">Culture code (e.g., "en-US")</param>
/// <param name="Name">Culture display name</param>
public record CreateCultureRequest(
    [property: Required, StringLength(10, MinimumLength = 2)]
    string Code,
    
    [property: Required, StringLength(100, MinimumLength = 2)]
    string Name
);

/// <summary>
/// Request to create a new translation
/// </summary>
/// <param name="EntityType">Type of the entity to translate</param>
/// <param name="EntityId">ID of the entity to translate</param>
/// <param name="CultureId">Target culture ID</param>
/// <param name="PropertyName">Property name to translate</param>
/// <param name="Value">Translation value</param>
public record CreateTranslationRequest(
    [property: Required, StringLength(100)]
    string EntityType,
    
    [property: Required, Range(1, int.MaxValue)]
    int EntityId,
    
    [property: Required, Range(1, int.MaxValue)]
    int CultureId,
    
    [property: Required, StringLength(100)]
    string PropertyName,
    
    [property: Required, StringLength(1000)]
    string Value
);