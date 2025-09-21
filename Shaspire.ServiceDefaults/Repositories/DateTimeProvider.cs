using Microsoft.Extensions.DependencyInjection;

namespace Shaspire.ServiceDefaults.Repositories;

internal class DateTimeProvider : IDateTimeProvider
{
  public DateTime Now => DateTime.Now;

  public DateTime UtcNow => DateTime.UtcNow;

  public DateTimeOffset OffsetNow => DateTimeOffset.Now;

  public DateTimeOffset OffsetUtcNow => DateTimeOffset.UtcNow;
}

public interface IDateTimeProvider
{
  DateTime Now { get; }
  DateTime UtcNow { get; }
  DateTimeOffset OffsetNow { get; }
  DateTimeOffset OffsetUtcNow { get; }
}

public static class DateTimeProviderExtensions
{
  public static void AddDateTimeProvider(this IServiceCollection services)
  {
    services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
  }
}