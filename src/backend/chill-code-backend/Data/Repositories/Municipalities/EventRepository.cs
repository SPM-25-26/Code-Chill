using ChillCodeBackend.Models.Entities;
using ChillCodeBackend.Data.Db;
using Microsoft.EntityFrameworkCore;

namespace ChillCodeBackend.Data.Repositories.Municipalities
{

    public class EventRepository : IEventRepository
    {
        private readonly ChillCodeDbContext _dbContext;

        public EventRepository(ChillCodeDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Event?> GetByIdentifierAsync(string identifier, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Events.FirstOrDefaultAsync(
                e => e.Identifier == identifier,
                cancellationToken
            );
        }

        public async Task AddOrUpdateAsync(Event evt, CancellationToken cancellationToken = default)
        {
            var existingEvent = await _dbContext.Events.FirstOrDefaultAsync(
                e => e.Identifier == evt.Identifier,
                cancellationToken
            );

            if (existingEvent is null)
            {
                await _dbContext.Events.AddAsync(evt, cancellationToken);
            }
            else
            {
                _dbContext.Entry(existingEvent).CurrentValues.SetValues(evt);
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteByIdAsync(string identifier, CancellationToken cancellationToken = default)
        {
            var existingEvent = await _dbContext.Events.FirstOrDefaultAsync(
                e => e.Identifier == identifier,
                cancellationToken
            );

            if (existingEvent is not null)
            {
                _dbContext.Events.Remove(existingEvent);
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task<IReadOnlyCollection<Event>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _dbContext.Events.ToListAsync(cancellationToken);
        }
    }
}
