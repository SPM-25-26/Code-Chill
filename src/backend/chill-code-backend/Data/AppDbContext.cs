using ChillCodeBackend.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChillCodeBackend.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // Users already exist
        public DbSet<User> Users { get; set; } = default!;

        // Add new entities
        public DbSet<Municipality> Municipalities { get; set; } = default!;
        public DbSet<Article> Articles { get; set; } = default!;
        public DbSet<Event> Events { get; set; } = default!;
        public DbSet<Models.Entities.Route> Routes { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Optional: configure entity properties if needed
            modelBuilder.Entity<Municipality>().HasKey(m => m.Name);
            modelBuilder.Entity<Article>().HasKey(a => a.Identifier);
            modelBuilder.Entity<Event>().HasKey(e => e.Identifier);
            modelBuilder.Entity<Models.Entities.Route>().HasKey(r => r.Name);

            // Relationships if needed
            // For example, Event has Municipality
            modelBuilder.Entity<Event>()
                .HasOne(e => e.MunicipalityData)
                .WithMany()
                .HasForeignKey("MunicipalityName"); // shadow property FK

            modelBuilder.Entity<Article>()
                .HasOne(a => a.MunicipalityData)
                .WithMany()
                .HasForeignKey("MunicipalityName"); // shadow property FK

            modelBuilder.Entity<Models.Entities.Route>()
                .HasOne(r => r.MunicipalityData)
                .WithMany()
                .HasForeignKey("MunicipalityName"); // shadow property FK
        }
    }
}
