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
            CancellationToken cancellationToken = default
        )
        {
            return await _dbContext.Municipalities.FirstOrDefaultAsync(
                m => m.Name == name,
                cancellationToken
            );
        }

        public async Task AddOrUpdateAsync(
            Municipality municipality,
            CancellationToken cancellationToken = default
        )
        {
            var existingMunicipality = await _dbContext.Municipalities.FirstOrDefaultAsync(
                m => m.Name == municipality.Name,
                cancellationToken
            );

            if (existingMunicipality is null)
            {
                await _dbContext.Municipalities.AddAsync(municipality, cancellationToken);
            }
            else
            {
                _dbContext.Entry(existingMunicipality).CurrentValues.SetValues(municipality);
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task<IReadOnlyCollection<Municipality>> GetAllAsync(
            CancellationToken cancellationToken = default
        )
        {
            return await _dbContext.Municipalities.ToListAsync(cancellationToken);
        }
    }
}
