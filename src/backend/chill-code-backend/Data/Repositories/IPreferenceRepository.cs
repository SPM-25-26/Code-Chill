using ChillCodeBackend.Models.Entities;

namespace ChillCodeBackend.Data.Repositories
{
    public interface IPreferenceRepository
    {
        Task<UserPreference?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken);
        Task<UserPreference> UpsertAsync(
            Guid userId,
            UserPreference preference,
            CancellationToken cancellationToken
        );
    }
}
