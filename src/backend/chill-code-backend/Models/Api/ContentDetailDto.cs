using ChillCodeBackend.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

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
                // Article non ha Gallery, Address, Website, Phone, Email, Start/EndDate
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
                Gallery = e.Gallery ?? new List<string>(),
                Website = string.IsNullOrWhiteSpace(e.Website) ? null : e.Website,
                Phone = string.IsNullOrWhiteSpace(e.Telephone) ? null : e.Telephone,
                Email = string.IsNullOrWhiteSpace(e.Email) ? null : e.Email,
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
                Gallery = r.Gallery ?? new List<string>(),
                Website = string.IsNullOrWhiteSpace(r.Website) ? null : r.Website,
                Phone = string.IsNullOrWhiteSpace(r.OrganizationTelephone) ? null : r.OrganizationTelephone,
                Email = string.IsNullOrWhiteSpace(r.OrganizationEmail) ? null : r.OrganizationEmail,
                StartDate = r.StartDate,
                EndDate = r.EndDate,
                Address = r.StartingPoint?.Address
            };
        }
    }
}
