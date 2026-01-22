using ChillCodeBackend.Models.Entities;

namespace ChillCodeBackend.Data.Repositories.Municipalities
{
    public interface IServiceRepository
    {
        Task<Service?> GetByIdentifierAsync(string identifier, CancellationToken ct);
        Task<IReadOnlyCollection<Service>> GetAllAsync(CancellationToken ct);
        Task<IReadOnlyCollection<Service>> GetByMunicipalityAsync(
            string municipality,
            CancellationToken ct
        );
        Task AddOrUpdateAsync(Service service, CancellationToken ct);
        Task<UpsertBatchResult> UpsertManyAsync(IEnumerable<Service> services, CancellationToken ct);
    }
}
