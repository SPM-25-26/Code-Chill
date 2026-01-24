using ChillCodeBackend.Data.Db;
using ChillCodeBackend.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChillCodeBackend.Data.Repositories.Municipalities
{
    public class OrganizationRepository : IOrganizationRepository
    {
        private readonly ChillCodeDbContext _context;

        public OrganizationRepository(ChillCodeDbContext context)
        {
            _context = context;
        }

        public async Task<Organization?> GetByTaxCodeAsync(
            string taxCode,
            string? language = null,
            CancellationToken ct = default
        )
        {
            var query = _context.Organizations.AsNoTracking().Where(o => o.TaxCode == taxCode);
            if (!string.IsNullOrWhiteSpace(language))
            {
                query = query.Where(o => o.Language == language);
            }
            return await query.FirstOrDefaultAsync(ct);
        }

        public async Task<IReadOnlyCollection<Organization>> GetAllAsync(CancellationToken ct)
        {
            return await _context.Organizations.AsNoTracking().ToListAsync(ct);
        }

        public async Task<IReadOnlyCollection<Organization>> GetByMunicipalityAsync(
            string municipality,
            string? language = null,
            CancellationToken ct = default
        )
        {
            var query = _context.Organizations
                .AsNoTracking()
                .Where(o => o.MunicipalityData.Name == municipality);
            if (!string.IsNullOrWhiteSpace(language))
            {
                query = query.Where(o => o.Language == language);
            }
            return await query.ToListAsync(ct);
        }

        public async Task AddOrUpdateAsync(Organization organization, CancellationToken ct)
        {
            var existing = await _context.Organizations.FindAsync(
                new object[] { organization.TaxCode, organization.Language },
                ct
            );

            if (existing == null)
            {
                organization.CreatedAt = DateTimeOffset.UtcNow;
                organization.UpdatedAt = DateTimeOffset.UtcNow;
                _context.Organizations.Add(organization);
            }
            else
            {
                organization.CreatedAt = existing.CreatedAt;
                organization.UpdatedAt = DateTimeOffset.UtcNow;
                _context.Entry(existing).CurrentValues.SetValues(organization);
                _context.Entry(existing).Property(o => o.CreatedAt).IsModified = false;
            }

            await _context.SaveChangesAsync(ct);
        }

        public async Task<UpsertBatchResult> UpsertManyAsync(
            IEnumerable<Organization> organizations,
            CancellationToken ct
        )
        {
            var orgList = organizations.ToList();
            var taxCodes = orgList.Select(o => o.TaxCode).ToList();

            var existing = await _context.Organizations
                .Where(o => taxCodes.Contains(o.TaxCode))
                .ToDictionaryAsync(o => (o.TaxCode, o.Language), ct);

            var inserted = 0;
            var updated = 0;

            foreach (var org in orgList)
            {
                var key = (org.TaxCode, org.Language);
                if (existing.TryGetValue(key, out var existingOrg))
                {
                    org.CreatedAt = existingOrg.CreatedAt;
                    org.UpdatedAt = DateTimeOffset.UtcNow;
                    _context.Entry(existingOrg).CurrentValues.SetValues(org);
                    _context.Entry(existingOrg).Property(o => o.CreatedAt).IsModified = false;
                    updated++;
                }
                else
                {
                    org.CreatedAt = DateTimeOffset.UtcNow;
                    org.UpdatedAt = DateTimeOffset.UtcNow;
                    _context.Organizations.Add(org);
                    inserted++;
                }
            }

            await _context.SaveChangesAsync(ct);
            return new UpsertBatchResult(inserted, updated);
        }
    }
}
