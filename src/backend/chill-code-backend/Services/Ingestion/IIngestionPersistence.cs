using System.Text.Json;
using ChillCodeBackend.Models.Entities;
using ChillCodeBackend.Models.Unicam;

namespace ChillCodeBackend.Services.Ingestion
{
    public interface IIngestionPersistence
    {
        Task PersistMunicipalityVisitAsync(
            UnicamMunicipalityVisit municipalityVisit,
            CancellationToken cancellationToken
        );
        Task PersistPoiDetailsAsync(IEnumerable<JsonElement> items, CancellationToken cancellationToken);
        Task PersistEventDetailsAsync(
            IEnumerable<JsonElement> items,
            MunicipalityInfo municipalityInfo,
            CancellationToken cancellationToken
        );
        Task PersistOrganizationDetailsAsync(
            IEnumerable<JsonElement> items,
            CancellationToken cancellationToken
        );
        Task PersistRouteDetailsAsync(
            IEnumerable<JsonElement> items,
            MunicipalityInfo municipalityInfo,
            CancellationToken cancellationToken
        );
        Task PersistArticleDetailsAsync(
            IEnumerable<JsonElement> items,
            MunicipalityInfo municipalityInfo,
            CancellationToken cancellationToken
        );
    }
}
