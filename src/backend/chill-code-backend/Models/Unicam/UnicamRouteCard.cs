using System.Text.Json.Serialization;

namespace ChillCodeBackend.Models.Unicam
{
    public class UnicamRouteCard
    {
        [JsonPropertyName("entityId")]
        public string? EntityId { get; set; }

        [JsonPropertyName("entityName")]
        public string? EntityName { get; set; }

        [JsonPropertyName("imagePath")]
        public string? ImagePath { get; set; }

        [JsonPropertyName("badgeText")]
        public string? BadgeText { get; set; }

        [JsonPropertyName("address")]
        public string? Address { get; set; }
    }
}
