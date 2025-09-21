using Microsoft.EntityFrameworkCore;
using Shaspire.ServiceDefaults.Models;

namespace Shaspire.ServiceDefaults.Repositories;

public class Repository<TDbContext, TEntity>(TDbContext dbContext) 
  : IRepository<TEntity> 
  where TEntity : class, IAggregateRoot
  where TDbContext : DbContext, IUnitOfWork
{
  protected DbSet<TEntity> DbSet => dbContext.Set<TEntity>();
  public IUnitOfWork UnitOfWork => dbContext;

  public async Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
  {
    var result = await DbSet.AddAsync(entity, cancellationToken);
    await dbContext.SaveChangesAsync(cancellationToken);
    return result.Entity;
  }

  public Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
  {
    DbSet.Update(entity);
    return Task.FromResult(entity);
  }

  public Task DeleteAsync(TEntity entity)
  {
    DbSet.Remove(entity);
    return Task.CompletedTask;
  }

  public Task<T?> FirstOrDefaultAsync<T>(IQueryable<T> query)
  {
    return query.FirstOrDefaultAsync();
  }

  public IQueryable<TEntity> GetQueryableSet()
  {
    return dbContext.Set<TEntity>();
  }

  public Task<T?> SingleOrDefaultAsync<T>(IQueryable<T> query)
  {
    return query.SingleOrDefaultAsync();
  }

  public Task<List<TEntity>> ToListAsync(IQueryable<TEntity> query)
  {
    return query.ToListAsync();
  }
}

public interface IRepository<TEntity> where TEntity : IAggregateRoot
{
  IUnitOfWork UnitOfWork { get; }
  IQueryable<TEntity> GetQueryableSet();
  Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default);
  Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);
  Task DeleteAsync(TEntity entity);
  Task<T?> FirstOrDefaultAsync<T>(IQueryable<T> query);
  Task<T?> SingleOrDefaultAsync<T>(IQueryable<T> query);
  Task<List<TEntity>> ToListAsync(IQueryable<TEntity> query);
}