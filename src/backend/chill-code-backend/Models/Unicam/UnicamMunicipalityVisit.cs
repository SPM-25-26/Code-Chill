using System.Text.Json.Serialization;

namespace ChillCodeBackend.Models.Unicam
{
    public class UnicamMunicipalityVisit
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("legalName")]
        public string? LegalName { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("contacts")]
        public UnicamMunicipalityVisitContacts? Contacts { get; set; }

        [JsonPropertyName("latitude")]
        public double? Latitude { get; set; }

        [JsonPropertyName("longitude")]
        public double? Longitude { get; set; }

        [JsonPropertyName("logoPath")]
        public string? LogoPath { get; set; }

        [JsonPropertyName("homeImages")]
        public List<string> HomeImages { get; set; } = new();

        [JsonPropertyName("events")]
        public List<UnicamMunicipalityVisitHighlight> Events { get; set; } = new();

        [JsonPropertyName("articlesAndPaths")]
        public List<UnicamMunicipalityVisitHighlight> ArticlesAndPaths { get; set; } = new();

        [JsonPropertyName("panoramaPath")]
        public string? PanoramaPath { get; set; }

        [JsonPropertyName("panoramaWidth")]
        public int? PanoramaWidth { get; set; }

        [JsonPropertyName("virtualTourUrls")]
        public List<string> VirtualTourUrls { get; set; } = new();

        [JsonPropertyName("nameAndProvince")]
        public string? NameAndProvince { get; set; }
    }

    public class UnicamMunicipalityVisitContacts
    {
        [JsonPropertyName("email")]
        public string? Email { get; set; }

        [JsonPropertyName("telephone")]
        public string? Telephone { get; set; }

        [JsonPropertyName("website")]
        public string? Website { get; set; }

        [JsonPropertyName("facebook")]
        public string? Facebook { get; set; }

        [JsonPropertyName("instagram")]
        public string? Instagram { get; set; }
    }

    public class UnicamMunicipalityVisitHighlight
    {
        [JsonPropertyName("entityId")]
        public string? EntityId { get; set; }

        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("category")]
        public string? Category { get; set; }

        [JsonPropertyName("imagePath")]
        public string? ImagePath { get; set; }

        [JsonPropertyName("extraInfo")]
        public string? ExtraInfo { get; set; }
    }
}
