using ChillCodeBackend.Models.Entities;

namespace ChillCodeBackend.Data.Repositories.Municipalities
{
    public interface IArticleRepository
    {
        Task<Article?> GetByIdentifierAsync(
            string identifier,
            CancellationToken cancellationToken = default
        );

        Task AddOrUpdateAsync(Article article, CancellationToken cancellationToken = default);

        Task DeleteByIdAsync(string identifier, CancellationToken ct = default);

        Task<IReadOnlyCollection<Article>> GetAllAsync(
            CancellationToken cancellationToken = default
        );
    }
}
