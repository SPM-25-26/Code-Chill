using ChillCodeBackend.Data.Db;
using ChillCodeBackend.Data.Repositories;
using ChillCodeBackend.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChillCodeBackend.Data.Repositories.Municipalities
{
    public class PointOfInterestRepository : IPointOfInterestRepository
    {
        private readonly ChillCodeDbContext _dbContext;

        public PointOfInterestRepository(ChillCodeDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<PointOfInterest?> GetByIdentifierAsync(
            string identifier,
            CancellationToken cancellationToken = default
        )
        {
            return await _dbContext.PointsOfInterest.FirstOrDefaultAsync(
                p => p.Identifier == identifier,
                cancellationToken
            );
        }

        public async Task AddOrUpdateAsync(
            PointOfInterest poi,
            CancellationToken cancellationToken = default
        )
        {
            await UpsertManyAsync(new[] { poi }, cancellationToken);
        }

        public async Task<UpsertBatchResult> UpsertManyAsync(
            IEnumerable<PointOfInterest> pois,
            CancellationToken cancellationToken = default
        )
        {
            var incoming = pois.Where(p => !string.IsNullOrWhiteSpace(p.Identifier)).ToList();
            if (incoming.Count == 0)
            {
                return new UpsertBatchResult(0, 0);
            }

            var identifiers = incoming.Select(p => p.Identifier).Distinct().ToList();
            var existing = await _dbContext
                .PointsOfInterest.Where(p => identifiers.Contains(p.Identifier))
                .ToListAsync(cancellationToken);
            var existingById = existing.ToDictionary(p => p.Identifier);

            var now = DateTimeOffset.UtcNow;
            var inserted = 0;
            var updated = 0;

            var previousAutoDetectChanges = _dbContext.ChangeTracker.AutoDetectChangesEnabled;
            _dbContext.ChangeTracker.AutoDetectChangesEnabled = false;
            try
            {
                foreach (var poi in incoming)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (!existingById.TryGetValue(poi.Identifier, out var existingPoi))
                    {
                        poi.CreatedAt = poi.CreatedAt == default ? now : poi.CreatedAt;
                        poi.UpdatedAt = now;
                        await _dbContext.PointsOfInterest.AddAsync(poi, cancellationToken);
                        inserted++;
                        continue;
                    }

                    var createdAt = existingPoi.CreatedAt;
                    _dbContext.Entry(existingPoi).CurrentValues.SetValues(poi);
                    existingPoi.NearestCarPark = poi.NearestCarPark;
                    existingPoi.MunicipalityData = poi.MunicipalityData;
                    existingPoi.CreatedAt = createdAt == default ? now : createdAt;
                    existingPoi.UpdatedAt = now;
                    _dbContext.Entry(existingPoi).Property(p => p.CreatedAt).IsModified = false;
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

        public async Task DeleteByIdAsync(
            string identifier,
            CancellationToken cancellationToken = default
        )
        {
            var existingPoi = await _dbContext.PointsOfInterest.FirstOrDefaultAsync(
                p => p.Identifier == identifier,
                cancellationToken
            );

            if (existingPoi is not null)
            {
                _dbContext.PointsOfInterest.Remove(existingPoi);
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task<IReadOnlyCollection<PointOfInterest>> GetAllAsync(
            CancellationToken cancellationToken = default
        )
        {
            return await _dbContext.PointsOfInterest.ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyCollection<PointOfInterest>> GetByCategoryAsync(
            MobileCategory category,
            CancellationToken cancellationToken = default
        )
        {
            return await _dbContext
                .PointsOfInterest.Where(p => p.Category == category)
                .ToListAsync(cancellationToken);
        }
    }
}
