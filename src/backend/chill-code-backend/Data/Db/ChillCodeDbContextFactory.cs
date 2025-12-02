using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ChillCodeBackend.Data.Db;

public class ChillCodeDbContextFactory : IDesignTimeDbContextFactory<ChillCodeDbContext>
{
    public ChillCodeDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ChillCodeDbContext>();
        // Use the same default connection as configured in appsettings.json for design-time tooling.
        optionsBuilder.UseSqlite("Data Source=App_Data/chillcode.db");
        return new ChillCodeDbContext(optionsBuilder.Options);
    }
}
