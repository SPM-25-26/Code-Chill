using System.Globalization;
using System.Net;
using System.Net.Http;
using ChillCodeBackend.Models.Unicam;
using Microsoft.Extensions.Logging;

namespace ChillCodeBackend.Data.Api
{
    public interface IUnicamApiService
    {
        Task<IReadOnlyList<UnicamPoiCard>> GetPoiCardsAsync(
            UnicamPoiCategory category,
            string municipality,
            string language = "it",
            CancellationToken cancellationToken = default
        );

        Task<IReadOnlyList<UnicamEventCard>> GetEventsAsync(
            string municipality,
            string? title = null,
            DateOnly? date = null,
            string language = "it",
            CancellationToken cancellationToken = default
        );

        Task<IReadOnlyList<UnicamOrganizationCard>> GetOrganizationsAsync(
            string municipality,
            string language = "it",
            CancellationToken cancellationToken = default
        );

        Task<IReadOnlyList<UnicamRouteCard>> GetRoutesAsync(
            string municipality,
            string language = "it",
            CancellationToken cancellationToken = default
        );
    }

    public class UnicamApiService : IUnicamApiService
    {
        private const string DefaultLanguage = "it";

        private static readonly IReadOnlyDictionary<UnicamPoiCategory, string> PoiEndpoints =
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

        public Task<IReadOnlyList<UnicamPoiCard>> GetPoiCardsAsync(
            UnicamPoiCategory category,
            string municipality,
            string language = DefaultLanguage,
            CancellationToken cancellationToken = default
        )
        {
            municipality = ValidateMunicipality(municipality);
            language = NormalizeLanguage(language);

            var relativePath = BuildQueryPath(
                PoiEndpoints[category],
                new Dictionary<string, string?>
                {
                    ["municipality"] = municipality,
                    ["language"] = language,
                }
            );

            return FetchListAsync<UnicamPoiCard>(relativePath, cancellationToken);
        }

        public Task<IReadOnlyList<UnicamEventCard>> GetEventsAsync(
            string municipality,
            string? title = null,
            DateOnly? date = null,
            string language = DefaultLanguage,
            CancellationToken cancellationToken = default
        )
        {
            municipality = ValidateMunicipality(municipality);
            language = NormalizeLanguage(language);

            var relativePath = BuildQueryPath(
                "/api/events/card-list",
                new Dictionary<string, string?>
                {
                    ["municipality"] = municipality,
                    ["language"] = language,
                    ["title"] = string.IsNullOrWhiteSpace(title) ? null : title,
                    ["date"] = date?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                }
            );

            return FetchListAsync<UnicamEventCard>(relativePath, cancellationToken);
        }

        public Task<IReadOnlyList<UnicamOrganizationCard>> GetOrganizationsAsync(
            string municipality,
            string language = DefaultLanguage,
            CancellationToken cancellationToken = default
        )
        {
            municipality = ValidateMunicipality(municipality);
            language = NormalizeLanguage(language);

            var relativePath = BuildQueryPath(
                "/api/organizations/card-list",
                new Dictionary<string, string?>
                {
                    ["municipality"] = municipality,
                    ["language"] = language,
                }
            );

            return FetchListAsync<UnicamOrganizationCard>(relativePath, cancellationToken);
        }

        public Task<IReadOnlyList<UnicamRouteCard>> GetRoutesAsync(
            string municipality,
            string language = DefaultLanguage,
            CancellationToken cancellationToken = default
        )
        {
            municipality = ValidateMunicipality(municipality);
            language = NormalizeLanguage(language);

            var relativePath = BuildQueryPath(
                "/api/routes/card-list",
                new Dictionary<string, string?>
                {
                    ["municipality"] = municipality,
                    ["language"] = language,
                }
            );

            return FetchListAsync<UnicamRouteCard>(relativePath, cancellationToken);
        }

        private async Task<IReadOnlyList<T>> FetchListAsync<T>(
            string relativePath,
            CancellationToken cancellationToken
        )
        {
            try
            {
                var result = await _proxy.FetchAsAsync<List<T>>(relativePath, cancellationToken);
                if (result is null)
                {
                    throw new UnicamApiException(
                        "Received empty response from Unicam API.",
                        relativePath
                    );
                }

                if (result.Count == 0)
                {
                    return Array.Empty<T>();
                }

                if (result.Any(item => item is null))
                {
                    throw new UnicamApiException(
                        "Received null items from Unicam API.",
                        relativePath
                    );
                }

                return result!;
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

        private static string BuildQueryPath(string route, IDictionary<string, string?> queryParams)
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

        private static string ValidateMunicipality(string municipality)
        {
            if (string.IsNullOrWhiteSpace(municipality))
            {
                throw new ArgumentException("Municipality is required", nameof(municipality));
            }

            return municipality.Trim();
        }

        private static string NormalizeLanguage(string language)
        {
            if (string.IsNullOrWhiteSpace(language))
            {
                return DefaultLanguage;
            }

            return language.Trim();
        }
    }
}
