using System.Text.Json;
using ChillCodeBackend.Models.Unicam;

namespace ChillCodeBackend.Models.Api
{
    public class IngestionResponseDto
    {
        public string Municipality { get; set; } = string.Empty;
        public string Language { get; set; } = "it";
        public DateTimeOffset StartedAt { get; set; }
        public DateTimeOffset? CompletedAt { get; set; }
        public int TotalPoiCount { get; set; }
        public Dictionary<UnicamPoiCategory, int> PoiCounts { get; set; } = new();
        public int EventCount { get; set; }
        public int OrganizationCount { get; set; }
        public int RouteCount { get; set; }
        public int ArticleCount { get; set; }
        public List<string> Errors { get; set; } = new();
        public List<JsonElement> PoiDetails { get; set; } = new();
        public List<JsonElement> EventDetails { get; set; } = new();
        public List<JsonElement> OrganizationDetails { get; set; } = new();
        public List<JsonElement> RouteDetails { get; set; } = new();
        public List<JsonElement> ArticleDetails { get; set; } = new();
    }
}
