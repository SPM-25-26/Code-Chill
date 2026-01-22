using System.Text.Json;
using ChillCodeBackend.Models.Unicam;

namespace ChillCodeBackend.Models.Ingestion
{
    public class IngestionResult
    {
        public string Municipality { get; set; } = string.Empty;
        public string Language { get; set; } = "it";
        public DateTimeOffset StartedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? CompletedAt { get; set; }
        public int TotalPoiCount { get; set; }
        public Dictionary<UnicamPoiCategory, int> PoiCounts { get; } = new();
        public int EventCount { get; set; }
        public int OrganizationCount { get; set; }
        public int ServiceCount { get; set; }
        public int RouteCount { get; set; }
        public int ArticleCount { get; set; }
        public List<string> Errors { get; } = new();
        public List<JsonElement> PoiDetails { get; } = new();
        public List<JsonElement> EventDetails { get; } = new();
        public List<JsonElement> OrganizationDetails { get; } = new();
        public List<JsonElement> RouteDetails { get; } = new();
        public List<JsonElement> ArticleDetails { get; } = new();
    }
}
