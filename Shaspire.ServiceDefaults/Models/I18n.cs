
namespace Shaspire.ServiceDefaults.Models;
public class EntityTranslation: IAggregateRoot
{
  public int Id { get; set; }
  public required string EntityType { get; set; }
  public int EntityId { get; set; }
  public int CultureId { get; set; }
  public required Culture Culture { get; set; }
  public required string PropertyName { get; set; }
  public required string Value { get; set; }
}

public class Culture: IAggregateRoot
{
  public int Id { get; set; }
  public required string Code { get; set; }
  public required string Name { get; set; }
  public ICollection<EntityTranslation> Translations { get; set; } = [];
}

