using MediatR;
using Microsoft.EntityFrameworkCore;
using Shaspire.ServiceDefaults.Models;

namespace Shaspire.ServiceDefaults.I18n;

public class AddTranslationCommand : IRequest<EntityTranslationDto>
{
  public int EntityId { get; set; }
  public string PropertyName { get; set; } = string.Empty;
  public string EntityType { get; set; } = string.Empty;
  public string Language { get; set; } = string.Empty;
  public string Value { get; set; } = string.Empty;
}

public class UpdateTranslationCommand : IRequest<EntityTranslationDto>
{
  public string PropertyName { get; set; } = string.Empty;
  public string EntityType { get; set; } = string.Empty;
  public string Language { get; set; } = string.Empty;
  public string Value { get; set; } = string.Empty;
}
public class DeleteTranslationCommand : IRequest
{
  public string PropertyName { get; set; } = string.Empty;
  public string EntityType { get; set; } = string.Empty;
  public string Language { get; set; } = string.Empty;
}

internal class AddTranslationCommandHandler(II18nRepository i18NRepository, ICultureRepository cultureRepository)
  : IRequestHandler<AddTranslationCommand, EntityTranslationDto>
{
  private void Validate(AddTranslationCommand request)
  {
    if (string.IsNullOrWhiteSpace(request.PropertyName))
    {
      throw new BadRequestException("Property name cannot be empty.");
    }
    if (string.IsNullOrWhiteSpace(request.EntityType))
    {
      throw new BadRequestException("Entity type cannot be empty.");
    }
    if (string.IsNullOrWhiteSpace(request.Language))
    {
      throw new BadRequestException("Language cannot be empty.");
    }
  }

  public async Task<EntityTranslationDto> Handle(AddTranslationCommand request, CancellationToken cancellationToken)
  {
    Validate(request);

    var culture = cultureRepository.GetQueryableSet()
      .FirstOrDefault(c => c.Name == request.Language) ??
      throw new NotFoundException($"Culture '{request.Language}' not found.");
    var translation = new EntityTranslation
    {
      EntityId = request.EntityId,
      PropertyName = request.PropertyName,
      EntityType = request.EntityType,
      Culture = culture,
      Value = request.Value
    };
    return (await i18NRepository.AddAsync(translation, cancellationToken)).ToDto();
  }
}
internal class UpdateTranslationCommandHandler(II18nRepository i18NRepository, ICultureRepository cultureRepository)
  : IRequestHandler<UpdateTranslationCommand, EntityTranslationDto>
{
  private void Validate(UpdateTranslationCommand request)
  {
    if (string.IsNullOrWhiteSpace(request.PropertyName))
    {
      throw new BadRequestException("Property name cannot be empty.");
    }
    if (string.IsNullOrWhiteSpace(request.EntityType))
    {
      throw new BadRequestException("Entity type cannot be empty.");
    }
    if (string.IsNullOrWhiteSpace(request.Language))
    {
      throw new BadRequestException("Language cannot be empty.");
    }
  }
  public async Task<EntityTranslationDto> Handle(UpdateTranslationCommand request, CancellationToken cancellationToken)
  {
    Validate(request);
    var culture = cultureRepository.GetQueryableSet()
      .FirstOrDefault(c => c.Name == request.Language) ??
      throw new NotFoundException($"Culture '{request.Language}' not found.");

    var translations = i18NRepository.GetQueryableSet()
      .Where(t => t.PropertyName == request.PropertyName && t.EntityType == request.EntityType);

    var translation = translations
      .Include(t => t.Culture)
      .FirstOrDefault(t => t.Culture.Name == request.Language);

    if (translation == null) {
      return (await i18NRepository.AddAsync(new EntityTranslation
      {
        PropertyName = request.PropertyName,
        EntityType = request.EntityType,
        Culture = culture,
        Value = request.Value
      }, cancellationToken)).ToDto();
    }

    translation.Value = request.Value;
    return (await i18NRepository.UpdateAsync(translation, cancellationToken)).ToDto();
  }
}

internal class DeleteTranslationCommandHandler(II18nRepository i18NRepository)
  : IRequestHandler<DeleteTranslationCommand>
{
  public async Task Handle(DeleteTranslationCommand request, CancellationToken cancellationToken)
  {
    var translations = i18NRepository.GetQueryableSet()
      .Where(t => t.PropertyName == request.PropertyName && t.EntityType == request.EntityType);
    var translation = translations
      .Include(t => t.Culture)
      .FirstOrDefault(t => t.Culture.Name == request.Language);
    if (translation != null)
    {
      await i18NRepository.DeleteAsync(translation);
    }
  }
}