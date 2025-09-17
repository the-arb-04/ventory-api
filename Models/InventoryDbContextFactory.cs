using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Inventory_Tracker.Models;

// This is a special class that is only used by the dotnet-ef tools.
// It tells the tools how to create the DbContext when you run commands like 'add-migration'.
public class InventoryDbContextFactory : IDesignTimeDbContextFactory<InventoryDbContext>
{
    public InventoryDbContext CreateDbContext(string[] args)
    {
        // This manually builds the configuration to read from appsettings.json
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<InventoryDbContext>();
        
        // Get the connection string from the configuration
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        
        // Configure the DbContext to use PostgreSQL
        optionsBuilder.UseNpgsql(connectionString);

        return new InventoryDbContext(optionsBuilder.Options);
    }
}
