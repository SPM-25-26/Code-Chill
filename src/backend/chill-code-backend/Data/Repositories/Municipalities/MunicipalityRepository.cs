using ChillCodeBackend.Data.Db;
using ChillCodeBackend.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChillCodeBackend.Data.Repositories.Municipalities
{
    public class MunicipalityRepository : IMunicipalityRepository
    {
        private readonly ChillCodeDbContext _dbContext;

        public MunicipalityRepository(ChillCodeDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Municipality?> GetByNameAsync(
            string name,
            string? language = null,
            CancellationToken cancellationToken = default
        )
        {
            var query = _dbContext.Municipalities.AsQueryable();
            query = query.Where(m => m.Name == name);
            if (!string.IsNullOrWhiteSpace(language))
            {
                query = query.Where(m => m.Language == language);
            }
            return await query.FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<Municipality?> GetByExternalIdAsync(
            string externalId,
            string? language = null,
            CancellationToken cancellationToken = default
        )
        {
            var query = _dbContext.Municipalities.AsQueryable();
            query = query.Where(m => m.ExternalId == externalId);
            if (!string.IsNullOrWhiteSpace(language))
            {
                query = query.Where(m => m.Language == language);
            }
            return await query.FirstOrDefaultAsync(cancellationToken);
        }

        public async Task AddOrUpdateAsync(
            Municipality municipality,
            CancellationToken cancellationToken = default
        )
        {
            await UpsertAsync(municipality, cancellationToken);
        }

        public async Task<bool> UpsertAsync(
            Municipality municipality,
            CancellationToken cancellationToken = default
        )
        {
            if (string.IsNullOrWhiteSpace(municipality.ExternalId))
            {
                throw new ArgumentException(
                    "Municipality ExternalId is required for upsert.",
                    nameof(municipality)
                );
            }

            var existingMunicipality = await _dbContext.Municipalities.FirstOrDefaultAsync(
                m => m.ExternalId == municipality.ExternalId && m.Language == municipality.Language,
                cancellationToken
            );

            var now = DateTimeOffset.UtcNow;
            if (existingMunicipality is null)
            {
                municipality.CreatedAt =
                    municipality.CreatedAt == default ? now : municipality.CreatedAt;
                municipality.UpdatedAt = now;
                await _dbContext.Municipalities.AddAsync(municipality, cancellationToken);
                await _dbContext.SaveChangesAsync(cancellationToken);
                return true;
            }

            var createdAt = existingMunicipality.CreatedAt;
            _dbContext.Entry(existingMunicipality).CurrentValues.SetValues(municipality);
            existingMunicipality.Contacts = municipality.Contacts;
            existingMunicipality.CreatedAt = createdAt == default ? now : createdAt;
            existingMunicipality.UpdatedAt = now;
            _dbContext.Entry(existingMunicipality).Property(m => m.CreatedAt).IsModified = false;

            await _dbContext.SaveChangesAsync(cancellationToken);
            return false;
        }

        public async Task<IReadOnlyCollection<Municipality>> GetAllAsync(
            CancellationToken cancellationToken = default
        )
        {
            return await _dbContext.Municipalities.ToListAsync(cancellationToken);
        }
    }
}
