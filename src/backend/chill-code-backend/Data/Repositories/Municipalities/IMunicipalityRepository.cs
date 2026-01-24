using ChillCodeBackend.Models.Entities;

namespace ChillCodeBackend.Data.Repositories.Municipalities
{
    public interface IMunicipalityRepository
    {
        Task<Municipality?> GetByNameAsync(
            string name,
            string? language = null,
            CancellationToken cancellationToken = default
        );
        Task<Municipality?> GetByExternalIdAsync(
            string externalId,
            string? language = null,
            CancellationToken cancellationToken = default
        );
        Task AddOrUpdateAsync(
            Municipality municipality,
            CancellationToken cancellationToken = default
        );
        Task<bool> UpsertAsync(
            Municipality municipality,
            CancellationToken cancellationToken = default
        );
        Task<IReadOnlyCollection<Municipality>> GetAllAsync(
            CancellationToken cancellationToken = default
        );
    }
}
