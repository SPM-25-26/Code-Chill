using System.Text.Json;
using ChillCodeBackend.Data.Db;
using ChillCodeBackend.Models.Api;
using ChillCodeBackend.Models.Entities;
using ChillCodeBackend.Models.Ingestion;
using ChillCodeBackend.Models.Unicam;
using ChillCodeBackend.Services.Ingestion;
using Microsoft.AspNetCore.Mvc;

namespace ChillCodeBackend.Controllers
{
    [ApiController]
    [Route("api/admin/ingest")]
    public class AdminIngestionController : ControllerBase
    {
        private readonly IIngestionOrchestrator _orchestrator;
        private readonly ChillCodeDbContext _dbContext;

        public AdminIngestionController(
            IIngestionOrchestrator orchestrator,
            ChillCodeDbContext dbContext
        )
        {
            _orchestrator = orchestrator;
            _dbContext = dbContext;
        }

        [HttpPost]
        public async Task<ActionResult<IngestionResponseDto>> TriggerIngestion(
            [FromBody] IngestionRequestDto request
        )
        {
            if (request is null || string.IsNullOrWhiteSpace(request.Municipality))
            {
                return BadRequest("Municipality is required.");
            }

            var result = await _orchestrator.IngestMunicipalityAsync(
                request.Municipality,
                request.Language ?? "it"
            );

            await PersistIngestionRunAsync(result, HttpContext.RequestAborted);

            var response = Map(result);

            // Return appropriate status code based on ingestion results:
            // - If there are errors but some data was successfully ingested, return 200 OK with errors in response
            // - If there are errors and NO data was ingested, return 500 Internal Server Error
            // - Otherwise return 200 OK
            if (result.Errors.Count > 0)
            {
                var hasAnyData =
                    result.TotalPoiCount > 0
                    || result.EventCount > 0
                    || result.RouteCount > 0
                    || result.ArticleCount > 0
                    || result.OrganizationCount > 0
                    || result.ServiceCount > 0;

                if (!hasAnyData)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, response);
                }
            }

            return Ok(response);
        }

        private async Task PersistIngestionRunAsync(
            IngestionResult result,
            CancellationToken cancellationToken
        )
        {
            try
            {
                var run = new IngestionRun
                {
                    Municipality = result.Municipality,
                    Language = result.Language,
                    StartedAt = result.StartedAt,
                    CompletedAt = result.CompletedAt,
                    TotalPoiCount = result.TotalPoiCount,
                    EventCount = result.EventCount,
                    OrganizationCount = result.OrganizationCount,
                    RouteCount = result.RouteCount,
                    ArticleCount = result.ArticleCount,
                    Errors = new List<string>(result.Errors),
                };

                _dbContext.IngestionRuns.Add(run);
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch
            {
                // Logging/auditing should never fail the ingestion response.
            }
        }

        private IngestionResponseDto Map(IngestionResult result)
        {
            return new IngestionResponseDto
            {
                Municipality = result.Municipality,
                Language = result.Language,
                StartedAt = result.StartedAt,
                CompletedAt = result.CompletedAt,
                TotalPoiCount = result.TotalPoiCount,
                PoiCounts = new Dictionary<UnicamPoiCategory, int>(result.PoiCounts),
                EventCount = result.EventCount,
                OrganizationCount = result.OrganizationCount,
                ServiceCount = result.ServiceCount,
                RouteCount = result.RouteCount,
                ArticleCount = result.ArticleCount,
                Errors = new List<string>(result.Errors),
            };
        }
    }
}
