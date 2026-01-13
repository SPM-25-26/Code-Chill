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

        // 🔹 Lista di tutti i nomi di comuni presenti nei contenuti
        [HttpGet("municipalities")]
        public async Task<ActionResult<List<string>>> GetMunicipalities(CancellationToken ct)
        {
            var articleNames = _dbContext.Articles.Select(a => a.MunicipalityData.Name);
            var eventNames = _dbContext.Events.Select(e => e.MunicipalityData.Name);
            var routeNames = _dbContext.Routes.Select(r => r.MunicipalityData.Name);
            var poiNames = _dbContext.PointsOfInterest.Select(p => p.MunicipalityData.Name);
            var muniNames = _dbContext.Municipalities.Select(m => m.Name);

            var names = await articleNames
                .Concat(eventNames)
                .Concat(routeNames)
                .Concat(poiNames)
                .Concat(muniNames)
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .Distinct()
                .OrderBy(n => n)
                .ToListAsync(ct);

            return Ok(names);
        }

        // 🔹 Contenuti di un singolo comune
        [HttpGet("municipalities/{municipality}")]
        public async Task<ActionResult<MunicipalityContentDto>> GetContent(
            [FromRoute] string municipality,
            [FromQuery] string? language,
            CancellationToken ct
        )
        {
            if (string.IsNullOrWhiteSpace(municipality))
                return BadRequest("Municipality is required.");

            var normalized = municipality.Trim();
            var lang = string.IsNullOrWhiteSpace(language) ? "it" : language.Trim();

            // Articles
            var articles = await _dbContext.Articles
                .AsNoTracking()
                .Where(a => a.MunicipalityData.Name == normalized && a.Language == lang)
                .Select(a => new ArticleDto
                {
                    Id = a.Identifier,
                    Title = a.Title,
                    Subtitle = a.Subtitle,
                    Region = a.Region,
                    ImagePath = a.ImagePath
                })
                .ToListAsync(ct);

            // Events
            var events = await _dbContext.Events
                .AsNoTracking()
                .Where(e => e.MunicipalityData.Name == normalized && e.Language == lang)
                .Select(e => new EventDto
                {
                    Id = e.Identifier,
                    Title = e.Title,
                    Description = e.Description,
                    Address = e.Address,
                    PrimaryImage = e.PrimaryImage,
                    StartDate = e.StartDate,
                    EndDate = e.EndDate
                })
                .ToListAsync(ct);

            // Routes
            var routes = await _dbContext.Routes
                .AsNoTracking()
                .Where(r => r.MunicipalityData.Name == normalized && r.Language == lang)
                .Select(r => new RouteDto
                {
                    Id = string.IsNullOrEmpty(r.ExternalId) ? r.Name : r.ExternalId,
                    Name = r.Name,
                    Description = r.Description,
                    PathTheme = r.PathTheme,
                    TravellingMethod = r.TravellingMethod,
                    Duration = r.Duration ?? string.Empty,
                    RouteLength = r.RouteLength ?? string.Empty,
                    ImagePath = r.ImagePath
                })
                .ToListAsync(ct);

            // POIs
            var pois = await _dbContext.PointsOfInterest
                .AsNoTracking()
                .Where(p => p.MunicipalityData.Name == normalized && p.Language == lang)
                .Select(p => new PoiDto
                {
                    Id = p.Identifier,
                    Title = !string.IsNullOrWhiteSpace(p.Title) ? p.Title : p.OfficialName,
                    Category = p.Category.ToString(),
                    Description = p.Description,
                    Address = p.Address,
                    Typology = p.Typology,
                    PrimaryImage = p.PrimaryImage
                })
                .ToListAsync(ct);

            return Ok(new MunicipalityContentDto
            {
                Municipality = normalized,
                Articles = articles,
                Events = events,
                Routes = routes,
                Pois = pois
            });
        }
    }
}
