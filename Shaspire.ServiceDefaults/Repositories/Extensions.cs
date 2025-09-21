using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shaspire.ServiceDefaults.Models;
using Shaspire.ServiceDefaults.Repositories;

namespace Shaspire.ServiceDefaults.Repositories;

public static class RepositoryExtensions
{
    /// <summary>
    /// Registers the repository pattern services with DI container
    /// </summary>
    /// <param name="builder">The host application builder</param>
    /// <param name="assemblies">Additional assemblies to scan for aggregate roots (optional)</param>
    /// <returns>The builder for chaining</returns>
    public static IHostApplicationBuilder AddRepositories(
        this IHostApplicationBuilder builder, 
        params Assembly[] assemblies)
    {
        var services = builder.Services;
        
        // Register Unit of Work
        services.AddScoped<IUnitOfWork>(provider => 
            provider.GetRequiredService<ApplicationDbContext>());

        // Get assemblies to scan - default to executing assembly + additional ones
        var assembliesToScan = new List<Assembly> { Assembly.GetExecutingAssembly() };
        if (assemblies?.Length > 0)
        {
            assembliesToScan.AddRange(assemblies);
        }

        // Auto-register repositories for all aggregate roots
        RegisterRepositories(services, assembliesToScan);

        return builder;
    }

    /// <summary>
    /// Registers repositories for all IAggregateRoot implementations found in assemblies
    /// </summary>
    private static void RegisterRepositories(IServiceCollection services, IEnumerable<Assembly> assemblies)
    {
        var aggregateRootTypes = new HashSet<Type>();

        // Find all aggregate root types across all assemblies
        foreach (var assembly in assemblies)
        {
            var types = assembly.GetTypes()
                .Where(t => typeof(IAggregateRoot).IsAssignableFrom(t) 
                    && !t.IsInterface 
                    && !t.IsAbstract
                    && t.IsClass);

            foreach (var type in types)
            {
                aggregateRootTypes.Add(type);
            }
        }

        // Register repositories for each aggregate root
        foreach (var aggregateType in aggregateRootTypes)
        {
            RegisterRepositoryForType(services, aggregateType);
        }
    }

    /// <summary>
    /// Registers repository for a specific aggregate root type
    /// </summary>
    private static void RegisterRepositoryForType(IServiceCollection services, Type aggregateType)
    {
        // Register IRepository<TEntity>
        var repositoryInterfaceType = typeof(IRepository<>).MakeGenericType(aggregateType);
        var repositoryImplementationType = typeof(Repository<,>).MakeGenericType(typeof(ApplicationDbContext), aggregateType);
        
        services.AddScoped(repositoryInterfaceType, repositoryImplementationType);

        // Optional: Register concrete repository type as well
        services.AddScoped(repositoryImplementationType);
    }

    /// <summary>
    /// Manually registers a specific repository (for custom implementations)
    /// </summary>
    /// <typeparam name="TEntity">The aggregate root type</typeparam>
    /// <typeparam name="TRepository">The repository implementation type</typeparam>
    public static IServiceCollection AddRepository<TEntity, TRepository>(this IServiceCollection services)
        where TEntity : class, IAggregateRoot
        where TRepository : class, IRepository<TEntity>
    {
        services.AddScoped<IRepository<TEntity>, TRepository>();
        services.AddScoped<TRepository>();
        return services;
    }

    /// <summary>
    /// Registers a custom repository implementation
    /// </summary>
    /// <typeparam name="TEntity">The aggregate root type</typeparam>
    /// <param name="services">Service collection</param>
    /// <param name="implementationFactory">Factory to create repository instance</param>
    public static IServiceCollection AddRepository<TEntity>(
        this IServiceCollection services, 
        Func<IServiceProvider, IRepository<TEntity>> implementationFactory)
        where TEntity : class, IAggregateRoot
    {
        services.AddScoped(implementationFactory);
        return services;
    }
}
