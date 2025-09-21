using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Shaspire.ServiceDefaults.Models;
namespace Shaspire.ServiceDefaults.Repositories;
public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContextUnitOfWork<ApplicationDbContext>(options) 
{
    public new DbSet<T> Set<T>() where T : class, IAggregateRoot => base.Set<T>();
    protected override void OnModelCreating(ModelBuilder builder)
    {
        var assembly = Assembly.GetExecutingAssembly();
        
        // Find all types that implement IAggregateRoot
        var entityTypes = assembly.GetTypes()
            .Where(t => typeof(IAggregateRoot).IsAssignableFrom(t) 
                && !t.IsInterface 
                && !t.IsAbstract);

        // Register each entity type with the builder
        foreach (var type in entityTypes)
        {
            // Only add if not already configured
            if (builder.Model.FindEntityType(type) == null)
            {
                builder.Entity(type);
            }
        }

        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }

}