using ChillCodeBackend.Models.Entities;

namespace ChillCodeBackend.Services;

public interface IEventStore
{
    Task<Event?> GetByIdentifierAsync(string identifier, CancellationToken cancellationToken = default);
    Task AddOrUpdateAsync(Event evt, CancellationToken cancellationToken = default);
    Task DeleteByIdAsync(string identifier, CancellationToken ct = default);
    Task<IReadOnlyCollection<Event>> GetAllAsync(CancellationToken cancellationToken = default);
}