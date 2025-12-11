using ChillCodeBackend.Models.Entities;

namespace ChillCodeBackend.Data.Repositories.Municipalities
{

    public interface IEventRepository
    {
        Task<Event?> GetByIdentifierAsync(string identifier, CancellationToken cancellationToken = default);
        Task AddOrUpdateAsync(Event evt, CancellationToken cancellationToken = default);
        Task DeleteByIdAsync(string identifier, CancellationToken ct = default);
        Task<IReadOnlyCollection<Event>> GetAllAsync(CancellationToken cancellationToken = default);
    }
}