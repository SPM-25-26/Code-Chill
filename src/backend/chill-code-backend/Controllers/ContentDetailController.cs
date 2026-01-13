using ChillCodeBackend.Data.Db;
using ChillCodeBackend.Models.Api;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChillCodeBackend.Controllers
{
    [ApiController]
    [Route("api/pois")]
    public class ContentDetailController : ControllerBase
    {
        private readonly ChillCodeDbContext _dbContext;

        public ContentDetailController(ChillCodeDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ContentDetailDto>> GetDetail(
            [FromRoute] string id,
            CancellationToken ct
        )
        {
            // 1️⃣ ARTICLE
            var article = await _dbContext.Articles
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Identifier == id, ct);

            if (article != null)
            {
                return Ok(ContentDetailDto.FromArticle(article));
            }

            // 2️⃣ EVENT
            var ev = await _dbContext.Events
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Identifier == id, ct);

            if (ev != null)
            {
                return Ok(ContentDetailDto.FromEvent(ev));
            }

            // 3️⃣ ROUTE
            var route = await _dbContext.Routes
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.ExternalId == id, ct);

            if (route != null)
            {
                return Ok(ContentDetailDto.FromRoute(route));
            }

            // 4️⃣ POI
            var poi = await _dbContext.PointsOfInterest
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Identifier == id, ct);

            if (poi != null)
            {
                return Ok(ContentDetailDto.FromPoi(poi));
            }

            return NotFound($"Content with id '{id}' not found.");
        }
    }
}
