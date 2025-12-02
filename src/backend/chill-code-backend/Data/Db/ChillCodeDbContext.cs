using ChillCodeBackend.Data.Db.Configurations;
using ChillCodeBackend.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChillCodeBackend.Data.Db;

public class ChillCodeDbContext : DbContext
{
    public ChillCodeDbContext(DbContextOptions<ChillCodeDbContext> options)
        : base(options) { }

    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new UserConfiguration());
    }
}
