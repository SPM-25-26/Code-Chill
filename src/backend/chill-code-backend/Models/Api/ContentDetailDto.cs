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
        public string? Region { get; set; }

        public string? Image { get; set; }
        public List<string> Gallery { get; set; } = new();
        public List<string> VirtualTours { get; set; } = new();

        public string? Website { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Facebook { get; set; }
        public string? Instagram { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        // Article-specific
        public string? TimeToRead { get; set; }
        public List<string> Themes { get; set; } = new();
        public List<ArticleParagraphDto> Paragraphs { get; set; } = new();

        // Event-specific
        public string? Typology { get; set; }
        public string? Audience { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        // Route-specific
        public string? Duration { get; set; }
        public string? RouteLength { get; set; }
        public string? PathTheme { get; set; }
        public string? TravellingMethod { get; set; }
        public int? NumberOfStages { get; set; }
        public List<string> BestWhen { get; set; } = new();

        // Municipality info
        public string? MunicipalityName { get; set; }
        public string? MunicipalityLogo { get; set; }

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
                Region = a.Region,
                TimeToRead = a.TimeToRead,
                Themes = a.Themes ?? new List<string>(),
                Paragraphs = a.Paragraphs?.OrderBy(p => p.Position).Select(p => new ArticleParagraphDto
                {
                    Title = p.Title,
                    Script = p.Script,
                    Position = p.Position,
                    Subtitle = p.Subtitle
                }).ToList() ?? new List<ArticleParagraphDto>(),
                MunicipalityName = a.MunicipalityData?.Name,
                MunicipalityLogo = a.MunicipalityData?.LogoPath
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
                VirtualTours = e.VirtualTours ?? new List<string>(),
                Website = string.IsNullOrWhiteSpace(e.Website) ? null : e.Website,
                Phone = string.IsNullOrWhiteSpace(e.Telephone) ? null : e.Telephone,
                Email = string.IsNullOrWhiteSpace(e.Email) ? null : e.Email,
                Facebook = string.IsNullOrWhiteSpace(e.Facebook) ? null : e.Facebook,
                Instagram = string.IsNullOrWhiteSpace(e.Instagram) ? null : e.Instagram,
                StartDate = e.StartDate,
                EndDate = e.EndDate,
                Typology = e.Typology,
                Audience = e.Audience,
                Latitude = e.Latitude,
                Longitude = e.Longitude,
                MunicipalityName = e.MunicipalityData?.Name,
                MunicipalityLogo = e.MunicipalityData?.LogoPath
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
                VirtualTours = r.VirtualTours ?? new List<string>(),
                Website = string.IsNullOrWhiteSpace(r.Website) ? null : r.Website,
                Phone = string.IsNullOrWhiteSpace(r.OrganizationTelephone) ? null : r.OrganizationTelephone,
                Email = string.IsNullOrWhiteSpace(r.OrganizationEmail) ? null : r.OrganizationEmail,
                Facebook = string.IsNullOrWhiteSpace(r.OrganizationFacebook) ? null : r.OrganizationFacebook,
                Instagram = string.IsNullOrWhiteSpace(r.OrganizationInstagram) ? null : r.OrganizationInstagram,
                StartDate = r.StartDate,
                EndDate = r.EndDate,
                Address = r.StartingPoint?.Address,
                Duration = r.Duration,
                RouteLength = r.RouteLength,
                PathTheme = r.PathTheme,
                TravellingMethod = r.TravellingMethod,
                NumberOfStages = r.NumberOfStages,
                BestWhen = r.BestWhen ?? new List<string>(),
                Latitude = r.StartingPoint?.Latitude,
                Longitude = r.StartingPoint?.Longitude,
                MunicipalityName = r.MunicipalityData?.Name,
                MunicipalityLogo = r.MunicipalityData?.LogoPath
            };
        }
    }

    public class ArticleParagraphDto
    {
        public string Title { get; set; } = string.Empty;
        public string Script { get; set; } = string.Empty;
        public int Position { get; set; }
        public string? Subtitle { get; set; }
    }
}
