using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Shaspire.ServiceDefaults.Repositories; // Your actual ApplicationDbContext namespace

namespace Shaspire.Migrator.SqlServer.Data;

/// <summary>
/// Design-time factory for creating ApplicationDbContext instances during migrations
/// </summary>
public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development"}.json", optional: true)
            .AddEnvironmentVariables()
            .AddCommandLine(args)
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        
        var connectionString = configuration.GetConnectionString("DefaultConnection")
                               ?? configuration.GetConnectionString("Shaspire")
                               ?? throw new InvalidOperationException("No connection string found. Please set ConnectionStrings:DefaultConnection or ConnectionStrings:Shaspire");

        optionsBuilder.UseSqlServer(connectionString, options =>
        {
            options.MigrationsAssembly("Shaspire.Migrator.SqlServer");
            options.MigrationsHistoryTable("__EFMigrationsHistory", "dbo");
        });

        // Enable sensitive data logging in development
        if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
        {
            optionsBuilder.EnableSensitiveDataLogging();
        }

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}