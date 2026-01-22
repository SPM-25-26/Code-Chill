using ChillCodeBackend.Data.Repositories;
using ChillCodeBackend.Models.Entities;

namespace ChillCodeBackend.Data.Repositories.Municipalities
{
    public interface IPointOfInterestRepository
    {
        Task<PointOfInterest?> GetByIdentifierAsync(
            string identifier,
            CancellationToken cancellationToken = default
        );
        Task AddOrUpdateAsync(PointOfInterest poi, CancellationToken cancellationToken = default);
        Task<UpsertBatchResult> UpsertManyAsync(
            IEnumerable<PointOfInterest> pois,
            CancellationToken cancellationToken = default
        );
        Task DeleteByIdAsync(string identifier, CancellationToken ct = default);
        Task<IReadOnlyCollection<PointOfInterest>> GetAllAsync(
            CancellationToken cancellationToken = default
        );
        Task<IReadOnlyCollection<PointOfInterest>> GetByCategoryAsync(
            MobileCategory category,
            CancellationToken cancellationToken = default
        );
    }
}
