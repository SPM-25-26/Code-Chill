using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using ChillCodeBackend.Models.Unicam;
using Microsoft.Extensions.Logging;

namespace ChillCodeBackend.Data.Api
{
    public class UnicamApiService : IUnicamApiService
    {
        private const string DefaultLanguage = "it";

        private readonly IReadOnlyDictionary<UnicamPoiCategory, string> PoiDetailEndpoints =
            new Dictionary<UnicamPoiCategory, string>
            {
                [UnicamPoiCategory.ArtCulture] = "/api/art-culture/detail/",
                [UnicamPoiCategory.EatAndDrink] = "/api/eat-and-drink/detail/",
                [UnicamPoiCategory.EntertainmentLeisure] = "/api/entertainment-leisure/detail/",
                [UnicamPoiCategory.Nature] = "/api/nature/detail/",
                [UnicamPoiCategory.Services] = "/api/services/detail/",
                [UnicamPoiCategory.Shopping] = "/api/shopping/detail/",
                [UnicamPoiCategory.Sleep] = "/api/sleep/detail/",
                [UnicamPoiCategory.TypicalProducts] = "/api/typical-products/detail/",
            };

        private readonly IReadOnlyDictionary<UnicamPoiCategory, string> PoiCardEndpoints =
            new Dictionary<UnicamPoiCategory, string>
            {
                [UnicamPoiCategory.ArtCulture] = "/api/art-culture/card-list",
                [UnicamPoiCategory.EatAndDrink] = "/api/eat-and-drink/card-list",
                [UnicamPoiCategory.EntertainmentLeisure] = "/api/entertainment-leisure/card-list",
                [UnicamPoiCategory.Nature] = "/api/nature/card-list",
                [UnicamPoiCategory.Services] = "/api/services/card-list",
                [UnicamPoiCategory.Shopping] = "/api/shopping/card-list",
                [UnicamPoiCategory.Sleep] = "/api/sleep/card-list",
                [UnicamPoiCategory.TypicalProducts] = "/api/typical-products/card-list",
            };

        private readonly IUnicamApiProxy _proxy;
        private readonly ILogger<UnicamApiService> _logger;

        public UnicamApiService(IUnicamApiProxy proxy, ILogger<UnicamApiService> logger)
        {
            _proxy = proxy;
            _logger = logger;
        }

        public Task<JsonElement> GetPoiDetailAsync(
            UnicamPoiCategory category,
            string identifier,
            string language = DefaultLanguage,
            CancellationToken cancellationToken = default
        ) =>
            FetchDetailAsync(PoiDetailEndpoints[category], identifier, language, cancellationToken);

        public Task<JsonElement> GetEventDetailAsync(
            string identifier,
            string language = DefaultLanguage,
            CancellationToken cancellationToken = default
        ) => FetchDetailAsync("/api/events/detail/", identifier, language, cancellationToken);

        public Task<JsonElement> GetOrganizationDetailAsync(
            string taxCode,
            string language = DefaultLanguage,
            CancellationToken cancellationToken = default
        ) => FetchDetailAsync("/api/organizations/detail/", taxCode, language, cancellationToken);

        public Task<JsonElement> GetRouteDetailAsync(
            string identifier,
            string language = DefaultLanguage,
            CancellationToken cancellationToken = default
        ) => FetchDetailAsync("/api/routes/detail/", identifier, language, cancellationToken);

        public Task<JsonElement> GetArticleDetailAsync(
            string identifier,
            string language = DefaultLanguage,
            CancellationToken cancellationToken = default
        ) => FetchDetailAsync("/api/articles/detail/", identifier, language, cancellationToken);

