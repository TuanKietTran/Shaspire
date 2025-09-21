namespace Shaspire.ServiceDefaults.Models;

public class AuditGuidRoot: Audit<Guid>, IAggregateRoot;

public class AuditIncrementRoot: Audit<int>, IAggregateRoot;

public class Audit<T>
{
  public required T Id { get; set; }
  public DateTimeOffset CreatedAt { get; set; }
  public DateTimeOffset UpdatedAt { get; set; }
  public DateTimeOffset? DeletedAt { get; set; }
}

public interface IAggregateRoot;