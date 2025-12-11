using System.Runtime.CompilerServices;
using System.Text.Json;
using ChillCodeBackend.Data.Repositories.Municipalities;
using ChillCodeBackend.Models.Unicam;

namespace ChillCodeBackend.Services.Ingestion
{
    public class IngestionPersistence : IIngestionPersistence
    {
        private readonly ILogger<IngestionPersistence> _logger;

        private readonly IMunicipalityRepository _municipalityRepository;

        private readonly IArticleRepository _articleStoreRepository;

        private readonly IEventRepository _eventRepository;

        private readonly IRouteRepository _routeRepository;

        public IngestionPersistence(ILogger<IngestionPersistence> logger, 
        IMunicipalityRepository municipalityRepository,
        IArticleRepository articleStoreRepository,
        IEventRepository eventRepository,
        IRouteRepository routeRepository)
        {
            _logger = logger;
            _municipalityRepository = municipalityRepository;
            _articleStoreRepository = articleStoreRepository;
            _eventRepository = eventRepository;
            _routeRepository = routeRepository;
        }

        public Task PersistMunicipalityVisitAsync(
            UnicamMunicipalityVisit municipalityVisit,
            CancellationToken cancellationToken
        )
        {
            _logger.LogInformation(
                "Skipping persistence for municipality visit data ({MunicipalityName}) (no-op).",
                municipalityVisit.Name ?? "unknown"
            );
            return Task.CompletedTask;
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
            _logger.LogInformation(
                "Skipping persistence for {Count} event detail records (no-op).",
                items.Count()
            );
            return Task.CompletedTask;
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
            _logger.LogInformation(
                "Skipping persistence for {Count} route detail records (no-op).",
                items.Count()
            );
            return Task.CompletedTask;
        }

        public Task PersistArticleDetailsAsync(
            IEnumerable<JsonElement> items,
            CancellationToken cancellationToken
        )
        {
            _logger.LogInformation(
                "Skipping persistence for {Count} article detail records (no-op).",
                items.Count()
            );
            // _articleStoreRepository.AddOrUpdateAsync();
            return Task.CompletedTask;
        }
    }
}
