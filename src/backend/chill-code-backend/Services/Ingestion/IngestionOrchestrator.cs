using System.Linq;
using System.Text.Json;
using ChillCodeBackend.Data.Api;
using ChillCodeBackend.Models.Ingestion;
using ChillCodeBackend.Models.Unicam;
using Microsoft.Extensions.Logging;

namespace ChillCodeBackend.Services.Ingestion
{
    public class IngestionOrchestrator : IIngestionOrchestrator
    {
        private readonly IUnicamApiService _apiService;
        private readonly IIngestionPersistence _persistence;
        private readonly ILogger<IngestionOrchestrator> _logger;

        public IngestionOrchestrator(
            IUnicamApiService apiService,
            IIngestionPersistence persistence,
            ILogger<IngestionOrchestrator> logger
        )
        {
            _apiService = apiService;
            _persistence = persistence;
            _logger = logger;
        }

        public async Task<IngestionResult> IngestMunicipalityAsync(
            string municipality,
            string language = "it",
            CancellationToken cancellationToken = default
        )
        {
            if (string.IsNullOrWhiteSpace(municipality))
            {
                throw new ArgumentException("Municipality is required", nameof(municipality));
            }

            var result = new IngestionResult
            {
                Municipality = municipality.Trim(),
                Language = language,
            };

            try
            {
                _logger.LogInformation(
                    "Starting ingestion for municipality {Municipality}",
                    municipality
                );

                var municipalityVisit = await _apiService.GetMunicipalityVisitAsync(
                    municipality,
                    language,
                    cancellationToken
                );
                await _persistence.PersistMunicipalityVisitAsync(
                    municipality,
                    municipalityVisit,
                    cancellationToken
                );
                _logger.LogInformation(
                    "Fetched municipality profile for {Municipality}: {MunicipalityName}",
                    municipality,
                    municipalityVisit.Name ?? municipality
                );

                // Events (from municipality visit highlights)
                var eventDetails = await FetchDetailsFromHighlightsAsync(
                    municipalityVisit.Events,
                    id => _apiService.GetEventDetailAsync(id, language, cancellationToken),
                    cancellationToken
                );
                await _persistence.PersistEventDetailsAsync(eventDetails, cancellationToken);
                result.EventDetails.AddRange(eventDetails);
                result.EventCount = eventDetails.Count;
                _logger.LogInformation("Fetched {Count} event details", eventDetails.Count);

                // Routes (from municipality visit highlights)
                var routeHighlights = municipalityVisit
                    .ArticlesAndPaths.Where(a =>
                        string.Equals(a.Category, "Routes", StringComparison.OrdinalIgnoreCase)
                        && !string.IsNullOrWhiteSpace(a.EntityId)
                    )
                    .ToList();
                var routeDetails = await FetchDetailsFromHighlightsAsync(
                    routeHighlights,
                    id => _apiService.GetRouteDetailAsync(id, language, cancellationToken),
                    cancellationToken
                );
                await _persistence.PersistRouteDetailsAsync(routeDetails, cancellationToken);
                result.RouteDetails.AddRange(routeDetails);
                result.RouteCount = routeDetails.Count;
                _logger.LogInformation("Fetched {Count} route details", routeDetails.Count);

                // Articles (from municipality visit highlights)
                var articleHighlights = municipalityVisit
                    .ArticlesAndPaths.Where(a =>
                        string.Equals(a.Category, "Articles", StringComparison.OrdinalIgnoreCase)
                        && !string.IsNullOrWhiteSpace(a.EntityId)
                    )
                    .ToList();
                var articleDetails = await FetchDetailsFromHighlightsAsync(
                    articleHighlights,
                    id => _apiService.GetArticleDetailAsync(id, language, cancellationToken),
                    cancellationToken
                );
                await _persistence.PersistArticleDetailsAsync(articleDetails, cancellationToken);
                result.ArticleDetails.AddRange(articleDetails);
                result.ArticleCount = articleDetails.Count;
                _logger.LogInformation("Fetched {Count} article details", articleDetails.Count);

                result.CompletedAt = DateTimeOffset.UtcNow;
                _logger.LogInformation(
                    "Ingestion finished for municipality {Municipality}. Total POIs {Poi}, Events {Events}, Organizations {Orgs}, Routes {Routes}, Articles {Articles}",
                    municipality,
                    result.TotalPoiCount,
                    result.EventCount,
                    result.OrganizationCount,
                    result.RouteCount,
                    result.ArticleCount
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Ingestion failed for municipality {Municipality}",
                    municipality
                );
                result.Errors.Add(ex.Message);
            }

            return result;
        }

        private async Task<List<JsonElement>> FetchDetailsFromHighlightsAsync(
            IEnumerable<UnicamMunicipalityVisitHighlight> highlights,
            Func<string, Task<JsonElement>> detailFetcher,
            CancellationToken cancellationToken
        )
        {
            var details = new List<JsonElement>();
            var skipped = 0;
            foreach (var item in highlights)
            {
                if (string.IsNullOrWhiteSpace(item.EntityId))
                {
                    continue;
                }

                cancellationToken.ThrowIfCancellationRequested();
                try
                {
                    var detail = await detailFetcher(item.EntityId);
                    details.Add(detail);
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (UnicamApiException ex)
                {
                    skipped++;
                    _logger.LogWarning(
                        ex,
                        "Skipping detail for EntityId {EntityId} (Category {Category}) due to Unicam API error.",
                        item.EntityId,
                        item.Category ?? "unknown"
                    );
                }
                catch (Exception ex)
                {
                    skipped++;
                    _logger.LogWarning(
                        ex,
                        "Skipping detail for EntityId {EntityId} (Category {Category}) due to unexpected error.",
                        item.EntityId,
                        item.Category ?? "unknown"
                    );
                }
            }

            if (skipped > 0)
            {
                _logger.LogInformation(
                    "Skipped {Skipped} highlight detail requests due to errors.",
                    skipped
                );
            }

            return details;
        }
    }
}
