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
    }
}
