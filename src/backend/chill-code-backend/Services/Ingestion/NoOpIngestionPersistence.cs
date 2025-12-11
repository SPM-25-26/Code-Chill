using System.Linq;
using System.Text.Json;
using ChillCodeBackend.Models.Unicam;
using Microsoft.Extensions.Logging;

namespace ChillCodeBackend.Services.Ingestion
{
    public class NoOpIngestionPersistence : IIngestionPersistence
    {
        private readonly ILogger<NoOpIngestionPersistence> _logger;

        public NoOpIngestionPersistence(ILogger<NoOpIngestionPersistence> logger)
        {
            _logger = logger;
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
            return Task.CompletedTask;
        }
    }
}
