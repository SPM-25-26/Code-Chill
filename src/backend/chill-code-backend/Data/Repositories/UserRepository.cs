using ChillCodeBackend.Data.Db;
using ChillCodeBackend.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChillCodeBackend.Data.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ChillCodeDbContext _dbContext;

        public UserRepository(ChillCodeDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<User?> GetByUserNameAsync(
            string userName,
            CancellationToken cancellationToken = default
        )
        {
            return await _dbContext.Users.FirstOrDefaultAsync(
                u => u.UserName == userName,
                cancellationToken
            );
        }

        public async Task<User?> GetByEmailAsync(
            string email,
            CancellationToken cancellationToken = default
        )
        {
            return await _dbContext.Users.FirstOrDefaultAsync(
                u => u.Email == email,
                cancellationToken
            );
        }

        public async Task AddOrUpdateAsync(User user, CancellationToken cancellationToken = default)
        {
            var existingUser = await _dbContext.Users.FirstOrDefaultAsync(
                u => u.Id == user.Id || u.Email == user.Email,
                cancellationToken
            );

            if (existingUser is null)
            {
                user.Id = user.Id == Guid.Empty ? Guid.NewGuid() : user.Id;
                var now = DateTimeOffset.UtcNow;
                user.CreatedAt = user.CreatedAt == default ? now : user.CreatedAt;
                user.ChangedAt = now;
                await _dbContext.Users.AddAsync(user, cancellationToken);
            }
            else
            {
                existingUser.UserName = user.UserName;
                existingUser.Email = user.Email;
                existingUser.PasswordHash = user.PasswordHash;
                existingUser.EmailVerified = user.EmailVerified;
                existingUser.RefreshToken = user.RefreshToken;
                existingUser.RefreshTokenExpiresAt = user.RefreshTokenExpiresAt;
                existingUser.ChangedAt = DateTimeOffset.UtcNow;
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task<IReadOnlyCollection<User>> GetAllAsync(
            CancellationToken cancellationToken = default
        )
        {
            var users = await _dbContext.Users.AsNoTracking().ToListAsync(cancellationToken);
            return users;
        }
    }
}
