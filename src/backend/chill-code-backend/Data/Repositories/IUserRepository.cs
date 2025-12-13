using ChillCodeBackend.Models.Entities;

namespace ChillCodeBackend.Data.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetByUserNameAsync(
            string userName,
            CancellationToken cancellationToken = default
        );
        Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task AddOrUpdateAsync(User user, CancellationToken cancellationToken = default);
        Task DeleteAsync(User user, CancellationToken cancellationToken = default);
        Task<IReadOnlyCollection<User>> GetAllAsync(CancellationToken cancellationToken = default);
    }
}
