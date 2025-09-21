using Microsoft.Extensions.DependencyInjection;
using Shaspire.ServiceDefaults.Models;

namespace Shaspire.ServiceDefaults.I18n;

public static class Extensions
{
    public static IServiceCollection AddI18n(this IServiceCollection services)
    {
        services.AddScoped<II18nRepository, I18nRepository>();
        services.AddScoped<ICultureRepository, CultureRepository>();
        return services;
    }
}

public static class MappingExtensions
{
    public static CultureDto ToDto(this Culture culture)
    {
        return new CultureDto(culture.Id, culture.Code, culture.Name);
    }

    public static CultureWithStatsDto ToStatsDto(this Culture culture)
    {
        return new CultureWithStatsDto(
            culture.Id, 
            culture.Code, 
            culture.Name, 
            culture.Translations?.Count ?? 0
        );
    }

    public static EntityTranslationDto ToDto(this EntityTranslation translation)
    {
        return new EntityTranslationDto(
            translation.Id,
            translation.EntityType,
            translation.EntityId,
            translation.CultureId,
            translation.Culture.Code,
            translation.PropertyName,
            translation.Value
        );
    }

    public static IEnumerable<CultureDto> ToDto(this IEnumerable<Culture> cultures)
    {
        return cultures.Select(c => c.ToDto());
    }

    public static IEnumerable<EntityTranslationDto> ToDto(this IEnumerable<EntityTranslation> translations)
    {
        return translations.Select(t => t.ToDto());
    }
}
