using ChillCodeBackend.Data.Db;
using ChillCodeBackend.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChillCodeBackend.Data.Repositories
{
    public class PreferenceRepository : IPreferenceRepository
    {
        private readonly ChillCodeDbContext _dbContext;

        public PreferenceRepository(ChillCodeDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<UserPreference?> GetByUserIdAsync(
            Guid userId,
            CancellationToken cancellationToken
        )
        {
            return await _dbContext
                .UserPreferences.AsNoTracking()
                .FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);
        }

        public async Task<UserPreference> UpsertAsync(
            Guid userId,
            UserPreference preference,
            CancellationToken cancellationToken
        )
        {
            var existing = await _dbContext.UserPreferences.FirstOrDefaultAsync(
                p => p.UserId == userId,
                cancellationToken
            );

            if (existing is null)
            {
                preference.UserId = userId;
                preference.CreatedAt = DateTimeOffset.UtcNow;
                preference.UpdatedAt = preference.CreatedAt;
                await _dbContext.UserPreferences.AddAsync(preference, cancellationToken);
            }
            else
            {
                existing.TravelVibe = preference.TravelVibe;
                existing.ActivityLevel = preference.ActivityLevel;
                existing.CitySize = preference.CitySize;
                existing.Interests = preference.Interests;
                existing.UpdatedAt = DateTimeOffset.UtcNow;
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            return existing ?? preference;
        }
    }
}
