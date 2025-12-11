using System.Text.Json;
using ChillCodeBackend.Models.Api;
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

        public AdminIngestionController(IIngestionOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
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

            if (result.Errors.Count > 0)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, Map(result));
            }

            return Ok(Map(result));
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
                RouteCount = result.RouteCount,
                ArticleCount = result.ArticleCount,
                PoiDetails = new List<System.Text.Json.JsonElement>(result.PoiDetails),
                EventDetails = new List<System.Text.Json.JsonElement>(result.EventDetails),
                OrganizationDetails = new List<System.Text.Json.JsonElement>(
                    result.OrganizationDetails
                ),
                RouteDetails = new List<System.Text.Json.JsonElement>(result.RouteDetails),
                ArticleDetails = new List<System.Text.Json.JsonElement>(result.ArticleDetails),
                Errors = new List<string>(result.Errors),
            };
        }
    }
}
