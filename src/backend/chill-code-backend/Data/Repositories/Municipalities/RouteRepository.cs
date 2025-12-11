using ChillCodeBackend.Data.Db;
using ChillCodeBackend.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChillCodeBackend.Data.Repositories.Municipalities
{

    public class RouteRepository : IRouteStore
    {
        private readonly ChillCodeDbContext _dbContext;

        public RouteRepository(ChillCodeDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Models.Entities.Route?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Routes.FirstOrDefaultAsync(
                r => r.Name == name,
                cancellationToken
            );
        }

        public async Task AddOrUpdateAsync(Models.Entities.Route route, CancellationToken cancellationToken = default)
        {
            var existingRoute = await _dbContext.Routes.FirstOrDefaultAsync(
                r => r.Name == route.Name,
                cancellationToken
            );

            if (existingRoute is null)
            {
                await _dbContext.Routes.AddAsync(route, cancellationToken);
            }
            else
            {
                _dbContext.Entry(existingRoute).CurrentValues.SetValues(route);
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteByNameAsync(string name, CancellationToken ct = default)
        {
            var existingRoute = await _dbContext.Routes.FirstOrDefaultAsync(
                r => r.Name == name,
                ct
            );

            if (existingRoute is not null)
            {
                _dbContext.Routes.Remove(existingRoute);
                await _dbContext.SaveChangesAsync(ct);
            }
        }

        public async Task<IReadOnlyCollection<Models.Entities.Route>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _dbContext.Routes.ToListAsync(cancellationToken);
        }

    }
}