        public Task<UnicamMunicipalityVisit> GetMunicipalityVisitAsync(
            string municipality,
            string language = DefaultLanguage,
            CancellationToken cancellationToken = default
        )
        {
            if (string.IsNullOrWhiteSpace(municipality))
            {
                throw new ArgumentException("Municipality is required", nameof(municipality));
            }

            municipality = municipality.Trim();
            language = NormalizeLanguage(language);

            // Municipality visit endpoint uses "city" parameter
            var relativePath = BuildQueryPath(
                "/api/organizations/municipalities/visit",
                new Dictionary<string, string?> { ["city"] = municipality, ["language"] = language }
            );

            return FetchAndHandleAsync<UnicamMunicipalityVisit>(relativePath, cancellationToken);
        }

        public Task<JsonElement> GetPoiCardsAsync(
            UnicamPoiCategory category,
            string municipality,
            string language = DefaultLanguage,
            CancellationToken cancellationToken = default
        )
        {
            if (string.IsNullOrWhiteSpace(municipality))
            {
                throw new ArgumentException("Municipality is required", nameof(municipality));
            }

            // Municipality name should already be normalized (without "Comune di") by the caller
            municipality = municipality.Trim();
            language = NormalizeLanguage(language);

            // POI card endpoints use "municipality" parameter
            var relativePath = BuildQueryPath(
                PoiCardEndpoints[category],
                new Dictionary<string, string?>
                {
                    ["municipality"] = municipality,
                    ["language"] = language,
                }
            );

            return FetchAndHandleAsync<JsonElement>(relativePath, cancellationToken);
        }

        private Task<JsonElement> FetchDetailAsync(
            string basePath,
            string identifier,
            string language,
            CancellationToken cancellationToken
        )
        {
            if (string.IsNullOrWhiteSpace(identifier))
            {
                throw new ArgumentException("Identifier is required", nameof(identifier));
            }

            language = NormalizeLanguage(language);
            var relativePath = BuildQueryPath(
                basePath + identifier.Trim(),
                new Dictionary<string, string?> { ["language"] = language }
            );

            return FetchAndHandleAsync<JsonElement>(relativePath, cancellationToken);
        }

        private async Task<T> FetchAndHandleAsync<T>(
            string relativePath,
            CancellationToken cancellationToken
        )
        {
            // Log the full path being called
            _logger.LogInformation("Calling Unicam API: {RelativePath}", relativePath);

            try
            {
                var result = await _proxy.FetchAsAsync<T>(relativePath, cancellationToken);
                if (result is null)
                {
                    throw new UnicamApiException(
                        "Received empty response from Unicam API.",
                        relativePath
                    );
                }

                _logger.LogInformation(
                    "Successfully fetched from Unicam API: {RelativePath}",
                    relativePath
                );
                return result;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Unicam API request failed for {RelativePath}", relativePath);
                throw new UnicamApiException(
                    "Failed to fetch data from Unicam API.",
                    relativePath,
                    ex
                );
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(
                    ex,
                    "Unicam API request timed out for {RelativePath}",
                    relativePath
                );
                throw new UnicamApiException("Unicam API request timed out.", relativePath, ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Unexpected error when calling Unicam API for {RelativePath}",
                    relativePath
                );
                throw new UnicamApiException(
                    "Unexpected error when calling Unicam API.",
                    relativePath,
                    ex
                );
            }
        }

        private string BuildQueryPath(string route, IDictionary<string, string?> queryParams)
        {
            var usable = queryParams
                .Where(kv => !string.IsNullOrWhiteSpace(kv.Value))
                .Select(kv =>
                    $"{WebUtility.UrlEncode(kv.Key)}={WebUtility.UrlEncode(kv.Value!.Trim())}"
                )
                .ToArray();

            if (usable.Length == 0)
            {
                return route;
            }

            var separator = route.Contains('?') ? "&" : "?";
            return route + separator + string.Join("&", usable);
        }

        private string NormalizeLanguage(string language)
        {
            if (string.IsNullOrWhiteSpace(language))
            {
                return DefaultLanguage;
            }

            return language.Trim();
        }
    }
}
