using ChillCodeBackend.Models.Entities;

namespace ChillCodeBackend.Data.Repositories.Municipalities
{
    public interface IOrganizationRepository
    {
        Task<Organization?> GetByTaxCodeAsync(string taxCode, string? language = null, CancellationToken ct = default);
        Task<IReadOnlyCollection<Organization>> GetAllAsync(CancellationToken ct);
        Task<IReadOnlyCollection<Organization>> GetByMunicipalityAsync(
            string municipality,
            string? language = null,
            CancellationToken ct = default
        );
        Task AddOrUpdateAsync(Organization organization, CancellationToken ct);
        Task<UpsertBatchResult> UpsertManyAsync(
            IEnumerable<Organization> organizations,
            CancellationToken ct
        );
    }
}
