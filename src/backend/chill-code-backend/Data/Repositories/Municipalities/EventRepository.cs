using ChillCodeBackend.Data.Db;
using ChillCodeBackend.Data.Repositories;
using ChillCodeBackend.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChillCodeBackend.Data.Repositories.Municipalities
{
    public class EventRepository : IEventRepository
    {
        private readonly ChillCodeDbContext _dbContext;

        public EventRepository(ChillCodeDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Event?> GetByIdentifierAsync(
            string identifier,
            CancellationToken cancellationToken = default
        )
        {
            return await _dbContext.Events.FirstOrDefaultAsync(
                e => e.Identifier == identifier,
                cancellationToken
            );
        }

        public async Task AddOrUpdateAsync(Event evt, CancellationToken cancellationToken = default)
        {
            await UpsertManyAsync(new[] { evt }, cancellationToken);
        }

        public async Task<UpsertBatchResult> UpsertManyAsync(
            IEnumerable<Event> events,
            CancellationToken cancellationToken = default
        )
        {
            var incoming = events.Where(e => !string.IsNullOrWhiteSpace(e.Identifier)).ToList();
            if (incoming.Count == 0)
            {
                return new UpsertBatchResult(0, 0);
            }

            var identifiers = incoming.Select(e => e.Identifier).Distinct().ToList();
            var existing = await _dbContext
                .Events.Where(e => identifiers.Contains(e.Identifier))
                .ToListAsync(cancellationToken);
            var existingById = existing.ToDictionary(e => e.Identifier);

            var now = DateTimeOffset.UtcNow;
            var inserted = 0;
            var updated = 0;

            var previousAutoDetectChanges = _dbContext.ChangeTracker.AutoDetectChangesEnabled;
            _dbContext.ChangeTracker.AutoDetectChangesEnabled = false;
            try
            {
                foreach (var evt in incoming)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (!existingById.TryGetValue(evt.Identifier, out var existingEvent))
                    {
                        evt.CreatedAt = evt.CreatedAt == default ? now : evt.CreatedAt;
                        evt.UpdatedAt = now;
                        await _dbContext.Events.AddAsync(evt, cancellationToken);
                        inserted++;
                        continue;
                    }

                    var createdAt = existingEvent.CreatedAt;
                    _dbContext.Entry(existingEvent).CurrentValues.SetValues(evt);
                    existingEvent.NearestCarPark = evt.NearestCarPark;
                    existingEvent.Organizer = evt.Organizer;
                    existingEvent.MunicipalityData = evt.MunicipalityData;
                    existingEvent.CreatedAt = createdAt == default ? now : createdAt;
                    existingEvent.UpdatedAt = now;
                    _dbContext.Entry(existingEvent).Property(e => e.CreatedAt).IsModified = false;
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
            var existingEvent = await _dbContext.Events.FirstOrDefaultAsync(
                e => e.Identifier == identifier,
                cancellationToken
            );

            if (existingEvent is not null)
            {
                _dbContext.Events.Remove(existingEvent);
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task<IReadOnlyCollection<Event>> GetAllAsync(
            CancellationToken cancellationToken = default
        )
        {
            return await _dbContext.Events.ToListAsync(cancellationToken);
        }
    }
}
