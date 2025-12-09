using ChillCodeBackend.Models.Entities;

namespace ChillCodeBackend.Services;

public interface IArticleStore
{
    Task<Article?> GetByIdentifierAsync(string identifier, CancellationToken cancellationToken = default);

    Task AddOrUpdateAsync(Article article, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Article>> GetAllAsync(CancellationToken cancellationToken = default);
    
}