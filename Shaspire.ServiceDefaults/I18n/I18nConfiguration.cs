using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shaspire.ServiceDefaults.Models;

namespace Shaspire.ServiceDefaults.I18n;

public class EntityTranslationConfiguration : IEntityTypeConfiguration<EntityTranslation>
{
    public void Configure(EntityTypeBuilder<EntityTranslation> builder)
    {
        
        // builder.HasIndex(c => c.CompanyName)
        //     .HasDatabaseName("IX_Customers_CompanyName");
        //     
        // builder.HasIndex(c => c.Email)
        //     .IsUnique()
        //     .HasFilter("[Email] IS NOT NULL")
        //     .HasDatabaseName("IX_Customers_Email");
        //     
        // // Seed initial data
        // builder.HasData(
        //     new Customer 
        //     { 
        //         Id = Guid.NewGuid(), 
        //         CompanyName = "Acme Corp", 
        //         ContactName = "John Smith",
        //         Email = "john@acme.com",
        //         CreatedAt = DateTime.UtcNow 
        //     }
        // );
    }
}