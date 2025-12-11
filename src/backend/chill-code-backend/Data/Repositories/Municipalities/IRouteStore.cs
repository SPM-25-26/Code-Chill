namespace ChillCodeBackend.Data.Repositories.Municipalities
{

    public interface IRouteStore
    {
        Task<Models.Entities.Route?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
        Task AddOrUpdateAsync(Models.Entities.Route route, CancellationToken cancellationToken = default);
        Task DeleteByNameAsync(string name, CancellationToken cancellationToken = default);
        Task<IReadOnlyCollection<Models.Entities.Route>> GetAllAsync(CancellationToken cancellationToken = default);
    }
}
