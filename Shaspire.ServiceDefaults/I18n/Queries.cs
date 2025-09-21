using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Shaspire.ServiceDefaults.Models;

namespace Shaspire.ServiceDefaults.I18n;

public class GetLocalizationQuery : IRequest<PagedResults<EntityTranslationDto>>
{
    public int page { get; set; } = 1;
    public int pageSize { get; set; } = 10;
    public string? Language { get; set; }
}

public class GetLocalizationByPropertyQuery : IRequest<IList<EntityTranslationDto>>
{
    public string PropertyName { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
}

internal class GetLocalizationQueryHandler(II18nRepository i18NRepository, ICultureRepository cultureRepository)
    : IRequestHandler<GetLocalizationQuery, PagedResults<EntityTranslationDto>>
{
    public async Task<PagedResults<EntityTranslationDto>> Handle(GetLocalizationQuery request, CancellationToken cancellationToken)
    {
        var culture = await cultureRepository.SingleOrDefaultAsync(
            cultureRepository.GetQueryableSet().Where(c => c.Name == request.Language)
        ) ?? throw new NotFoundException($"Culture '{request.Language}' not found.");
        var query = i18NRepository.GetQueryableSet().Where(t => t.CultureId == culture.Id).Select(x => x.ToDto());

        return await query.ApplyPaging(request.page, request.pageSize);
    }
}

internal class GetLocalizationByPropertyQueryHandler(II18nRepository i18NRepository)
    : IRequestHandler<GetLocalizationByPropertyQuery, IList<EntityTranslationDto>>
{
    public async Task<IList<EntityTranslationDto>> Handle(GetLocalizationByPropertyQuery request, CancellationToken cancellationToken)
    {
        var translations = await i18NRepository.ToListAsync(
            i18NRepository.GetQueryableSet().Where(t => t.PropertyName == request.PropertyName && t.EntityType == request.EntityType)
            .Include(t => t.Culture.ToStatsDto())
        );
        return translations.ToDto().ToList() ?? [];
    }
}
