using System.Data;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Shaspire.ServiceDefaults.Repositories;

public class DbContextUnitOfWork<TDbContext>(DbContextOptions<TDbContext> options) : DbContext(options), IUnitOfWork
    where TDbContext : DbContext
{
  private IDbContextTransaction _dbContextTransaction = null!;

  public async Task<IDisposable> BeginTransactionAsync(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted, CancellationToken cancellationToken = default)
  {
    _dbContextTransaction = await Database.BeginTransactionAsync(cancellationToken);
    return _dbContextTransaction;
  }

  public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
  {
    await _dbContextTransaction.CommitAsync(cancellationToken);
  }

  protected override void OnModelCreating(ModelBuilder builder)
  {
    base.OnModelCreating(builder);
    builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
  }
}

public interface IUnitOfWork : IDisposable
{
  Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
  Task<IDisposable> BeginTransactionAsync(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted, CancellationToken cancellationToken = default);
  Task CommitTransactionAsync(CancellationToken cancellationToken = default);
}