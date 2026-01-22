using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
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

            // Normalize municipality name by removing "Comune di" prefix at the start
            // This ensures the API returns the correct response with both names
            var normalizedMunicipality = NormalizeMunicipalityName(municipality);

            var result = new IngestionResult
            {
                Municipality = normalizedMunicipality,
                Language = language,
            };

            try
            {
                _logger.LogInformation(
                    "Starting ingestion for municipality {Municipality}",
                    normalizedMunicipality
                );

                // Municipality Visit API needs the short name (without "Comune di")
                // API then returns: name = "Castignano", legalName = "Comune di Castignano"
                var municipalityVisit = await _apiService.GetMunicipalityVisitAsync(
                    normalizedMunicipality,
                    language,
                    cancellationToken
                );

                // Use the short name from the API response for POI card endpoints
                var municipalityShortName = municipalityVisit.Name ?? normalizedMunicipality;

                // Persisted entities use the municipality "display name" (from the API payload),
                // so we align the result to that as well (useful for verification/logging).
                if (!string.IsNullOrWhiteSpace(municipalityVisit.Name))
                {
                    result.Municipality = municipalityVisit.Name!;
                }
                await _persistence.PersistMunicipalityVisitAsync(
                    normalizedMunicipality,
                    municipalityVisit,
                    cancellationToken
                );
                _logger.LogInformation(
                    "Fetched municipality profile for {Municipality}: {MunicipalityName}",
                    normalizedMunicipality,
                    municipalityVisit.Name ?? normalizedMunicipality
                );

                var municipalityInfo = new Models.Entities.MunicipalityInfo
                {
                    Name = municipalityVisit.Name ?? normalizedMunicipality,
                    LogoPath = municipalityVisit.LogoPath ?? string.Empty,
                };

                // Events (from municipality visit highlights)
                var eventDetails = await FetchDetailsFromHighlightsAsync(
                    municipalityVisit.Events,
                    id => _apiService.GetEventDetailAsync(id, language, cancellationToken),
                    cancellationToken
                );
                await _persistence.PersistEventDetailsAsync(
                    eventDetails,
                    municipalityInfo,
                    language,
                    cancellationToken
                );
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
                var routeDetailsRaw = await FetchDetailsFromHighlightsAsync(
                    routeHighlights,
                    id => _apiService.GetRouteDetailAsync(id, language, cancellationToken),
                    cancellationToken
                );
                var routeDetails = MergeRouteIds(routeHighlights, routeDetailsRaw);
                await _persistence.PersistRouteDetailsAsync(
                    routeDetails,
                    municipalityInfo,
                    language,
                    cancellationToken
                );
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
                await _persistence.PersistArticleDetailsAsync(
                    articleDetails,
                    municipalityInfo,
                    language,
                    cancellationToken
                );
                result.ArticleDetails.AddRange(articleDetails);
                result.ArticleCount = articleDetails.Count;
                _logger.LogInformation("Fetched {Count} article details", articleDetails.Count);

                // Organizations
                _logger.LogInformation(
                    "Fetching organization cards for {Municipality}",
                    municipalityShortName
                );
                var organizationCardsResponse = await _apiService.GetOrganizationCardsAsync(
                    municipalityShortName,
                    language,
                    cancellationToken
                );
                var organizationCards = new List<UnicamOrganizationCard>();
                if (organizationCardsResponse.ValueKind == JsonValueKind.Array)
                {
                    foreach (var card in organizationCardsResponse.EnumerateArray())
                    {
                        var orgCard =
                            JsonSerializer.Deserialize<UnicamOrganizationCard>(card.GetRawText());
                        if (orgCard != null && !string.IsNullOrWhiteSpace(orgCard.EntityId))
                        {
                            organizationCards.Add(orgCard);
                        }
                    }
                }

                _logger.LogInformation(
                    "Found {Count} organization cards for {Municipality}",
                    organizationCards.Count,
                    municipalityShortName
                );

                var organizationDetails = await FetchDetailsFromCardIdsAsync(
                    organizationCards.Select(c => c.EntityId ?? string.Empty),
                    id => _apiService.GetOrganizationDetailAsync(id, language, cancellationToken),
                    "organization",
                    cancellationToken
                );
                if (organizationDetails.Count > 0)
                {
                    await _persistence.PersistOrganizationDetailsAsync(
                        organizationDetails,
                        municipalityInfo,
                        language,
                        cancellationToken
                    );
                }

                result.OrganizationDetails.AddRange(organizationDetails);
                result.OrganizationCount = organizationDetails.Count;
                _logger.LogInformation(
                    "Fetched {Count} organization details for {Municipality}",
                    organizationDetails.Count,
                    municipalityShortName
                );

                // Services (non-POI services endpoint)
                _logger.LogInformation(
                    "Fetching service cards for {Municipality}",
                    municipalityShortName
                );
                var serviceCardsResponse = await _apiService.GetServiceCardsAsync(
                    municipalityShortName,
                    language,
                    cancellationToken
                );
                var serviceCards = new List<UnicamPoiCard>();
                if (serviceCardsResponse.ValueKind == JsonValueKind.Array)
                {
                    foreach (var card in serviceCardsResponse.EnumerateArray())
                    {
                        var serviceCard =
                            JsonSerializer.Deserialize<UnicamPoiCard>(card.GetRawText());
                        if (serviceCard != null && !string.IsNullOrWhiteSpace(serviceCard.EntityId))
                        {
                            serviceCards.Add(serviceCard);
                        }
                    }
                }

                _logger.LogInformation(
                    "Found {Count} service cards for {Municipality}",
                    serviceCards.Count,
                    municipalityShortName
                );

                var serviceDetails = await FetchDetailsFromCardIdsAsync(
                    serviceCards.Select(c => c.EntityId ?? string.Empty),
                    id => _apiService.GetServiceDetailAsync(id, language, cancellationToken),
                    "service",
                    cancellationToken
                );
                if (serviceDetails.Count > 0)
                {
                    await _persistence.PersistServiceDetailsAsync(
                        serviceDetails,
                        municipalityInfo,
                        language,
                        cancellationToken
                    );
                }
                result.ServiceCount = serviceDetails.Count;
                _logger.LogInformation(
                    "Fetched {Count} service details for {Municipality}",
                    serviceDetails.Count,
                    municipalityShortName
                );

                // POIs - Load cards for each category and then fetch details
                // Use short name (without "Comune di") for POI card endpoints
                await IngestPoiCategoryAsync(
                    UnicamPoiCategory.ArtCulture,
                    municipalityShortName,
                    language,
                    municipalityInfo,
                    result,
                    cancellationToken
                );
                await IngestPoiCategoryAsync(
                    UnicamPoiCategory.EatAndDrink,
                    municipalityShortName,
                    language,
                    municipalityInfo,
                    result,
                    cancellationToken
                );
                await IngestPoiCategoryAsync(
                    UnicamPoiCategory.EntertainmentLeisure,
                    municipalityShortName,
                    language,
                    municipalityInfo,
                    result,
                    cancellationToken
                );
                await IngestPoiCategoryAsync(
                    UnicamPoiCategory.Nature,
                    municipalityShortName,
                    language,
                    municipalityInfo,
                    result,
                    cancellationToken
                );
                await IngestPoiCategoryAsync(
                    UnicamPoiCategory.Services,
                    municipalityShortName,
                    language,
                    municipalityInfo,
                    result,
                    cancellationToken
                );
                await IngestPoiCategoryAsync(
                    UnicamPoiCategory.Shopping,
                    municipalityShortName,
                    language,
                    municipalityInfo,
                    result,
                    cancellationToken
                );
                await IngestPoiCategoryAsync(
                    UnicamPoiCategory.Sleep,
                    municipalityShortName,
                    language,
                    municipalityInfo,
                    result,
                    cancellationToken
                );
                await IngestPoiCategoryAsync(
                    UnicamPoiCategory.TypicalProducts,
                    municipalityShortName,
                    language,
                    municipalityInfo,
                    result,
                    cancellationToken
                );

                result.CompletedAt = DateTimeOffset.UtcNow;
                _logger.LogInformation(
                    "Ingestion finished for municipality {Municipality}. Total POIs {Poi}, Events {Events}, Organizations {Orgs}, Routes {Routes}, Articles {Articles}",
                    normalizedMunicipality,
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
                    normalizedMunicipality
                );
                result.Errors.Add(ex.Message);
            }

                return result;
        }

        private async Task<List<JsonElement>> FetchDetailsFromCardIdsAsync(
            IEnumerable<string> entityIds,
            Func<string, Task<JsonElement>> detailFetcher,
            string itemLabel,
            CancellationToken cancellationToken
        )
        {
            var details = new List<JsonElement>();
            var skipped = 0;

            foreach (var entityId in entityIds)
            {
                if (string.IsNullOrWhiteSpace(entityId))
                {
                    continue;
                }

                cancellationToken.ThrowIfCancellationRequested();
                try
                {
                    var detail = await detailFetcher(entityId);
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
                        "Skipping detail for {ItemLabel} {EntityId} due to Unicam API error.",
                        itemLabel,
                        entityId
                    );
                }
                catch (Exception ex)
                {
                    skipped++;
                    _logger.LogWarning(
                        ex,
                        "Skipping detail for {ItemLabel} {EntityId} due to unexpected error.",
                        itemLabel,
                        entityId
                    );
                }
            }

            if (skipped > 0)
            {
                _logger.LogInformation(
                    "Skipped {Skipped} {ItemLabel} detail requests due to errors.",
                    skipped,
                    itemLabel
                );
            }

            return details;
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

        private static List<JsonElement> MergeRouteIds(
            List<UnicamMunicipalityVisitHighlight> highlights,
            List<JsonElement> details
        )
        {
            if (highlights.Count == 0 || details.Count == 0)
            {
                return details;
            }

            var mapped = new List<JsonElement>(details.Count);

            foreach (var (detail, index) in details.Select((d, i) => (d, i)))
            {
                var highlightId = index < highlights.Count ? highlights[index].EntityId : null;
                if (highlightId is null)
                {
                    mapped.Add(detail);
                    continue;
                }

                if (
                    detail.TryGetProperty("id", out _)
                    || detail.TryGetProperty("Id", out _)
                    || detail.TryGetProperty("identifier", out _)
                    || detail.TryGetProperty("Identifier", out _)
                    || detail.TryGetProperty("entityId", out _)
                    || detail.TryGetProperty("EntityId", out _)
                )
                {
                    mapped.Add(detail);
                    continue;
                }

                try
                {
                    var node =
                        JsonNode.Parse(detail.GetRawText()) as JsonObject ?? new JsonObject();
                    node["entityId"] = highlightId;
                    var merged = JsonDocument.Parse(node.ToJsonString()).RootElement;
                    mapped.Add(merged);
                }
                catch
                {
                    mapped.Add(detail);
                }
            }

            return mapped;
        }

        private async Task IngestPoiCategoryAsync(
            UnicamPoiCategory category,
            string municipality,
            string language,
            Models.Entities.MunicipalityInfo municipalityInfo,
            IngestionResult result,
            CancellationToken cancellationToken
        )
        {
            try
            {
                _logger.LogInformation(
                    "Fetching {Category} POI cards for {Municipality}",
                    category,
                    municipality
                );

                // Get list of POI cards
                var cardsResponse = await _apiService.GetPoiCardsAsync(
                    category,
                    municipality,
                    language,
                    cancellationToken
                );

                // Parse cards to extract entity IDs
                var cards = new List<UnicamPoiCard>();
                if (cardsResponse.ValueKind == JsonValueKind.Array)
                {
                    foreach (var card in cardsResponse.EnumerateArray())
                    {
                        var poiCard = JsonSerializer.Deserialize<UnicamPoiCard>(card.GetRawText());
                        if (poiCard != null && !string.IsNullOrWhiteSpace(poiCard.EntityId))
                        {
                            cards.Add(poiCard);
                        }
                    }
                }

                _logger.LogInformation(
                    "Found {Count} {Category} POI cards for {Municipality}",
                    cards.Count,
                    category,
                    municipality
                );

                // Fetch details for each POI
                var details = new List<JsonElement>();
                var skipped = 0;
                foreach (var card in cards)
                {
                    if (string.IsNullOrWhiteSpace(card.EntityId))
                    {
                        continue;
                    }

                    cancellationToken.ThrowIfCancellationRequested();
                    try
                    {
                        var detail = await _apiService.GetPoiDetailAsync(
                            category,
                            card.EntityId,
                            language,
                            cancellationToken
                        );
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
                            "Skipping detail for {Category} POI {EntityId} due to Unicam API error.",
                            category,
                            card.EntityId
                        );
                    }
                    catch (Exception ex)
                    {
                        skipped++;
                        _logger.LogWarning(
                            ex,
                            "Skipping detail for {Category} POI {EntityId} due to unexpected error.",
                            category,
                            card.EntityId
                        );
                    }
                }

                if (skipped > 0)
                {
                    _logger.LogInformation(
                        "Skipped {Skipped} {Category} POI detail requests due to errors.",
                        skipped,
                        category
                    );
                }

                // Persist POI details
                if (details.Count > 0)
                {
                    await _persistence.PersistPoiDetailsAsync(
                        details,
                        category,
                        municipalityInfo,
                        language,
                        cancellationToken
                    );
                    result.TotalPoiCount += details.Count;
                }

                _logger.LogInformation(
                    "Fetched {Count} {Category} POI details for {Municipality}",
                    details.Count,
                    category,
                    municipality
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to ingest {Category} POIs for municipality {Municipality}",
                    category,
                    municipality
                );
                // Add error to result but don't throw - continue with other categories
                result.Errors.Add($"Failed to ingest {category} POIs: {ex.Message}");
            }
        }

        /// <summary>
        /// Removes the "Comune di" prefix from municipality names at the start of ingestion.
        /// The API expects the short name (e.g., "Castignano") and then returns both:
        /// - name: "Castignano" (short name for POI endpoints)
        /// - legalName: "Comune di Castignano" (official full name)
        /// </summary>
        private static string NormalizeMunicipalityName(string municipality)
        {
            if (string.IsNullOrWhiteSpace(municipality))
            {
                return string.Empty;
            }

            municipality = municipality.Trim();

            // Remove "Comune di " prefix (case-insensitive)
            const string prefix = "Comune di ";
            if (municipality.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                return municipality.Substring(prefix.Length).Trim();
            }

            return municipality;
        }
    }
}
