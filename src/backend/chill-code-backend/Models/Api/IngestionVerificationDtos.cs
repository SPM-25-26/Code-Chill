namespace ChillCodeBackend.Models.Api
{
    public class MunicipalitySummaryDto
    {
        public string Municipality { get; set; } = string.Empty;
        public int ArticleCount { get; set; }
        public int EventCount { get; set; }
        public int RouteCount { get; set; }
        public int UnassignedArticleCount { get; set; }
        public int UnassignedEventCount { get; set; }
        public int UnassignedRouteCount { get; set; }
        public bool MunicipalityProfileExists { get; set; }
        public List<IngestionRunDto> Runs { get; set; } = new();
    }

    public class IngestionRunDto
    {
        public int Id { get; set; }
        public string Municipality { get; set; } = string.Empty;
        public string Language { get; set; } = "it";
        public DateTimeOffset StartedAt { get; set; }
        public DateTimeOffset? CompletedAt { get; set; }
        public int TotalPoiCount { get; set; }
        public int EventCount { get; set; }
        public int OrganizationCount { get; set; }
        public int RouteCount { get; set; }
        public int ArticleCount { get; set; }
        public List<string> Errors { get; set; } = new();
    }
}
