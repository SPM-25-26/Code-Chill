namespace ChillCodeBackend.Models.Entities
{
    /// <summary>
    /// Generic Point of Interest entity for various POI categories
    /// (ArtCulture, EatAndDrink, EntertainmentLeisure, Nature, Services, Shopping, Sleep, TypicalProducts)
    /// </summary>
    public class PointOfInterest
    {
        public string Identifier { get; set; } = string.Empty;

        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public DateTime? SourceLastUpdated { get; set; }

        public string Language { get; set; } = "it";

        // Category type (Sleep, EatAndDrink, Services, etc.)
        public MobileCategory Category { get; set; }

        public string Title { get; set; } = string.Empty;
        public string OfficialName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Typology { get; set; } = string.Empty;

        public string PrimaryImage { get; set; } = string.Empty;
        public string ImageThumbPath { get; set; } = string.Empty;

        public List<string> Gallery { get; set; } = [];
        public List<string> VirtualTours { get; set; } = [];

        public string Email { get; set; } = string.Empty;
        public string Telephone { get; set; } = string.Empty;
        public string Website { get; set; } = string.Empty;
        public string Facebook { get; set; } = string.Empty;
        public string Instagram { get; set; } = string.Empty;

        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        public List<NeighborSummary> Neighbors { get; set; } = new();

        public CarParkInfo? NearestCarPark { get; set; }

        // Opening hours and accessibility
        public List<OpeningHoursPeriod> OpeningHours { get; set; } = new();
        public string Accessibility { get; set; } = string.Empty;

        // Prices and services
        public List<PriceInfo> Prices { get; set; } = new();
        public List<string> Services { get; set; } = new();
        public List<string> Facilities { get; set; } = new();

        // Additional metadata
        public string? OrganizationTaxCode { get; set; }
        public string? OrganizationLegalName { get; set; }

        public MunicipalityInfo MunicipalityData { get; set; } = new();
    }

    public class OpeningHoursPeriod
    {
        public string DayOfWeek { get; set; } = string.Empty;
        public string Opens { get; set; } = string.Empty;
        public string Closes { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class PriceInfo
    {
        public string Description { get; set; } = string.Empty;
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public string Currency { get; set; } = "EUR";
        public string PriceType { get; set; } = string.Empty;
    }
}
