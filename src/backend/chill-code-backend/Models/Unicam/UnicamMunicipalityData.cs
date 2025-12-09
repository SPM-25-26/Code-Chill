using System.Text.Json.Serialization;

namespace ChillCodeBackend.Models.Unicam
{
    public class UnicamMunicipalityData
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("logoPath")]
        public string? LogoPath { get; set; }
    }
}
