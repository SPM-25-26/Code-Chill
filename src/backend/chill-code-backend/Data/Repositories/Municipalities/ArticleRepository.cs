using ChillCodeBackend.Data.Db;
using ChillCodeBackend.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChillCodeBackend.Data.Repositories.Municipalities
{

    public class ArticleRepository : IArticleStore
    {
        private readonly ChillCodeDbContext _dbContext;

        public ArticleRepository(ChillCodeDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Article?> GetByIdentifierAsync(string identifier, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Articles.FirstOrDefaultAsync(
                a => a.Identifier == identifier,
                cancellationToken
            );
        }

        public async Task AddOrUpdateAsync(Article article, CancellationToken cancellationToken = default)
        {
            var existingArticle = await _dbContext.Articles.FirstOrDefaultAsync(
                a => a.Identifier == article.Identifier,
                cancellationToken
            );

            if (existingArticle is null)
            {
                await _dbContext.Articles.AddAsync(article, cancellationToken);
            }
            else
            {
                _dbContext.Entry(existingArticle).CurrentValues.SetValues(article);
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
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

        public async Task<IReadOnlyCollection<Article>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _dbContext.Articles.ToListAsync(cancellationToken);
        }
    }
}
