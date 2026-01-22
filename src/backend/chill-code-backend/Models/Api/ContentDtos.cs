namespace ChillCodeBackend.Models.Api
{
    public class MunicipalityContentDto
    {
        public string Municipality { get; set; } = string.Empty;
        public List<ArticleDto> Articles { get; set; } = new();
        public List<EventDto> Events { get; set; } = new();
        public List<RouteDto> Routes { get; set; } = new();
        public List<PoiDto> Pois { get; set; } = new();
        public List<OrganizationCardDto> Organizations { get; set; } = new();
        public List<ServiceCardDto> Services { get; set; } = new();
    }

    public class ArticleDto
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Subtitle { get; set; } = string.Empty;
        public string Region { get; set; } = string.Empty;
        public string ImagePath { get; set; } = string.Empty;
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }

    public class EventDto
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string PrimaryImage { get; set; } = string.Empty;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }

    public class RouteDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string PathTheme { get; set; } = string.Empty;
        public string TravellingMethod { get; set; } = string.Empty;
        public string Duration { get; set; } = string.Empty;
        public string RouteLength { get; set; } = string.Empty;
        public string ImagePath { get; set; } = string.Empty;
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }

    public class PoiDto
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Typology { get; set; } = string.Empty;
        public string PrimaryImage { get; set; } = string.Empty;
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }
}
