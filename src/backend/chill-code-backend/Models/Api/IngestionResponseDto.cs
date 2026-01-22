using ChillCodeBackend.Models.Unicam;

namespace ChillCodeBackend.Models.Api
{
    /// <summary>
    /// Response DTO for ingestion operations.
    /// Contains only summary information (counts and errors), not the full detail data.
    /// </summary>
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
        public int ServiceCount { get; set; }
        public int RouteCount { get; set; }
        public int ArticleCount { get; set; }
        public List<string> Errors { get; set; } = new();
    }
}
