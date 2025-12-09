using ChillCodeBackend.Models.Ingestion;

namespace ChillCodeBackend.Services.Ingestion
{
    public interface IIngestionOrchestrator
    {
        Task<IngestionResult> IngestMunicipalityAsync(
            string municipality,
            string language = "it",
            CancellationToken cancellationToken = default
        );
    }
}
