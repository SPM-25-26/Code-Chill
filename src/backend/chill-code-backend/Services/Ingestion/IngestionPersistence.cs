using System.Linq;
using System.Text.Json;
using ChillCodeBackend.Data.Repositories.Municipalities;
using ChillCodeBackend.Models.Entities;
using ChillCodeBackend.Models.Unicam;
using ChillCodeBackend.Services.Ingestion.Mappers;

namespace ChillCodeBackend.Services.Ingestion
{
    public class IngestionPersistence : IIngestionPersistence
    {
        private readonly ILogger<IngestionPersistence> _logger;
        private readonly IMunicipalityRepository _municipalityRepository;
        private readonly IArticleRepository _articleStoreRepository;
        private readonly IEventRepository _eventRepository;
        private readonly IRouteRepository _routeRepository;
        private readonly ArticleMapper _articleMapper;
        private readonly MunicipalityMapper _municipalityMapper;
        private readonly EventMapper _eventMapper;
        private readonly RouteMapper _routeMapper;

        public IngestionPersistence(
            ILogger<IngestionPersistence> logger,
            IMunicipalityRepository municipalityRepository,
            IArticleRepository articleStoreRepository,
            IEventRepository eventRepository,
            IRouteRepository routeRepository
        )
        {
            _logger = logger;
            _municipalityRepository = municipalityRepository;
            _articleStoreRepository = articleStoreRepository;
            _eventRepository = eventRepository;
            _routeRepository = routeRepository;
            _articleMapper = new ArticleMapper();
            _municipalityMapper = new MunicipalityMapper();
            _eventMapper = new EventMapper();
            _routeMapper = new RouteMapper();
        }

        public Task PersistMunicipalityVisitAsync(
            UnicamMunicipalityVisit municipalityVisit,
            CancellationToken cancellationToken
        )
        {
            var municipality = _municipalityMapper.Map(municipalityVisit);
            return PersistMunicipalityAsync(municipality, cancellationToken);
        }

        public Task PersistPoiDetailsAsync(
            IEnumerable<JsonElement> items,
            CancellationToken cancellationToken
        )
        {
            _logger.LogInformation(
                "Skipping persistence for {Count} POI detail records (no-op).",
                items.Count()
            );
            return Task.CompletedTask;
        }

        public Task PersistEventDetailsAsync(
            IEnumerable<JsonElement> items,
            CancellationToken cancellationToken
        )
        {
            var events = _eventMapper.MapEvents(items);
            return PersistEventsAsync(events, cancellationToken);
        }

        public Task PersistOrganizationDetailsAsync(
            IEnumerable<JsonElement> items,
            CancellationToken cancellationToken
        )
        {
            _logger.LogInformation(
                "Skipping persistence for {Count} organization detail records (no-op).",
                items.Count()
            );
            return Task.CompletedTask;
        }

        public Task PersistRouteDetailsAsync(
            IEnumerable<JsonElement> items,
            CancellationToken cancellationToken
        )
        {
            var routes = _routeMapper.MapRoutes(items);
            return PersistRoutesAsync(routes, cancellationToken);
        }

        public Task PersistArticleDetailsAsync(
            IEnumerable<JsonElement> items,
            CancellationToken cancellationToken
        )
        {
            var articles = _articleMapper.MapArticles(items);
            return PersistArticlesAsync(articles, cancellationToken);
        }

        private async Task PersistArticlesAsync(
            IEnumerable<Article> articles,
            CancellationToken cancellationToken
        )
        {
            var persisted = 0;
            foreach (var article in articles)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await _articleStoreRepository.AddOrUpdateAsync(article, cancellationToken);
                persisted++;
            }

            _logger.LogInformation(
                "Persisted {Count} article detail records to the database.",
                persisted
            );
        }

        private async Task PersistMunicipalityAsync(
            Municipality municipality,
            CancellationToken cancellationToken
        )
        {
            await _municipalityRepository.AddOrUpdateAsync(municipality, cancellationToken);
            _logger.LogInformation(
                "Persisted municipality visit data for {MunicipalityName}.",
                municipality.Name
            );
        }

        private async Task PersistEventsAsync(
            IEnumerable<Event> events,
            CancellationToken cancellationToken
        )
        {
            var persisted = 0;
            foreach (var evt in events)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await _eventRepository.AddOrUpdateAsync(evt, cancellationToken);
                persisted++;
            }

            _logger.LogInformation(
                "Persisted {Count} event detail records to the database.",
                persisted
            );
        }

        private async Task PersistRoutesAsync(
            IEnumerable<Route> routes,
            CancellationToken cancellationToken
        )
        {
            var persisted = 0;
            foreach (var route in routes)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await _routeRepository.AddOrUpdateAsync(route, cancellationToken);
                persisted++;
            }

            _logger.LogInformation(
                "Persisted {Count} route detail records to the database.",
                persisted
            );
        }
    }
}
