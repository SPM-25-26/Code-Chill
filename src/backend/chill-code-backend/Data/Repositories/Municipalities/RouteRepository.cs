using ChillCodeBackend.Data.Db;
using ChillCodeBackend.Data.Repositories;
using ChillCodeBackend.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChillCodeBackend.Data.Repositories.Municipalities
{
    public class RouteRepository : IRouteRepository
    {
        private readonly ChillCodeDbContext _dbContext;

        public RouteRepository(ChillCodeDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Models.Entities.Route?> GetByNameAsync(
            string name,
            CancellationToken cancellationToken = default
        )
        {
            return await _dbContext.Routes.FirstOrDefaultAsync(
                r => r.Name == name,
                cancellationToken
            );
        }

        public async Task<Models.Entities.Route?> GetByExternalIdAsync(
            string externalId,
            CancellationToken cancellationToken = default
        )
        {
            return await _dbContext.Routes.FirstOrDefaultAsync(
                r => r.ExternalId == externalId,
                cancellationToken
            );
        }

        public async Task AddOrUpdateAsync(
            Models.Entities.Route route,
            CancellationToken cancellationToken = default
        )
        {
            await UpsertManyAsync(new[] { route }, cancellationToken);
        }

        public async Task<UpsertBatchResult> UpsertManyAsync(
            IEnumerable<Models.Entities.Route> routes,
            CancellationToken cancellationToken = default
        )
        {
            var incoming = routes.Where(r => !string.IsNullOrWhiteSpace(r.ExternalId)).ToList();
            if (incoming.Count == 0)
            {
                return new UpsertBatchResult(0, 0);
            }

            var externalIds = incoming.Select(r => r.ExternalId).Distinct().ToList();
            var existing = await _dbContext
                .Routes.Where(r => externalIds.Contains(r.ExternalId))
                .ToListAsync(cancellationToken);
            var existingByExternalId = existing.ToDictionary(r => r.ExternalId);

            var now = DateTimeOffset.UtcNow;
            var inserted = 0;
            var updated = 0;

            var previousAutoDetectChanges = _dbContext.ChangeTracker.AutoDetectChangesEnabled;
            _dbContext.ChangeTracker.AutoDetectChangesEnabled = false;
            try
            {
                foreach (var route in incoming)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (!existingByExternalId.TryGetValue(route.ExternalId, out var existingRoute))
                    {
                        route.CreatedAt = route.CreatedAt == default ? now : route.CreatedAt;
                        route.UpdatedAt = now;
                        await _dbContext.Routes.AddAsync(route, cancellationToken);
                        inserted++;
                        continue;
                    }

                    var createdAt = existingRoute.CreatedAt;
                    _dbContext.Entry(existingRoute).CurrentValues.SetValues(route);
                    existingRoute.StartingPoint = route.StartingPoint;
                    existingRoute.MunicipalityData = route.MunicipalityData;
                    existingRoute.CreatedAt = createdAt == default ? now : createdAt;
                    existingRoute.UpdatedAt = now;
                    _dbContext.Entry(existingRoute).Property(r => r.CreatedAt).IsModified = false;
                    updated++;
                }
            }
            finally
            {
                _dbContext.ChangeTracker.AutoDetectChangesEnabled = previousAutoDetectChanges;
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            return new UpsertBatchResult(inserted, updated);
        }

        public async Task DeleteByNameAsync(string name, CancellationToken ct = default)
        {
            var existingRoute = await _dbContext.Routes.FirstOrDefaultAsync(
                r => r.Name == name,
                ct
            );

            if (existingRoute is not null)
            {
                _dbContext.Routes.Remove(existingRoute);
                await _dbContext.SaveChangesAsync(ct);
            }
        }

        public async Task<IReadOnlyCollection<Models.Entities.Route>> GetAllAsync(
            CancellationToken cancellationToken = default
        )
        {
            return await _dbContext.Routes.ToListAsync(cancellationToken);
        }
    }
}
