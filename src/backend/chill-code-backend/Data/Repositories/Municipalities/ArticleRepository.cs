using ChillCodeBackend.Data.Db;
using ChillCodeBackend.Data.Repositories;
using ChillCodeBackend.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChillCodeBackend.Data.Repositories.Municipalities
{
    public class ArticleRepository : IArticleRepository
    {
        private readonly ChillCodeDbContext _dbContext;

        public ArticleRepository(ChillCodeDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Article?> GetByIdentifierAsync(
            string identifier,
            string? language = null,
            CancellationToken cancellationToken = default
        )
        {
            var query = _dbContext.Articles.AsQueryable();
            query = query.Where(a => a.Identifier == identifier);
            if (!string.IsNullOrWhiteSpace(language))
            {
                query = query.Where(a => a.Language == language);
            }
            return await query.FirstOrDefaultAsync(cancellationToken);
        }

        public async Task AddOrUpdateAsync(
            Article article,
            CancellationToken cancellationToken = default
        )
        {
            await UpsertManyAsync(new[] { article }, cancellationToken);
        }

        public async Task<UpsertBatchResult> UpsertManyAsync(
            IEnumerable<Article> articles,
            CancellationToken cancellationToken = default
        )
        {
            var incoming = articles.Where(a => !string.IsNullOrWhiteSpace(a.Identifier)).ToList();
            if (incoming.Count == 0)
            {
                return new UpsertBatchResult(0, 0);
            }

            var identifiers = incoming.Select(a => a.Identifier).Distinct().ToList();
            var existing = await _dbContext
                .Articles.Where(a => identifiers.Contains(a.Identifier))
                .ToListAsync(cancellationToken);

            var existingByKey = existing.ToDictionary(a => (a.Identifier, a.Language));
            var now = DateTimeOffset.UtcNow;
            var inserted = 0;
            var updated = 0;

            var previousAutoDetectChanges = _dbContext.ChangeTracker.AutoDetectChangesEnabled;
            _dbContext.ChangeTracker.AutoDetectChangesEnabled = false;
            try
            {
                foreach (var article in incoming)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var key = (article.Identifier, article.Language);
                    if (!existingByKey.TryGetValue(key, out var existingArticle))
                    {
                        article.CreatedAt = article.CreatedAt == default ? now : article.CreatedAt;
                        article.UpdatedAt = now;
                        await _dbContext.Articles.AddAsync(article, cancellationToken);
                        inserted++;
                        continue;
                    }

                    var createdAt = existingArticle.CreatedAt;
                    _dbContext.Entry(existingArticle).CurrentValues.SetValues(article);
                    existingArticle.MunicipalityData = article.MunicipalityData;
                    existingArticle.CreatedAt = createdAt == default ? now : createdAt;
                    existingArticle.UpdatedAt = now;
                    _dbContext.Entry(existingArticle).Property(a => a.CreatedAt).IsModified = false;
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

        public async Task DeleteByIdAsync(string identifier, CancellationToken ct = default)
        {
            var existingArticle = await _dbContext.Articles.FirstOrDefaultAsync(
                a => a.Identifier == identifier,
                ct
            );

            if (existingArticle is not null)
            {
                _dbContext.Articles.Remove(existingArticle);
                await _dbContext.SaveChangesAsync(ct);
            }
        }

        public async Task<IReadOnlyCollection<Article>> GetAllAsync(
            CancellationToken cancellationToken = default
        )
        {
            return await _dbContext.Articles.ToListAsync(cancellationToken);
        }
    }
}
