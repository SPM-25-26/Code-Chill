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
        private readonly IPointOfInterestRepository _poiRepository;
        private readonly IOrganizationRepository _organizationRepository;
        private readonly IServiceRepository _serviceRepository;
        private readonly ArticleMapper _articleMapper;
        private readonly MunicipalityMapper _municipalityMapper;
        private readonly EventMapper _eventMapper;
        private readonly RouteMapper _routeMapper;
        private readonly PointOfInterestMapper _poiMapper;
        private readonly OrganizationMapper _organizationMapper;
        private readonly ServiceMapper _serviceMapper;

        public IngestionPersistence(
            ILogger<IngestionPersistence> logger,
            IMunicipalityRepository municipalityRepository,
            IArticleRepository articleStoreRepository,
            IEventRepository eventRepository,
            IRouteRepository routeRepository,
            IPointOfInterestRepository poiRepository,
            IOrganizationRepository organizationRepository,
            IServiceRepository serviceRepository
        )
        {
            _logger = logger;
            _municipalityRepository = municipalityRepository;
            _articleStoreRepository = articleStoreRepository;
            _eventRepository = eventRepository;
            _routeRepository = routeRepository;
            _poiRepository = poiRepository;
            _organizationRepository = organizationRepository;
            _serviceRepository = serviceRepository;
            _articleMapper = new ArticleMapper();
            _municipalityMapper = new MunicipalityMapper();
            _eventMapper = new EventMapper();
            _routeMapper = new RouteMapper();
            _poiMapper = new PointOfInterestMapper();
            _organizationMapper = new OrganizationMapper();
            _serviceMapper = new ServiceMapper();
        }

        public Task PersistMunicipalityVisitAsync(
            string municipalityExternalId,
            UnicamMunicipalityVisit municipalityVisit,
            string language,
            CancellationToken cancellationToken
        )
        {
            var municipality = _municipalityMapper.Map(
                municipalityExternalId,
                municipalityVisit,
                language
            );
            return PersistMunicipalityAsync(municipality, cancellationToken);
        }

        public Task PersistPoiDetailsAsync(
            IEnumerable<JsonElement> items,
            UnicamPoiCategory category,
            MunicipalityInfo municipalityInfo,
            string language,
            CancellationToken cancellationToken
        )
        {
            // Convert UnicamPoiCategory to MobileCategory
            var mobileCategory = category switch
            {
                UnicamPoiCategory.ArtCulture => MobileCategory.ArtCulture,
                UnicamPoiCategory.EatAndDrink => MobileCategory.EatAndDrink,
                UnicamPoiCategory.EntertainmentLeisure => MobileCategory.EntertainmentLeisure,
                UnicamPoiCategory.Nature => MobileCategory.Nature,
                UnicamPoiCategory.Services => MobileCategory.Services,
                UnicamPoiCategory.Shopping => MobileCategory.Shopping,
                UnicamPoiCategory.Sleep => MobileCategory.Sleep,
                UnicamPoiCategory.TypicalProducts => MobileCategory.TypicalProducts,
                _ => throw new ArgumentOutOfRangeException(nameof(category), category, null),
            };

            var pois = _poiMapper.MapPointsOfInterest(items, mobileCategory).ToList();
            ApplyMunicipalityFallback(pois, municipalityInfo);
            ApplyLanguage(pois, language);
            return PersistPointsOfInterestAsync(pois, cancellationToken);
        }

        public Task PersistEventDetailsAsync(
            IEnumerable<JsonElement> items,
            MunicipalityInfo municipalityInfo,
            string language,
            CancellationToken cancellationToken
        )
        {
            var events = _eventMapper.MapEvents(items).ToList();
            ApplyMunicipalityFallback(events, municipalityInfo);
            ApplyLanguage(events, language);
            return PersistEventsAsync(events, cancellationToken);
        }

        public Task PersistOrganizationDetailsAsync(
            IEnumerable<JsonElement> items,
            MunicipalityInfo municipalityInfo,
            string language,
            CancellationToken cancellationToken
        )
        {
            var organizations = _organizationMapper.MapOrganizations(items).ToList();
            ApplyMunicipalityFallback(organizations, municipalityInfo);
            ApplyLanguage(organizations, language);
            return PersistOrganizationsAsync(organizations, cancellationToken);
        }

        public Task PersistServiceDetailsAsync(
            IEnumerable<JsonElement> items,
            MunicipalityInfo municipalityInfo,
            string language,
            CancellationToken cancellationToken
        )
        {
            var services = _serviceMapper.MapServices(items).ToList();
            ApplyMunicipalityFallback(services, municipalityInfo);
            ApplyLanguage(services, language);
            return PersistServicesAsync(services, cancellationToken);
        }

        public Task PersistRouteDetailsAsync(
            IEnumerable<JsonElement> items,
            MunicipalityInfo municipalityInfo,
            string language,
            CancellationToken cancellationToken
        )
        {
            var routes = _routeMapper.MapRoutes(items).ToList();
            ApplyMunicipalityFallback(routes, municipalityInfo);
            ApplyLanguage(routes, language);
            return PersistRoutesAsync(routes, cancellationToken);
        }

        public Task PersistArticleDetailsAsync(
            IEnumerable<JsonElement> items,
            MunicipalityInfo municipalityInfo,
            string language,
            CancellationToken cancellationToken
        )
        {
            var articles = _articleMapper.MapArticles(items).ToList();
            ApplyMunicipalityFallback(articles, municipalityInfo);
            ApplyLanguage(articles, language);
            return PersistArticlesAsync(articles, cancellationToken);
        }

        private static void ApplyLanguage(IEnumerable<Article> items, string language)
        {
            foreach (var item in items)
            {
                item.Language = language;
            }
        }

        private static void ApplyLanguage(IEnumerable<Event> items, string language)
        {
            foreach (var item in items)
            {
                item.Language = language;
            }
        }

        private static void ApplyLanguage(IEnumerable<Models.Entities.Route> items, string language)
        {
            foreach (var item in items)
            {
                item.Language = language;
            }
        }

        private static void ApplyLanguage(IEnumerable<PointOfInterest> items, string language)
        {
            foreach (var item in items)
            {
                item.Language = language;
            }
        }

        private static void ApplyLanguage(IEnumerable<Organization> items, string language)
        {
            foreach (var item in items)
            {
                item.Language = language;
            }
        }

        private static void ApplyLanguage(IEnumerable<Service> items, string language)
        {
            foreach (var item in items)
            {
                item.Language = language;
            }
        }

        private static void ApplyMunicipalityFallback(
            IEnumerable<Article> items,
            MunicipalityInfo fallback
        )
        {
            foreach (var item in items)
            {
                item.MunicipalityData = MergeMunicipalityInfo(item.MunicipalityData, fallback);
            }
        }

        private static void ApplyMunicipalityFallback(
            IEnumerable<Event> items,
            MunicipalityInfo fallback
        )
        {
            foreach (var item in items)
            {
                item.MunicipalityData = MergeMunicipalityInfo(item.MunicipalityData, fallback);
            }
        }

        private static void ApplyMunicipalityFallback(
            IEnumerable<Models.Entities.Route> items,
            MunicipalityInfo fallback
        )
        {
            foreach (var item in items)
            {
                item.MunicipalityData = MergeMunicipalityInfo(item.MunicipalityData, fallback);
            }
        }

        private static void ApplyMunicipalityFallback(
            IEnumerable<PointOfInterest> items,
            MunicipalityInfo fallback
        )
        {
            foreach (var item in items)
            {
                item.MunicipalityData = MergeMunicipalityInfo(item.MunicipalityData, fallback);
            }
        }

        private static void ApplyMunicipalityFallback(
            IEnumerable<Organization> items,
            MunicipalityInfo fallback
        )
        {
            foreach (var item in items)
            {
                item.MunicipalityData = MergeMunicipalityInfo(item.MunicipalityData, fallback);
            }
        }

        private static void ApplyMunicipalityFallback(
            IEnumerable<Service> items,
            MunicipalityInfo fallback
        )
        {
            foreach (var item in items)
            {
                item.MunicipalityData = MergeMunicipalityInfo(item.MunicipalityData, fallback);
            }
        }

        private static MunicipalityInfo MergeMunicipalityInfo(
            MunicipalityInfo existing,
            MunicipalityInfo fallback
        )
        {
            var name = string.IsNullOrWhiteSpace(existing.Name) ? fallback.Name : existing.Name;
            var logoPath = string.IsNullOrWhiteSpace(existing.LogoPath)
                ? fallback.LogoPath
                : existing.LogoPath;

            return new MunicipalityInfo { Name = name, LogoPath = logoPath };
        }

        private async Task PersistArticlesAsync(
            IEnumerable<Article> articles,
            CancellationToken cancellationToken
        )
        {
            var result = await _articleStoreRepository.UpsertManyAsync(articles, cancellationToken);
            _logger.LogInformation(
                "Persisted article detail records to the database. Added {Added}, Updated {Updated}.",
                result.Inserted,
                result.Updated
            );
        }

        private async Task PersistMunicipalityAsync(
            Municipality municipality,
            CancellationToken cancellationToken
        )
        {
            var inserted = await _municipalityRepository.UpsertAsync(
                municipality,
                cancellationToken
            );
            _logger.LogInformation(
                "Persisted municipality visit data for {MunicipalityName}. {Operation}.",
                municipality.Name,
                inserted ? "Added" : "Updated"
            );
        }

        private async Task PersistEventsAsync(
            IEnumerable<Event> events,
            CancellationToken cancellationToken
        )
        {
            var result = await _eventRepository.UpsertManyAsync(events, cancellationToken);
            _logger.LogInformation(
                "Persisted event detail records to the database. Added {Added}, Updated {Updated}.",
                result.Inserted,
                result.Updated
            );
        }

        private async Task PersistRoutesAsync(
            IEnumerable<Models.Entities.Route> routes,
            CancellationToken cancellationToken
        )
        {
            var result = await _routeRepository.UpsertManyAsync(routes, cancellationToken);
            _logger.LogInformation(
                "Persisted route detail records to the database. Added {Added}, Updated {Updated}.",
                result.Inserted,
                result.Updated
            );
        }

        private async Task PersistPointsOfInterestAsync(
            IEnumerable<PointOfInterest> pois,
            CancellationToken cancellationToken
        )
        {
            var result = await _poiRepository.UpsertManyAsync(pois, cancellationToken);
            _logger.LogInformation(
                "Persisted POI detail records to the database. Added {Added}, Updated {Updated}.",
                result.Inserted,
                result.Updated
            );
        }

        private async Task PersistOrganizationsAsync(
            IEnumerable<Organization> organizations,
            CancellationToken cancellationToken
        )
        {
            var result = await _organizationRepository.UpsertManyAsync(
                organizations,
                cancellationToken
            );
            _logger.LogInformation(
                "Persisted organization detail records to the database. Added {Added}, Updated {Updated}.",
                result.Inserted,
                result.Updated
            );
        }

        private async Task PersistServicesAsync(
            IEnumerable<Service> services,
            CancellationToken cancellationToken
        )
        {
            var result = await _serviceRepository.UpsertManyAsync(services, cancellationToken);
            _logger.LogInformation(
                "Persisted service detail records to the database. Added {Added}, Updated {Updated}.",
                result.Inserted,
                result.Updated
            );
        }
    }
}
