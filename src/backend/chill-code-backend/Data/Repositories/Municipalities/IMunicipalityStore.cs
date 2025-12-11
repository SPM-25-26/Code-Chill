using ChillCodeBackend.Models.Entities;

namespace ChillCodeBackend.Data.Repositories.Municipalities
{

    public interface IMunicipalityStore
    {
        Task<Municipality?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
        Task AddOrUpdateAsync(Municipality municipality, CancellationToken cancellationToken = default);
        Task<IReadOnlyCollection<Municipality>> GetAllAsync(CancellationToken cancellationToken = default);

    }
}