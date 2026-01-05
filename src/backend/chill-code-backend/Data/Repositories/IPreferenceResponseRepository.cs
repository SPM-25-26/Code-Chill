using ChillCodeBackend.Models.Entities;

namespace ChillCodeBackend.Data.Repositories
{
    public interface IPreferenceResponseRepository
    {
        Task<PreferenceResponse?> GetLatestAsync(Guid userId, CancellationToken cancellationToken);
        Task<PreferenceResponse> UpsertAsync(
            Guid userId,
            PreferenceResponse response,
            CancellationToken cancellationToken
        );
    }
}
