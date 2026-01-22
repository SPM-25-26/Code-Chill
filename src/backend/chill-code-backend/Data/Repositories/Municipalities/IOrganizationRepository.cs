using ChillCodeBackend.Models.Entities;

namespace ChillCodeBackend.Data.Repositories.Municipalities
{
    public interface IOrganizationRepository
    {
        Task<Organization?> GetByTaxCodeAsync(string taxCode, CancellationToken ct);
        Task<IReadOnlyCollection<Organization>> GetAllAsync(CancellationToken ct);
        Task<IReadOnlyCollection<Organization>> GetByMunicipalityAsync(
            string municipality,
            CancellationToken ct
        );
        Task AddOrUpdateAsync(Organization organization, CancellationToken ct);
        Task<UpsertBatchResult> UpsertManyAsync(
            IEnumerable<Organization> organizations,
            CancellationToken ct
        );
    }
}
