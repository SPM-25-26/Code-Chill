using ChillCodeBackend.Models.Entities;

namespace ChillCodeBackend.Models.Api
{
    public class ContentDetailDto
    {
        public string Id { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // article | event | route

        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public string? Subtitle { get; set; }
        public string? Address { get; set; }

        public string? Image { get; set; }
        public List<string> Gallery { get; set; } = new();

        public string? Website { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        /* ================== FACTORY METHODS ================== */

        public static ContentDetailDto FromArticle(Article a)
        {
            return new ContentDetailDto
            {
                Id = a.Identifier,
                Type = "article",
                Title = a.Title,
                Subtitle = a.Subtitle,
                Description = a.Script,
                Image = a.ImagePath,
            };
        }

        public static ContentDetailDto FromEvent(Event e)
        {
            return new ContentDetailDto
            {
                Id = e.Identifier,
                Type = "event",
                Title = e.Title,
                Description = e.Description,
                Address = e.Address,
                Image = e.PrimaryImage,
                Gallery = e.Gallery,
                Website = e.Website,
                Phone = e.Telephone,
                Email = e.Email,
                StartDate = e.StartDate,
                EndDate = e.EndDate
            };
        }

        public static ContentDetailDto FromRoute(Entities.Route r)
        {
            return new ContentDetailDto
            {
                Id = r.ExternalId,
                Type = "route",
                Title = r.Name,
                Description = r.Description,
                Image = r.ImagePath,
                Gallery = r.Gallery,
                Website = r.Website,
                Phone = r.OrganizationTelephone,
                Email = r.OrganizationEmail
            };
        }
    }
}
