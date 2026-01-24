using ChillCodeBackend.Models.Entities;

namespace ChillCodeBackend.Data.Repositories.Municipalities
{
    public interface IServiceRepository
    {
        Task<Service?> GetByIdentifierAsync(string identifier, string? language = null, CancellationToken ct = default);
        Task<IReadOnlyCollection<Service>> GetAllAsync(CancellationToken ct);
        Task<IReadOnlyCollection<Service>> GetByMunicipalityAsync(
            string municipality,
            string? language = null,
            CancellationToken ct = default
        );
        Task AddOrUpdateAsync(Service service, CancellationToken ct);
        Task<UpsertBatchResult> UpsertManyAsync(IEnumerable<Service> services, CancellationToken ct);
    }
}
