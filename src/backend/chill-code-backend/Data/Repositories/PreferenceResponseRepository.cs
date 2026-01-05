using ChillCodeBackend.Data.Db;
using ChillCodeBackend.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChillCodeBackend.Data.Repositories
{
    public class PreferenceResponseRepository : IPreferenceResponseRepository
    {
        private readonly ChillCodeDbContext _dbContext;

        public PreferenceResponseRepository(ChillCodeDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<PreferenceResponse?> GetLatestAsync(
            Guid userId,
            CancellationToken cancellationToken
        )
        {
            // SQLite cannot order by DateTimeOffset; sort on client after fetch.
            var responses = await _dbContext
                .PreferenceResponses.AsNoTracking()
                .Where(r => r.UserId == userId)
                .ToListAsync(cancellationToken);

            return responses.OrderByDescending(r => r.UpdatedAt).FirstOrDefault();
        }

        public async Task<PreferenceResponse> UpsertAsync(
            Guid userId,
            PreferenceResponse response,
            CancellationToken cancellationToken
        )
        {
            var existing = await _dbContext.PreferenceResponses.FirstOrDefaultAsync(
                r => r.UserId == userId,
                cancellationToken
            );

            if (existing is null)
            {
                response.UserId = userId;
                response.CreatedAt = DateTimeOffset.UtcNow;
                response.UpdatedAt = response.CreatedAt;
                await _dbContext.PreferenceResponses.AddAsync(response, cancellationToken);
            }
            else
            {
                existing.QuestionnaireVersion = response.QuestionnaireVersion;
                existing.AnswersJson = response.AnswersJson;
                existing.DerivedPreferencesJson = response.DerivedPreferencesJson;
                existing.UpdatedAt = DateTimeOffset.UtcNow;
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            return existing ?? response;
        }
    }
}
