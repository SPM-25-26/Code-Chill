using ChillCodeBackend.Data.Db;
using ChillCodeBackend.Models.Api;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChillCodeBackend.Controllers
{
    [ApiController]
    [Route("api/content")]
    public class ContentController : ControllerBase
    {
        private readonly ChillCodeDbContext _dbContext;

        public ContentController(ChillCodeDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet("municipalities")]
        public async Task<ActionResult<List<string>>> GetMunicipalities(CancellationToken ct)
        {
            // Distinct municipalities present in stored content or municipality table.
            var articleNames = _dbContext.Articles.Select(a => a.MunicipalityData.Name);
            var eventNames = _dbContext.Events.Select(e => e.MunicipalityData.Name);
            var routeNames = _dbContext.Routes.Select(r => r.MunicipalityData.Name);
            var muniNames = _dbContext.Municipalities.Select(m => m.Name);

            var names = await articleNames
                .Concat(eventNames)
                .Concat(routeNames)
                .Concat(muniNames)
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .Distinct()
                .OrderBy(n => n)
                .ToListAsync(ct);

            return Ok(names);
        }

        [HttpGet("municipalities/{municipality}")]
        public async Task<ActionResult<MunicipalityContentDto>> GetContent(
            [FromRoute] string municipality,
            CancellationToken ct
        )
        {
            if (string.IsNullOrWhiteSpace(municipality))
            {
                return BadRequest("Municipality is required.");
            }

            var normalized = municipality.Trim();

            var articles = await _dbContext
                .Articles.AsNoTracking()
                .Where(a => a.MunicipalityData.Name == normalized)
                .Select(a => new ArticleDto
                {
                    Id = a.Identifier,
                    Title = a.Title,
                    Subtitle = a.Subtitle,
                    Region = a.Region,
                    ImagePath = a.ImagePath,
                })
                .ToListAsync(ct);

            var events = await _dbContext
                .Events.AsNoTracking()
                .Where(e => e.MunicipalityData.Name == normalized)
                .Select(e => new EventDto
                {
                    Id = e.Identifier,
                    Title = e.Title,
                    Description = e.Description,
                    Address = e.Address,
                    PrimaryImage = e.PrimaryImage,
                    StartDate = e.StartDate,
                    EndDate = e.EndDate,
                })
                .ToListAsync(ct);

            var routes = await _dbContext
                .Routes.AsNoTracking()
                .Where(r => r.MunicipalityData.Name == normalized)
                .Select(r => new RouteDto
                {
                    Id = r.ExternalId ?? r.Name,
                    Name = r.Name,
                    Description = r.Description,
                    PathTheme = r.PathTheme,
                    TravellingMethod = r.TravellingMethod,
                    Duration = r.Duration ?? string.Empty,
                    RouteLength = r.RouteLength ?? string.Empty,
                    ImagePath = r.ImagePath,
                })
                .ToListAsync(ct);

            return Ok(
                new MunicipalityContentDto
                {
                    Municipality = normalized,
                    Articles = articles,
                    Events = events,
                    Routes = routes,
                }
            );
        }
    }
}

