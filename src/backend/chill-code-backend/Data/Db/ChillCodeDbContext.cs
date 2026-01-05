using ChillCodeBackend.Data.Db.Configurations;
using ChillCodeBackend.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChillCodeBackend.Data.Db
{
    public class ChillCodeDbContext : DbContext
    {
        public ChillCodeDbContext(DbContextOptions<ChillCodeDbContext> options)
            : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Municipality> Municipalities => Set<Municipality>();
        public DbSet<Event> Events => Set<Event>();
        public DbSet<Article> Articles => Set<Article>();
        public DbSet<IngestionRun> IngestionRuns => Set<IngestionRun>();

        public DbSet<Models.Entities.Route> Routes => Set<Models.Entities.Route>();
        public DbSet<UserPreference> UserPreferences => Set<UserPreference>();
        public DbSet<PreferenceResponse> PreferenceResponses => Set<PreferenceResponse>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ChillCodeDbContext).Assembly);
        }
    }
}
