using System.Text.Json;
using ChillCodeBackend.Models.Entities;
using ChillCodeBackend.Models.Unicam;

namespace ChillCodeBackend.Services.Ingestion
{
    public interface IIngestionPersistence
    {
        Task PersistMunicipalityVisitAsync(
            string municipalityExternalId,
            UnicamMunicipalityVisit municipalityVisit,
            CancellationToken cancellationToken
        );
        Task PersistPoiDetailsAsync(
            IEnumerable<JsonElement> items,
            UnicamPoiCategory category,
            MunicipalityInfo municipalityInfo,
            string language,
            CancellationToken cancellationToken
        );
        Task PersistEventDetailsAsync(
            IEnumerable<JsonElement> items,
            MunicipalityInfo municipalityInfo,
            string language,
            CancellationToken cancellationToken
        );
        Task PersistOrganizationDetailsAsync(
            IEnumerable<JsonElement> items,
            CancellationToken cancellationToken
        );
        Task PersistRouteDetailsAsync(
            IEnumerable<JsonElement> items,
            MunicipalityInfo municipalityInfo,
            string language,
            CancellationToken cancellationToken
        );
        Task PersistArticleDetailsAsync(
            IEnumerable<JsonElement> items,
            MunicipalityInfo municipalityInfo,
            string language,
            CancellationToken cancellationToken
        );
    }
}
