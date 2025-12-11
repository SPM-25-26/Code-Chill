using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ChillCodeBackend.Models.Entities
{
    public class Route
    {
        public string ImagePath { get; set; } = string.Empty;

        public string Number { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

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
        [Required]
        public string Category { get; set; }

        [Required]
        public string PoiIdentifier { get; set; }

        [Required]
        public string PoiOfficialName { get; set; }

        [Required]
        public string PoiImagePath { get; set; }

        [Required]
        public string PoiImageThumbPath { get; set; }

        [Required]
        public string Signposting { get; set; }

        [Required]
        public string SupportService { get; set; }

        [Required]
        public double PoiLatitude { get; set; }

        [Required]
        public double PoiLongitude { get; set; }

        [Required]
        public string PoiAddress { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
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
        [Required]
        public double Latitude { get; set; }

        [Required]
        public double Longitude { get; set; }

        public string? Address { get; set; }
    }

    public class MunicipalityForLocalStorageSetting
    {
        public string? Name { get; set; }

        public string? LogoPath { get; set; }
    }

    public enum MobileCategory
    {
        Sleep,
        EatAndDrink,
        Events,
        ArtCulture,
        Nature,
        TypicalProducts,
        Routes,
        Services,
        EntertainmentLeisure,
        Organizations,
        Articles,
        Shopping
    }
}