using ChillCodeBackend.Data.Db;
using ChillCodeBackend.Models.Api;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChillCodeBackend.Controllers
{
    [ApiController]
    [Route("api/admin/ingestion")]
    public class AdminIngestionVerificationController : ControllerBase
    {
        private readonly ChillCodeDbContext _dbContext;

        public AdminIngestionVerificationController(ChillCodeDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet("municipalities")]
        public async Task<ActionResult<List<string>>> GetMunicipalities(
            CancellationToken cancellationToken
        )
        {
            var names = await _dbContext
                .Municipalities.AsNoTracking()
                .Select(m => m.Name)
                .OrderBy(n => n)
                .ToListAsync(cancellationToken);

            return Ok(names);
        }

        [HttpGet("municipalities/{municipality}/summary")]
        public async Task<ActionResult<MunicipalitySummaryDto>> GetMunicipalitySummary(
            [FromRoute] string municipality,
            CancellationToken cancellationToken
        )
        {
            if (string.IsNullOrWhiteSpace(municipality))
            {
                return BadRequest("Municipality is required.");
            }

            var normalized = municipality.Trim();

            var municipalityProfileExists = await _dbContext
                .Municipalities.AsNoTracking()
                .AnyAsync(m => m.Name == normalized, cancellationToken);

            var articleCount = await _dbContext
                .Articles.AsNoTracking()
                .CountAsync(a => a.MunicipalityData.Name == normalized, cancellationToken);

            var unassignedArticleCount = await _dbContext
                .Articles.AsNoTracking()
                .CountAsync(a => a.MunicipalityData.Name == string.Empty, cancellationToken);

            var eventCount = await _dbContext
                .Events.AsNoTracking()
                .CountAsync(e => e.MunicipalityData.Name == normalized, cancellationToken);

            var unassignedEventCount = await _dbContext
                .Events.AsNoTracking()
                .CountAsync(e => e.MunicipalityData.Name == string.Empty, cancellationToken);

            var organizationCount = await _dbContext
                .Organizations.AsNoTracking()
                .CountAsync(o => o.MunicipalityData.Name == normalized, cancellationToken);

            var unassignedOrganizationCount = await _dbContext
                .Organizations.AsNoTracking()
                .CountAsync(o => o.MunicipalityData.Name == string.Empty, cancellationToken);

            var serviceCount = await _dbContext
                .Services.AsNoTracking()
                .CountAsync(s => s.MunicipalityData.Name == normalized, cancellationToken);

            var unassignedServiceCount = await _dbContext
                .Services.AsNoTracking()
                .CountAsync(s => s.MunicipalityData.Name == string.Empty, cancellationToken);

            var routeCount = await _dbContext
                .Routes.AsNoTracking()
                .CountAsync(r => r.MunicipalityData.Name == normalized, cancellationToken);

            var unassignedRouteCount = await _dbContext
                .Routes.AsNoTracking()
                .CountAsync(r => r.MunicipalityData.Name == string.Empty, cancellationToken);

            var runs = await _dbContext
                .IngestionRuns.AsNoTracking()
                .Where(r => r.Municipality == normalized)
                .Select(r => new IngestionRunDto
                {
                    Id = r.Id,
                    Municipality = r.Municipality,
                    Language = r.Language,
                    StartedAt = r.StartedAt,
                    CompletedAt = r.CompletedAt,
                    TotalPoiCount = r.TotalPoiCount,
                    EventCount = r.EventCount,
                    OrganizationCount = r.OrganizationCount,
                    RouteCount = r.RouteCount,
                    ArticleCount = r.ArticleCount,
                    Errors = r.Errors,
                })
                .ToListAsync(cancellationToken);
            // SQLite provider doesn't support ordering by DateTimeOffset; sort in-memory instead.
            runs = runs.OrderByDescending(r => r.CompletedAt ?? r.StartedAt).ToList();

            return Ok(
                new MunicipalitySummaryDto
                {
                    Municipality = normalized,
                    MunicipalityProfileExists = municipalityProfileExists,
                    ArticleCount = articleCount,
                    EventCount = eventCount,
                    OrganizationCount = organizationCount,
                    ServiceCount = serviceCount,
                    RouteCount = routeCount,
                    UnassignedArticleCount = unassignedArticleCount,
                    UnassignedEventCount = unassignedEventCount,
                    UnassignedOrganizationCount = unassignedOrganizationCount,
                    UnassignedServiceCount = unassignedServiceCount,
                    UnassignedRouteCount = unassignedRouteCount,
                    Runs = runs,
                }
            );
        }
    }
}
