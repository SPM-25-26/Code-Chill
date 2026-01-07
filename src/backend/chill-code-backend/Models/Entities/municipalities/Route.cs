using System;
using System.Collections.Generic;

namespace ChillCodeBackend.Models.Entities
{
    public class Route
    {
        public string ExternalId { get; set; } = string.Empty;

        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public DateTime? SourceLastUpdated { get; set; }

        public string Language { get; set; } = "it";

        public string Name { get; set; } = string.Empty;

        public string ImagePath { get; set; } = string.Empty;

        public string Number { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string PathTheme { get; set; } = string.Empty;

        public string TravellingMethod { get; set; } = string.Empty;

        public string ShortName { get; set; } = string.Empty;

        public string? OrganizationWebsite { get; set; }

        public string? OrganizationEmail { get; set; }

        public string? OrganizationFacebook { get; set; }

        public string? OrganizationInstagram { get; set; }

        public string? OrganizationTelephone { get; set; }

        public string? Website { get; set; }

        public string? SecurityLevel { get; set; }

        public int NumberOfStages { get; set; }

        public int QuantifiedPathwayPaving { get; set; }

        public string? Duration { get; set; }

        public string? RouteLength { get; set; }

        public List<string> Gallery { get; set; } = [];

        public List<string> VirtualTours { get; set; } = [];

        public List<StageMobile> Stages { get; set; } = [];

        public List<FeatureCard> StagesPoi { get; set; } = [];

        public Point StartingPoint { get; set; } = new Point();

        public List<string> BestWhen { get; set; } = [];

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public MunicipalityInfo MunicipalityData { get; set; } = new MunicipalityInfo();
    }

    public class StageMobile
    {
        public string Category { get; set; } = string.Empty;

        public string PoiIdentifier { get; set; } = string.Empty;

        public string PoiOfficialName { get; set; } = string.Empty;

        public string PoiImagePath { get; set; } = string.Empty;

        public string PoiImageThumbPath { get; set; } = string.Empty;

        public string Signposting { get; set; } = string.Empty;

        public string SupportService { get; set; } = string.Empty;

        public double PoiLatitude { get; set; }

        public double PoiLongitude { get; set; }

        public string PoiAddress { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public int Number { get; set; }

        public string? Description { get; set; }
    }

    public class FeatureCard
    {
        public string? EntityId { get; set; }

        public string? Title { get; set; }

        public MobileCategory? Category { get; set; }

        public string? ImagePath { get; set; }

        public string? ExtraInfo { get; set; }
    }

    public class Point
    {
        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public string? Address { get; set; }
    }
}
