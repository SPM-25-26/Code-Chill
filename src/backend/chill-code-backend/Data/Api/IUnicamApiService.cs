using System.Text.Json;
using ChillCodeBackend.Models.Unicam;

namespace ChillCodeBackend.Data.Api
{
    public interface IUnicamApiService
    {
        Task<JsonElement> GetPoiDetailAsync(
            UnicamPoiCategory category,
            string identifier,
            string language = "it",
            CancellationToken cancellationToken = default
        );

        Task<JsonElement> GetEventDetailAsync(
            string identifier,
            string language = "it",
            CancellationToken cancellationToken = default
        );

        Task<JsonElement> GetOrganizationDetailAsync(
            string taxCode,
            string language = "it",
            CancellationToken cancellationToken = default
        );

        Task<JsonElement> GetServiceDetailAsync(
            string identifier,
            string language = "it",
            CancellationToken cancellationToken = default
        );

        Task<JsonElement> GetRouteDetailAsync(
            string identifier,
            string language = "it",
            CancellationToken cancellationToken = default
        );

        Task<JsonElement> GetArticleDetailAsync(
            string identifier,
            string language = "it",
            CancellationToken cancellationToken = default
        );

        Task<UnicamMunicipalityVisit> GetMunicipalityVisitAsync(
            string municipality,
            string language = "it",
            CancellationToken cancellationToken = default
        );

        // Card endpoints to get lists of POIs by municipality
        Task<JsonElement> GetPoiCardsAsync(
            UnicamPoiCategory category,
            string municipality,
            string language = "it",
            CancellationToken cancellationToken = default
        );

        // Organization card list endpoint
        Task<JsonElement> GetOrganizationCardsAsync(
            string municipality,
            string language = "it",
            CancellationToken cancellationToken = default
        );

        // Service card list endpoint
        Task<JsonElement> GetServiceCardsAsync(
            string municipality,
            string language = "it",
            CancellationToken cancellationToken = default
        );
    }
}
