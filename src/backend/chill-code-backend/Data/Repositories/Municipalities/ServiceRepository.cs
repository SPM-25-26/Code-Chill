using ChillCodeBackend.Data.Db;
using ChillCodeBackend.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChillCodeBackend.Data.Repositories.Municipalities
{
    public class ServiceRepository : IServiceRepository
    {
        private readonly ChillCodeDbContext _context;

        public ServiceRepository(ChillCodeDbContext context)
        {
            _context = context;
        }

        public async Task<Service?> GetByIdentifierAsync(
            string identifier,
            string? language = null,
            CancellationToken ct = default
        )
        {
            var query = _context.Services.AsNoTracking().Where(s => s.Identifier == identifier);
            if (!string.IsNullOrWhiteSpace(language))
            {
                query = query.Where(s => s.Language == language);
            }
            return await query.FirstOrDefaultAsync(ct);
        }

        public async Task<IReadOnlyCollection<Service>> GetAllAsync(CancellationToken ct)
        {
            return await _context.Services.AsNoTracking().ToListAsync(ct);
        }

        public async Task<IReadOnlyCollection<Service>> GetByMunicipalityAsync(
            string municipality,
            string? language = null,
            CancellationToken ct = default
        )
        {
            var query = _context.Services
                .AsNoTracking()
                .Where(s => s.MunicipalityData.Name == municipality);
            if (!string.IsNullOrWhiteSpace(language))
            {
                query = query.Where(s => s.Language == language);
            }
            return await query.ToListAsync(ct);
        }

        public async Task AddOrUpdateAsync(Service service, CancellationToken ct)
        {
            var existing = await _context.Services.FindAsync(
                new object[] { service.Identifier, service.Language },
                ct
            );

            if (existing == null)
            {
                service.CreatedAt = DateTimeOffset.UtcNow;
                service.UpdatedAt = DateTimeOffset.UtcNow;
                _context.Services.Add(service);
            }
            else
            {
                service.CreatedAt = existing.CreatedAt;
                service.UpdatedAt = DateTimeOffset.UtcNow;
                _context.Entry(existing).CurrentValues.SetValues(service);
                _context.Entry(existing).Property(s => s.CreatedAt).IsModified = false;
            }

            await _context.SaveChangesAsync(ct);
        }

        public async Task<UpsertBatchResult> UpsertManyAsync(
            IEnumerable<Service> services,
            CancellationToken ct
        )
        {
            var serviceList = services.ToList();
            var identifiers = serviceList.Select(s => s.Identifier).ToList();

            var existing = await _context.Services
                .Where(s => identifiers.Contains(s.Identifier))
                .ToDictionaryAsync(s => (s.Identifier, s.Language), ct);

            var inserted = 0;
            var updated = 0;

            foreach (var service in serviceList)
            {
                var key = (service.Identifier, service.Language);
                if (existing.TryGetValue(key, out var existingService))
                {
                    service.CreatedAt = existingService.CreatedAt;
                    service.UpdatedAt = DateTimeOffset.UtcNow;
                    _context.Entry(existingService).CurrentValues.SetValues(service);
                    _context.Entry(existingService).Property(s => s.CreatedAt).IsModified = false;
                    updated++;
                }
                else
                {
                    service.CreatedAt = DateTimeOffset.UtcNow;
                    service.UpdatedAt = DateTimeOffset.UtcNow;
                    _context.Services.Add(service);
                    inserted++;
                }
            }

            await _context.SaveChangesAsync(ct);
            return new UpsertBatchResult(inserted, updated);
        }
    }
}
